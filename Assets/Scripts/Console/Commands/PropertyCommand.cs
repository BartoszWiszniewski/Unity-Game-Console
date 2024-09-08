using System;
using System.Collections.Generic;
using System.Reflection;
using Console.Attributes;
using Console.Types;
using UnityEngine;

namespace Console.Commands
{
    public class PropertyCommand : BaseCommand
    {
        public override bool IsStatic { get; }
        public override IReadOnlyList<CommandArgument> CommandArguments { get; }
        
        private readonly PropertyInfo _propertyInfo;
        private readonly bool _isGetter;
        
        private PropertyCommand(Type targetType, PropertyInfo propertyInfo, string command, string description, string group, CommandTargetType target, bool isGetter) 
            : base(targetType, command, description, group, target)
        {
            _propertyInfo = propertyInfo;
            _isGetter = isGetter;
            IsStatic = propertyInfo.GetMethod?.IsStatic ?? propertyInfo.SetMethod?.IsStatic ?? false;
            CommandArguments = isGetter 
                ? new List<CommandArgument>() 
                : new List<CommandArgument> { new("value", propertyInfo.PropertyType, false, null) };
        }
        
        public static IEnumerable<PropertyCommand> Create(Type targetType, PropertyInfo propertyInfo, CommandAttribute attribute)
        {
            if (propertyInfo.CanRead)
            {
                yield return new PropertyCommand(targetType, propertyInfo, attribute.Command, "Get " + attribute.Description, attribute.Group, attribute.Target, true);
            }
            if (propertyInfo.CanWrite)
            {
                yield return new PropertyCommand(targetType, propertyInfo, attribute.Command, "Set " 
                    + attribute.Description, attribute.Group, attribute.Target, false);
            }
        }
        
        public override object Execute(params object[] parameters)
        {
            try
            {
                if (_isGetter)
                {
                    if (IsStatic)
                    {
                        return _propertyInfo.GetValue(null);
                    }

                    object result = null;
                    foreach (var target in GetTargets())
                    {
                        result = _propertyInfo.GetValue(target);
                    }

                    return result;
                }

                if (parameters.Length != 1 || parameters[0].GetType() != _propertyInfo.PropertyType)
                {
                    throw new ArgumentException($"Invalid parameter for setting property {TargetType.Name}.{_propertyInfo.Name}.");
                }

                if (IsStatic)
                {
                    _propertyInfo.SetValue(null, parameters[0]);
                }
                else
                {
                    foreach (var target in GetTargets())
                    {
                        _propertyInfo.SetValue(target, parameters[0]);
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }
}