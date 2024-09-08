using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Console.Attributes;
using Unity.VisualScripting;
using UnityEngine;

namespace Console.Commands
{
    public class CommandCollection : IDictionary<CommandKey, ICommand>
    {
        private readonly IDictionary<CommandKey, ICommand> _commands = new Dictionary<CommandKey, ICommand>();
        
        public ICollection<CommandKey> Keys => _commands.Keys;
        public ICollection<ICommand> Values => _commands.Values;
        public int Count => _commands.Count;
        public bool IsReadOnly => _commands.IsReadOnly;
        
        public ICommand this[CommandKey key]
        {
            get => _commands[key];
            set => _commands[key] = value;
        }

        public CommandCollection()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // Get all types in the assembly
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    RegisterMethodCommands(type);
                    RegisterPropertyCommands(type);
                }
            }
        }

        private void RegisterMethodCommands(Type type)
        {
            // Get all methods in the type
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var method in methods)
            {
                // Check if the method has the CommandAttribute
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute == null) continue;
                        
                // Create a MethodCommand and add it to the dictionary
                var command = MethodCommand.Create(type, method, attribute);
                if (command == null)
                {
                    Debug.LogError($"Failed to create command for method {method.Name} command: {attribute.Command} in type {type.Name}");
                    continue;
                }
                        
                if(!_commands.TryAdd(command.CommandKey, command))
                {
                    Debug.LogError($"Failed to add command {command.CommandKey} to the command collection for type: {type.Name}");
                }
            }
        }

        private void RegisterPropertyCommands(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var propertyInfo in properties)
            {
                var attribute = propertyInfo.GetCustomAttribute<CommandAttribute>();
                if (attribute == null) continue;
                        
                foreach (var command in PropertyCommand.Create(type, propertyInfo, attribute))
                {
                    if(!_commands.TryAdd(command.CommandKey, command))
                    {
                        Debug.LogError($"Failed to add command {command.CommandKey} to the command collection for type: {type.Name}");
                    }
                }
            }
        }
        
        public IEnumerator<KeyValuePair<CommandKey, ICommand>> GetEnumerator()
        {
            return _commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<CommandKey, ICommand> item)
        {
            _commands.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _commands.Clear();
        }

        public bool Contains(KeyValuePair<CommandKey, ICommand> item)
        {
            return _commands.Contains(item);
        }

        public void CopyTo(KeyValuePair<CommandKey, ICommand>[] array, int arrayIndex)
        {
            _commands.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<CommandKey, ICommand> item)
        {
            return _commands.Remove(item.Key);
        }
        
        public void Add(CommandKey key, ICommand value)
        {
            _commands.Add(key, value);
        }

        public bool ContainsKey(CommandKey key)
        {
            return _commands.ContainsKey(key);
        }

        public bool Remove(CommandKey key)
        {
            return _commands.Remove(key);
        }

        public bool TryGetValue(CommandKey key, out ICommand value)
        {
            return _commands.TryGetValue(key, out value);
        }
        
        public IReadOnlyList<ICommand> GetCommands(string command)
        {
            return _commands.Values
                .Where(x => x.Command == command)
                .OrderBy(x => x.Command)
                .ThenBy(x => x.CommandArguments.Count)
                .ToList();
        }
        
        public IReadOnlyList<ICommand> GetGroupCommands(string group)
        {
            return _commands.Values
                .Where(x => x.Group == group)
                .OrderBy(x => x.Command)
                .ThenBy(x => x.CommandArguments.Count)
                .ToList();
        }
        
        public IDictionary<string, IReadOnlyList<ICommand>> GetGroupedCommands()
        {
            var groupedCommands = new Dictionary<string, List<ICommand>>();
            foreach (var item in _commands)
            {
                if(groupedCommands.TryGetValue(item.Value.Group, out var commands))
                {
                    commands.Add(item.Value);
                }
                else
                {
                    groupedCommands[item.Value.Group] = new List<ICommand> { item.Value };
                }
            }

            return groupedCommands.ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<ICommand>)(x.Value.OrderBy(y => y.Command)
                    .ThenBy(y => y.CommandArguments.Count)
                    .ToList()));
        }
        
        public void TryExecuteCommand(string command, params object[] parameters)
        {
            var commands = GetCommands(command);
            if (commands.Count == 0)
            {
                Debug.LogError($"No command found with the name {command}");
                return;
            }

            var commandToExecute = commands
                .Where(cmd => cmd.CanExecute(parameters))
                .OrderBy(x => Math.Abs(x.CommandArguments.Count - parameters.Length)) // Order by the closest match in terms of parameter count
                .ThenBy(x => x.CommandArguments.Count) // If same difference, prefer commands with fewer total arguments
                .FirstOrDefault();
            
            
            if (commandToExecute == null)
            {
                Debug.LogError($"No command found with the name {command} and fitting parameters");
                return;
            }
            
            commandToExecute.Execute(parameters);
        }
    }
}