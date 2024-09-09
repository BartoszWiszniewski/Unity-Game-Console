using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Console
{
    public static class GameConsoleInputProcessor
    {
        public static GameConsoleInputData ProcessInput(string input)
        {
            var data = new GameConsoleInputData();
            var parts = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Select(m => m.Value)
                .ToArray();

            data.Command = parts[0];
            data.Args = new string[parts.Length - 1];

            Array.Copy(parts, 1, data.Args, 0, parts.Length - 1);

            return data;
        }
        
        public static string CreateInputString(string command, string[] args)
        {
            return command + " " + string.Join(" ", args);
        }

        public static string ProcessInputArgument(string argument)
        {
            return argument.Trim().Replace("\"", string.Empty);
        }
    }

    public class GameConsoleInputData
    {
        public string Command;
        public string[] Args;
    }
}