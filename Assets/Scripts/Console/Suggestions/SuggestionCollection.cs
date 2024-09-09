using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Console.Commands;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Console.Suggestions
{
    public class SuggestionCollection
    {
        private readonly Dictionary<Type, ISuggestion> _suggestions = new Dictionary<Type, ISuggestion>();

        public SuggestionCollection(Dictionary<Type, ISuggestion> customSuggestions)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            
            _suggestions.Clear();
            _suggestions.AddRange(customSuggestions);
            
            LoadAllSuggestions();
        }
        
        private void LoadAllSuggestions()
        {
            var suggestionTypes = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var suggestionType in suggestionTypes)
            {
                var types = suggestionType.GetTypes()
                    .Where(type => IsValidType(type) && !_suggestions.ContainsKey(type));
                
                foreach (var type in types)
                {
                    var instance = (ISuggestion) Activator.CreateInstance(type);
                    if (!_suggestions.TryAdd(instance.Type, instance))
                    {
                        Debug.LogError($"Failed to add suggestion for type {instance.GetType()} key already exists");
                    }
                }
            }
        }
        
        private static bool IsValidType(Type type)
        {
            return typeof(ISuggestion).IsAssignableFrom(type) &&
                   !type.IsInterface &&
                   !type.IsAbstract &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }
        
        public bool TryGetSuggestion(Type type, string input, out List<string> suggestions)
        {
            var targetType = type;
            if (targetType.IsEnum)
            {
                targetType = typeof(Enum);
            }
            
            if (_suggestions.TryGetValue(targetType, out var suggestion))
            {
                suggestions = suggestion.GetSuggestions(type, input);
                return true;
            }

            suggestions = new List<string>();
            return false;
        }
    }
    
    public class ByteSuggestion : ISuggestion
    {
        public Type Type => typeof(byte);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0"
            };
        }
    }
    
    public class ShortSuggestion : ISuggestion
    {
        public Type Type => typeof(short);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0"
            };
        }
    }
    
    public class IntSuggestion : ISuggestion
    {
        public Type Type => typeof(int);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0"
            };
        }
    }
    
    public class LongSuggestion : ISuggestion
    {
        public Type Type => typeof(long);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0"
            };
        }
    }
    
    public class FloatSuggestion : ISuggestion
    {
        public Type Type => typeof(float);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0.0"
            };
        }
    }
    
    public class DoubleSuggestion : ISuggestion
    {
        public Type Type => typeof(double);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0.0"
            };
        }
    }
    
    public class StringSuggestion : ISuggestion
    {
        public Type Type => typeof(string);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "text"
            };
        }
    }
    
    public class BoolSuggestion : ISuggestion
    {
        private readonly List<string> _values = new List<string>
        {
            "true",
            "false"
        };

        public Type Type => typeof(bool);

        public List<string> GetSuggestions(Type type, string input)
        {
            return _values
                .Where(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x)
                .ToList();
        }
    }
    
    public class DateTimeSuggestion : ISuggestion
    {
        public Type Type => typeof(DateTime);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                DateTime.Now.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
    
    public class Vector2Suggestion : ISuggestion
    {
        public Type Type => typeof(Vector2);

        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0.0,0.0"
            };
        }
    }
    
    public class Vector3Suggestion : ISuggestion
    {
        public Type Type => typeof(Vector3);
        
        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0.0,0.0,0.0"
            };
        }
    }
    
    public class Vector4Suggestion : ISuggestion
    {
        public Type Type => typeof(Vector4);
        
        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0.0,0.0,0.0,0.0"
            };
        }
    }
    
    public class ColorSuggestion : ISuggestion
    {
        public Type Type => typeof(Color);
        
        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0.0,0.0,0.0,1.0"
            };
        }
    }
    
    public class Vector2IntSuggestion : ISuggestion
    {
        public Type Type => typeof(Vector2Int);
        
        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0,0"
            };
        }
    }
    
    public class Vector3IntSuggestion : ISuggestion
    {
        public Type Type => typeof(Vector3Int);
        
        public List<string> GetSuggestions(Type type, string input)
        {
            return new List<string>
            {
                "0,0,0"
            };
        }
    }
    
    public class GameObjectSuggestion : ISuggestion
    {
        public Type Type => typeof(GameObject);
        
        private string _cachedInput;
        private List<string> _cachedGameObjects = new List<string>();
        
        public List<string> GetSuggestions(Type type, string input)
        {
            input = input.Trim('"');
            if(string.IsNullOrWhiteSpace(input))
            {
                _cachedInput = string.Empty;
                _cachedGameObjects.Clear();
                return new List<string>();
            }
            _cachedInput = input;
            if (!_cachedGameObjects.Any())
            {
                _cachedGameObjects = Object.FindObjectsOfType<GameObject>().Select(x => x.name).ToList();
            }
            
            return _cachedGameObjects
                .Where(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x)
                .Select(GetGameObjectName)
                .ToList();
        }
        
        private string GetGameObjectName(string name)
        {
            return name.Contains(" ") ? $"\"{name}\"" : name;
        }
    }
}