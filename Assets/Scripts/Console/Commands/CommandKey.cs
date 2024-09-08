using System;
using System.Collections.Generic;

namespace Console.Commands
{
    public readonly struct CommandKey : IEquatable<CommandKey>
    {
        public readonly string Command;
        public readonly int ArgumentCount;

        public CommandKey(string command, int argumentCount)
        {
            Command = command;
            ArgumentCount = argumentCount;
        }

        public bool Equals(CommandKey other)
        {
            return Command == other.Command && ArgumentCount == other.ArgumentCount;
        }

        public override bool Equals(object obj)
        {
            return obj is CommandKey other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Command, ArgumentCount);
        }
        
        public static bool operator ==(CommandKey left, CommandKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CommandKey left, CommandKey right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{Command} {ArgumentCount}";
        }
    }
}