using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Console.Converters
{
    public class ValueConverterCollection
    {
        private readonly Dictionary<Type, IValueConverter> _converters = new();

        public ValueConverterCollection()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            LoadAllValueConverters();
        }

        private void LoadAllValueConverters()
        {
            _converters.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(IsValidType);

                foreach (var type in types)
                {
                    var instance = (IValueConverter)Activator.CreateInstance(type);
                    if (!_converters.TryAdd(instance.Type, instance))
                    {
                        Debug.LogError($"Failed to add value converter for type {instance.Type} key already exists");
                    }
                }
            }
        }

        private static bool IsValidType(Type type)
        {
            return typeof(IValueConverter).IsAssignableFrom(type) &&
                   !type.IsInterface &&
                   !type.IsAbstract;
        }
        
        public IValueConverter GetConverter(Type type)
        {
            var targetType = type;
            if (targetType.IsEnum)
            {
                targetType = typeof(Enum);
            }
            
            _converters.TryGetValue(targetType, out var converter);
            if (converter == null)
            {
                Debug.LogError($"Failed to find value converter for type {type}");
            }

            return converter;
        }

        public string ConvertToString(object value)
        {
            var type = value.GetType();
            var converter = GetConverter(type);
            try
            {
                return converter?.Convert(value);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert value {value} to string: {e.Message}");
                return null;
            }
        }

        public string ConvertToString<T>(T value)
        {
            return ConvertToString((object)value);
        }

        public object ConvertFromString(string value, Type type)
        {
            var converter = GetConverter(type);
            try
            {
                return converter?.Convert(value);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert value {value} to type {type}: {e.Message}");
                return null;
            }
        }

        public bool TryConvert(string value, Type type, out object result)
        {
            var converter = GetConverter(type);
            if (converter == null)
            {
                Debug.LogError($"Failed to find value converter for type {type}");
                result = null;
                return false;
            }

            try
            {
                if(type.IsEnum)
                {
                    value = $"{type.FullName}:{value}";
                    //result = Enum.Parse(type, value);
                    //return true;
                }
                
                result = converter.Convert(value);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert value {value} to type {type}: {e.Message}");
                result = null;
                return false;
            }
        }

        public bool TryConvertArguments(string[] stringArguments, Type[] targetTypes, out object[] arguments)
        {
            if (stringArguments.Length != targetTypes.Length)
            {
                arguments = Array.Empty<object>();
                return false;
            }

            arguments = new object[stringArguments.Length];
            for (var i = 0; i < stringArguments.Length; i++)
            {
                if (!TryConvert(stringArguments[i], targetTypes[i], out var result))
                {
                    arguments = Array.Empty<object>();
                    return false;
                }

                arguments[i] = result;
            }

            return true;
        }
    }

    public class ByteValueConverter : IValueConverter
    {
        public Type Type => typeof(byte);

        public object Convert(string value)
        {
            return byte.Parse(value);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class ShortValueConverter : IValueConverter
    {
        public Type Type => typeof(short);

        public object Convert(string value)
        {
            return short.Parse(value);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class IntValueConverter : IValueConverter
    {
        public Type Type => typeof(int);

        public object Convert(string value)
        {
            return int.Parse(value);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class LongValueConverter : IValueConverter
    {
        public Type Type => typeof(long);

        public object Convert(string value)
        {
            return long.Parse(value);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class FloatValueConverter : IValueConverter
    {
        public Type Type => typeof(float);

        public object Convert(string value)
        {
            return float.Parse(value);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class DoubleValueConverter : IValueConverter
    {
        public Type Type => typeof(double);

        public object Convert(string value)
        {
            return double.Parse(value);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class StringValueConverter : IValueConverter
    {
        public Type Type => typeof(string);

        public object Convert(string value)
        {
            value = value.Trim('"');
            return value;
        }

        public string Convert(object value)
        {
            var stringValue = (string)value;
            return stringValue.Contains(" ") ? $"\"{stringValue}\"" : stringValue;
        }
    }

    public class BoolValueConverter : IValueConverter
    {
        public Type Type => typeof(bool);

        public object Convert(string value)
        {
            return bool.Parse(value);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class DateTimeValueConverter : IValueConverter
    {
        public Type Type => typeof(DateTime);

        public object Convert(string value)
        {
            return DateTime.Parse(value);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class Vector2ValueConverter : IValueConverter
    {
        public Type Type => typeof(Vector2);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new Vector2(float.Parse(values[0].Trim()), float.Parse(values[1].Trim()));
        }

        public string Convert(object value)
        {
            var vector = (Vector2)value;
            return $"{vector.x},{vector.y}";
        }
    }

    public class Vector3ValueConverter : IValueConverter
    {
        public Type Type => typeof(Vector3);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new Vector3(float.Parse(values[0].Trim()), float.Parse(values[1].Trim()), float.Parse(values[2].Trim()));
        }

        public string Convert(object value)
        {
            var vector = (Vector3)value;
            return $"{vector.x},{vector.y},{vector.z}";
        }
    }

    public class Vector4ValueConverter : IValueConverter
    {
        public Type Type => typeof(Vector4);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new Vector4(float.Parse(values[0].Trim()),
                float.Parse(values[1].Trim()),
                float.Parse(values[2].Trim()),
                float.Parse(values[3].Trim()));
        }

        public string Convert(object value)
        {
            var vector = (Vector4)value;
            return $"{vector.x},{vector.y},{vector.z},{vector.w}";
        }
    }

    public class ColorValueConverter : IValueConverter
    {
        public Type Type => typeof(Color);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new Color(float.Parse(values[0].Trim()),
                float.Parse(values[1].Trim()),
                float.Parse(values[2].Trim()),
                float.Parse(values[3].Trim()));
        }

        public string Convert(object value)
        {
            var color = (Color)value;
            return $"{color.r},{color.g},{color.b},{color.a}";
        }
    }

    public class Vector2Int : IValueConverter
    {
        public Type Type => typeof(UnityEngine.Vector2Int);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new UnityEngine.Vector2Int(int.Parse(values[0].Trim()), int.Parse(values[1].Trim()));
        }

        public string Convert(object value)
        {
            var vector = (UnityEngine.Vector2Int)value;
            return $"{vector.x},{vector.y}";
        }
    }

    public class Vector3Int : IValueConverter
    {
        public Type Type => typeof(UnityEngine.Vector3Int);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new UnityEngine.Vector3Int(int.Parse(values[0].Trim()), int.Parse(values[1].Trim()),
                int.Parse(values[2].Trim()));
        }

        public string Convert(object value)
        {
            var vector = (UnityEngine.Vector3Int)value;
            return $"{vector.x},{vector.y},{vector.z}";
        }
    }

    public class QuaternionValueConverter : IValueConverter
    {
        public Type Type => typeof(Quaternion);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new Quaternion(float.Parse(values[0].Trim()), float.Parse(values[1].Trim()),
                float.Parse(values[2].Trim()), float.Parse(values[3].Trim()));
        }

        public string Convert(object value)
        {
            var quaternion = (Quaternion)value;
            return $"{quaternion.x},{quaternion.y},{quaternion.z},{quaternion.w}";
        }
    }

    public class RectValueConverter : IValueConverter
    {
        public Type Type => typeof(Rect);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new Rect(float.Parse(values[0].Trim()), float.Parse(values[1].Trim()),
                float.Parse(values[2].Trim()), float.Parse(values[3].Trim()));
        }

        public string Convert(object value)
        {
            var rect = (Rect)value;
            return $"{rect.x},{rect.y},{rect.width},{rect.height}";
        }
    }

    public class BoundsValueConverter : IValueConverter
    {
        public Type Type => typeof(UnityEngine.Bounds);

        public object Convert(string value)
        {
            var values = value.Split(',');
            return new Bounds(
                new Vector3(float.Parse(values[0].Trim()), float.Parse(values[1].Trim()), float.Parse(values[2].Trim())),
                new Vector3(float.Parse(values[3].Trim()), float.Parse(values[4].Trim()), float.Parse(values[5].Trim())));
        }

        public string Convert(object value)
        {
            var bounds = (Bounds)value;
            return
                $"{bounds.center.x},{bounds.center.y},{bounds.center.z},{bounds.size.x},{bounds.size.y},{bounds.size.z}";
        }
    }

    public class EnumValueConverter : IValueConverter
    {
        public Type Type => typeof(Enum);

        public object Convert(string value)
        {
            var parts = value.Split(':');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Input value must be in the format 'Type:EnumValue'");
            }

            var typeName = parts[0].Trim();
            var enumValue = parts[1].Trim();

            var type = Type.GetType(typeName);
            if (type == null || !type.IsEnum)
            {
                throw new ArgumentException($"Type '{typeName}' is not a valid enum type");
            }

            return Enum.Parse(type, enumValue);
        }

        public string Convert(object value)
        {
            return value.ToString();
        }
    }

    public class GameObjectValueConverter : IValueConverter
    {
        public Type Type => typeof(GameObject);

        public object Convert(string value)
        {
            value = value.Trim('"');
            return GameObject.Find(value);
        }

        public string Convert(object value)
        {
            var name = ((GameObject)value).name;
            
            return name.Contains(" ") ? $"\"{name}\"" : name;
        }
    }
}