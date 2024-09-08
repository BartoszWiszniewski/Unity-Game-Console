using System;
using Console.Types;

namespace Console.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string Command { get; }
        public string Description { get; }
        public string Group { get; set; }
        public CommandTargetType Target { get; }

        public CommandAttribute(string command, string description = null, string group = null, CommandTargetType target = CommandTargetType.All)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                group = "Global";
            }
            
            Command = command;
            Description = description;
            Group = group;
            Target = target;
        }
    }
}