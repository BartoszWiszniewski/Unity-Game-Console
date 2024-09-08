using System;

namespace Console.Commands
{
    public readonly struct CommandArgument
    {
        public readonly string Name;
        public readonly Type Type;
        public readonly bool HasDefaultValue;
        public readonly object DefaultValue;
        
        public CommandArgument(string name, Type type,  bool hasDefaultValue, object defaultValue = null)
        {
            Name = name;
            Type = type;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
        }
    }
}