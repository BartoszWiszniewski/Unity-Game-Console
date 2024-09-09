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
    [RequireComponent(typeof(GameConsoleSuggestionController))]
    public class GameConsole : MonoBehaviour
    {
        private CommandCollection _commands;
        public CommandCollection Commands => _commands;
        
        private ValueConverterCollection _converters;
        public ValueConverterCollection Converters => _converters;

        [SerializeField] 
        private Canvas consoleCanvas;
        public bool IsVisible => consoleCanvas.gameObject.activeSelf;
        
        [SerializeField]
        private GameConsoleStyle style;
        public GameConsoleStyle Style => style;
        
        [SerializeField]
        private Image backgroundImage;
        
        [SerializeField]
        private TextMeshProUGUI textField;
        
        [SerializeField]
        private Image textBackgroundImage;
        
        [SerializeField]
        private TMP_InputField commandInputTextField;
        public TMP_InputField CommandInputTextField => commandInputTextField;

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
        
        private List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;

        private GameConsoleSuggestionController _gameConsoleSuggestionController;
        
        private void Awake()
        {
            textField.text = "";
            Cleanup();

            if (_gameConsoleSuggestionController == null)
            {
                _gameConsoleSuggestionController = GetComponent<GameConsoleSuggestionController>();
            }
            
            _commands = new CommandCollection();
            _converters = new ValueConverterCollection();
        }

        private void Start()
        {
            backgroundImage.color = style.backgroundColor;
            
            textField.color = style.textColor;
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
            commandInputTextField.onSubmit.AddListener(ExecuteCommand);
            _commandHistory.Clear();
            Cleanup();
        }

        private void OnDisable()
        {
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
            if (consoleCanvas.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    ShowPreviousCommand();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    ShowNextCommand();
                }
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

        private void ShowPreviousCommand()
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                commandInputTextField.text = _commandHistory[_historyIndex];
                commandInputTextField.caretPosition = commandInputTextField.text.Length;
            }
        }

        private void ShowNextCommand()
        {
            if (_historyIndex < _commandHistory.Count - 1)
            {
                _historyIndex++;
                commandInputTextField.text = _commandHistory[_historyIndex];
                commandInputTextField.caretPosition = commandInputTextField.text.Length;
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

                SaveCommand(command);
                
                return;
            }
            
            PrintError($"Failed to execute command {commandName} with arguments {string.Join(", ", commandArguments)}");
        }
        
        private void SaveCommand(string command)
        {
            _commandHistory.RemoveAll(c => c == command);
            _commandHistory.Add(command);
            _historyIndex = _commandHistory.Count;
        }
        
        private void Cleanup()
        {
            commandInputTextField.text = "";
            commandInputTextField.caretPosition = 0;
        }

        [Command("TestEnum", "Test enum", group: "Test", target: CommandTargetType.Single)]
        public TestEnum TestEnum { get; set; }
    }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}
