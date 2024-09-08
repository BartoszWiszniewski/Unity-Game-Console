using System;
using System.Collections.Generic;
using System.Linq;
using Console.Types;

namespace Console.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public CommandKey CommandKey => new CommandKey(Command, CommandArguments.Count);
        public string Command { get; }
        public string Description { get; }
        public string Group { get; }
        public CommandTargetType Target { get; }
        public abstract bool IsStatic { get; }
        public Type TargetType { get; }
        public abstract IReadOnlyList<CommandArgument> CommandArguments { get; }

        protected BaseCommand(Type targetType, string command, string description, string group, CommandTargetType target)
        {
            TargetType = targetType;
            Command = command;
            Description = description;
            Group = group;
            Target = target;
        }
        
        public abstract object Execute(params object[] parameters);
        
        public virtual bool CanExecute(params object[] parameters)
        {
            if (parameters.Length > CommandArguments.Count)
            {
                return false;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                {
                    continue;
                }

                if (parameters[i].GetType() != CommandArguments[i].Type)
                {
                    return false;
                }
            }

            for (var i = parameters.Length; i < CommandArguments.Count; i++)
            {
                if (CommandArguments[i].DefaultValue is null)
                {
                    return false;
                }
            }

            return true;
        }
        
        protected object[] GetTargets()
        {
            switch (Target)
            {
                case CommandTargetType.Single:
                {
                    var target = UnityEngine.Object.FindObjectOfType(TargetType);
                    return target != null ? new object[] { target } : Array.Empty<object>();
                }
                case CommandTargetType.All:
                {
                    return UnityEngine.Object.FindObjectsOfType(TargetType).OfType<object>().ToArray();
                }
                case CommandTargetType.SingleIncludeInactive:
                {
                    var target = UnityEngine.Object.FindObjectsOfType(TargetType, true);
                    return target != null ? new object[] { target } : Array.Empty<object>();
                }
                case CommandTargetType.AllIncludeInactive:
                {
                    return UnityEngine.Object.FindObjectsOfType(TargetType, true).OfType<object>().ToArray();
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}