using System;
using System.Collections.Generic;
using System.Linq;
using Console.Types;
using UnityEngine;

namespace Console.Commands
{
    public interface ICommand
    {
        public CommandKey CommandKey { get; }
        public string Command { get; }
        public string Description { get; }
        public string Group { get; }
        public CommandTargetType Target { get; }
        public bool IsStatic { get; }
        public Type TargetType { get; }
        public IReadOnlyList<CommandArgument> CommandArguments { get; }
        public Type[] ParameterTypes => CommandArguments.Select(x => x.Type).ToArray();
        public object Execute(params object[] parameters);
        public bool CanExecute(params object[] parameters);
    }
}