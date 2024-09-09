using System;
using System.Collections.Generic;

namespace Console.Suggestions
{
    public interface ISuggestion
    {
        public Type Type { get; }
        public List<string> GetSuggestions(Type type, string input);
    }
}