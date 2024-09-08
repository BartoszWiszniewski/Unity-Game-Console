using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Console.Attributes;
using Console.Commands;
using Console.Converters;
using Console.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Console
{
    [ExecuteAlways]
    public class GameConsole : MonoBehaviour
    {
        private CommandCollection _commands;
        private ValueConverterCollection _converters;

        [SerializeField] 
        private Canvas consoleCanvas;
        
        [SerializeField]
        private GameConsoleStyle style;
        
        [SerializeField]
        private Image backgroundImage;
        
        [SerializeField]
        private TextMeshProUGUI textField;
        
        [SerializeField]
        private Image textBackgroundImage;
        
        [SerializeField]
        private TMP_InputField commandInputTextField;
        
        [SerializeField]
        private TextMeshProUGUI commandTextField;

        [SerializeField]
        private Image commandBackgroundImage;
        
        [SerializeField]
        private KeyCode openConsoleKey = KeyCode.BackQuote;
        
        [SerializeField]
        private Graphic[] foregroundControlsGraphics;
        
        [SerializeField]
        private Graphic[] backgroundControlsGraphics;
        
        [SerializeField]
        private RectTransform suggestionContainer;
        
        [SerializeField]
        private TextMeshProUGUI suggestionTextField;
        
        private List<ICommand> _currentSuggestions = new List<ICommand>();
        
        private void Awake()
        {
            textField.text = "";
            Cleanup();
        }

        private void Start()
        {
            _commands = new CommandCollection();
            _converters = new ValueConverterCollection();
            
            backgroundImage.color = style.backgroundColor;
            
            textField.color = style.textColor;
            suggestionTextField.color = style.textColor;
            textField.fontSize = style.textFontSize;
            
            commandTextField.color = style.inputTextColor;
            commandTextField.fontSize = style.inputTextFontSize;
            
            textBackgroundImage.color = style.textBackgroundColor;
            commandBackgroundImage.color = style.inputBackgroundColor;

            foreach (var backgroundControlsGraphic in backgroundControlsGraphics)
            {
                backgroundControlsGraphic.color = style.controlBackground;
            }
            
            foreach (var foregroundControlsGraphic in foregroundControlsGraphics)
            {
                foregroundControlsGraphic.color = style.controlColor;
            }
        }

        private void OnEnable()
        {
            commandInputTextField.onValueChanged.AddListener(OnCommandTextChanged);
            commandInputTextField.onSubmit.AddListener(ExecuteCommand);
            Cleanup();
        }

        private void OnDisable()
        {
            commandInputTextField.onValueChanged.RemoveListener(OnCommandTextChanged);
            commandInputTextField.onSubmit.RemoveListener(ExecuteCommand);
            Cleanup();
        }
        
        [Command("list-commands", "Get all commands", target: CommandTargetType.Single)]
        public void GetCommands()
        {
            var commands = _commands.GetGroupedCommands();
            var sb = new System.Text.StringBuilder();
            foreach (var commandGroup in commands)
            {
                sb.AppendLine("Group: " + commandGroup.Key);
                foreach (var command in commandGroup.Value)
                {
                    sb.AppendLine($"\t -{command.Command} {string.Join(", ", command.CommandArguments.Select(x => x.Name))}");   
                }
            }
            
            Print(sb.ToString());
        }

        public void Print(string text)
        {
            textField.text += text + "\n";
        }
        
        public void Print(string text, Color color)
        {
            Print($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>");
        }
        
        public void PrintError(string text)
        {
            Print($"> Error: <color=#{ColorUtility.ToHtmlStringRGB(style.errorColor)}>{text}</color>");
        }
        
        public void ExecuteCurrentCommand()
        {
            ExecuteCommand(commandInputTextField.text);
        }

        [Command("clear", "Clear console", group: "Console", target: CommandTargetType.Single)]
        public void Clear()
        {
            textField.text = "";
            Cleanup();
        }

        public void Close()
        {
            if(consoleCanvas == null) return;
            consoleCanvas.gameObject.SetActive(false);
            Cleanup();
        }

        public void Open()
        {
            if(consoleCanvas == null) return;
            consoleCanvas.gameObject.SetActive(true);
            Cleanup();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                AutocompleteCommand();
            }
            
            if(Input.GetKeyDown(openConsoleKey) && consoleCanvas != null)
            {
                if(consoleCanvas.gameObject.activeSelf)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }
        }

        private string GetCommandText(string command)
        {
            return Regex.Replace(command, @"^[\s\u200B]+|[\s\u200B]+$", string.Empty);
        }
        
        public void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            Cleanup();
            commandInputTextField.ActivateInputField(); // Keep focus on input field
            
            command = GetCommandText(command);
            Print($"> <color=#{ColorUtility.ToHtmlStringRGB(style.infoColor)}>{command}</color>");
            
            var commandParts = command.Split(' ');
            var commandName = commandParts[0];
            var commandArguments = commandParts.Skip(1).ToArray();
            
            var commands = _commands.GetCommands(commandName);
            if (commands.Count == 0)
            {
                PrintError("Command not found: " + commandName);
                return;
            }
            
            var commandsToExecute = commands
                .OrderBy(x => Math.Abs(x.CommandArguments.Count - commandArguments.Length)) // Order by the closest match in terms of parameter count
                .ThenBy(x => x.CommandArguments.Count) // If same difference, prefer commands with fewer total arguments
                .ToArray();
            
            foreach (var commandToExecute in commandsToExecute)
            {
                if (!_converters.TryConvertArguments(commandArguments, commandToExecute.ParameterTypes, out var convertedArguments))
                {
                    continue;
                }
                
                var result = commandToExecute.Execute(convertedArguments);
                if (result is not null)
                {
                    Print(result.ToString());
                }
                return;
            }
            
            PrintError($"Failed to execute command {commandName} with arguments {string.Join(", ", commandArguments)}");
        }
        
        private void Cleanup()
        {
            commandInputTextField.text = "";
            commandInputTextField.caretPosition = 0;
            suggestionTextField.text = "";
            suggestionContainer.gameObject.SetActive(false);
            _currentSuggestions.Clear();
        }
        
        private List<ICommand> GetCommands(string commandText)
        {
            var command = commandText.Split(' ').First();
            return _commands.Values.Where(x => x.Command.StartsWith(command, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Command)
                .ThenBy(x => x.CommandArguments.Count)
                .ToList();
        }

        private void OnCommandTextChanged(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                suggestionTextField.text = "";
                suggestionContainer.gameObject.SetActive(false);
                _currentSuggestions.Clear();
                return;
            }
            
            _currentSuggestions = GetCommands(value);
            if(!_currentSuggestions.Any())
            {
                suggestionTextField.text = "";
                suggestionContainer.gameObject.SetActive(false);
                return;
            }
            
            var sb = new StringBuilder();
            foreach (var command in _currentSuggestions)
            {
                sb.AppendLine($"{command.Command} {string.Join(", ", command.CommandArguments.Select(x => x.Name))} - {command.Description}");
            }
            
            suggestionContainer.gameObject.SetActive(true);
            suggestionTextField.text = sb.ToString();
        }
        
        private void AutocompleteCommand()
        {
            if (_currentSuggestions.Any() && !string.IsNullOrWhiteSpace(commandInputTextField.text) && !commandInputTextField.text.Contains(" "))
            {
                var firstSuggestion = _currentSuggestions.First();
                commandInputTextField.text = firstSuggestion.Command;
                commandInputTextField.caretPosition = firstSuggestion.Command.Length;
                OnCommandTextChanged(firstSuggestion.Command);
            }
        }
    }
}
