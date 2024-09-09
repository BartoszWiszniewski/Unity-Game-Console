using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Console.Suggestions
{
    public class EnumSuggestion : ISuggestion
    {
        public Type Type => typeof(Enum);

        public List<string> GetSuggestions(Type type, string input)
        {
            if (!type.IsEnum)
            {
                Debug.LogError($"{type.Name} is not an enum type.");
                return new List<string>();
            }

            return Enum.GetNames(type)
                .Where(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x)
                .ToList();
        }
    }
}