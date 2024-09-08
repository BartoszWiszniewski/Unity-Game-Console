using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Console.Attributes;
using Console.Types;
using UnityEngine;

namespace Console.Commands
{
    public class MethodCommand : BaseCommand
    {
        public override bool IsStatic { get; }
        
        private readonly MethodInfo _methodInfo;
        
        public override IReadOnlyList<CommandArgument> CommandArguments { get; }

        private MethodCommand(Type targetType, MethodInfo methodInfo, string command, string description, string group, CommandTargetType target) : base(targetType, command, description, group, target)
        {
            _methodInfo = methodInfo;
            IsStatic = methodInfo.IsStatic;
            CommandArguments = methodInfo.GetParameters().Select(p => new CommandArgument(p.Name, p.ParameterType, p.HasDefaultValue, p.DefaultValue)).ToList();
        }
        
        public static MethodCommand Create(Type targetType, MethodInfo methodInfo, CommandAttribute attribute)
        {
            return new MethodCommand(targetType, methodInfo, attribute.Command, attribute.Description, attribute.Group, attribute.Target);
        }
        
        public override object Execute(params object[] parameters)
        {
            try
            {
                var finalParameters = new object[CommandArguments.Count];

                for (var i = 0; i < CommandArguments.Count; i++)
                {
                    if (i < parameters.Length && parameters[i] != null)
                    {
                        finalParameters[i] = parameters[i];
                    }
                    else if (CommandArguments[i].HasDefaultValue)
                    {
                        finalParameters[i] = CommandArguments[i].DefaultValue;
                    }
                    else
                    {
                        throw new ArgumentException($"Parameter '{CommandArguments[i].Name}' is required but not provided.");
                    }
                }

                if (IsStatic)
                {
                    return _methodInfo.Invoke(null, finalParameters);
                }

                object result = null;
                foreach (var target in GetTargets())
                {
                    result = _methodInfo.Invoke(target, finalParameters);
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }
}