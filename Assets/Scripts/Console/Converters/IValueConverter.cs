using System;

namespace Console.Converters
{
    public interface IValueConverter
    {
        public Type Type { get; }
        public object Convert(string value);
        public string Convert(object value);
    }
}