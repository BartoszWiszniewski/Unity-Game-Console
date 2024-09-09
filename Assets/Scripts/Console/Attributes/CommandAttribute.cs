using System;
using System.Text.RegularExpressions;
using Console.Types;
using UnityEngine;

namespace Console.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public CommandTargetType Target { get; set; }

        public CommandAttribute(string command, string description = null, string group = null, CommandTargetType target = CommandTargetType.All)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                group = "Global";
            }
            
            Command = ProcessCommand(command);
            Description = description;
            Group = group;
            Target = target;
        }
        
        private static string ProcessCommand(string command)
        {
            var cachedCommand = command;
            // Trim leading and trailing white spaces
            command = command.Trim();

            // Remove special characters
            command = Regex.Replace(command, @"[(){}<>''"":,.'`]", string.Empty);
            
            // Replace white spaces between text with "-"
            command = Regex.Replace(command, @"\s+", "-");
            
            // Convert camel case to kebab case
            command = Regex.Replace(command, "(?<!^)([A-Z])", "-$1").ToLower();

            // Replace underscores with hyphens
            command = command.Replace('_', '-');

            // Replace multiple consecutive hyphens with a single hyphen
            command = Regex.Replace(command, "-{2,}", "-");

            if (!cachedCommand.Equals(command))
            {
                Debug.LogWarning($"Command name has been modified from {cachedCommand} to {command}");
            }
            
            return command;
        }
    }
}