using System;
using System.Collections.Generic;
using System.Linq;
using Console.Commands;

namespace Console.Suggestions
{
    public class CommandSuggestion : ISuggestion
    {
        private readonly CommandCollection _commandCollection;
        public Type Type  => typeof(ICommand);
        
        public CommandSuggestion(CommandCollection commandCollection)
        {
            _commandCollection = commandCollection;
        }

        public List<string> GetSuggestions(Type type, string input)
        {
            return _commandCollection.Values.Where(x => x.Command.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Command)
                .ThenBy(x => x.CommandArguments.Count)
                .Select(command => $"{command.Command} {string.Join(" ", command.CommandArguments.Select(argument => argument.Name))} - {command.Description}")
                .ToList();
        }
    }
}