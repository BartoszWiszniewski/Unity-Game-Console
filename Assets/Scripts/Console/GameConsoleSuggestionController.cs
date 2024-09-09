using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Console.Commands;
using Console.Suggestions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Console
{
    public class GameConsoleSuggestionController : MonoBehaviour
    {
        [SerializeField]
        private GameConsole gameConsole;
        
        [SerializeField]
        private RectTransform suggestionContainer;
        
        [SerializeField]
        private TextMeshProUGUI suggestionTextField;

        private SuggestionCollection _suggestionCollection;

        private Coroutine _waitForCommandsCoroutine;
        
        private void Awake()
        {
            if(gameConsole == null)
            {
                gameConsole = GetComponent<GameConsole>();
            }
        }

        private void Start()
        {
            suggestionTextField.color = gameConsole.Style.textColor;
        }

        private void OnEnable()
        {
            _waitForCommandsCoroutine = StartCoroutine(WaitForCommands());
            CleanSuggestions();
        }
        
        private void OnDisable()
        {
            StopCoroutine(_waitForCommandsCoroutine);
            gameConsole.CommandInputTextField.onValueChanged.RemoveListener(OnInputValueChanged);
            gameConsole.CommandInputTextField.onSubmit.RemoveListener(OnInputSubmit);
            CleanSuggestions();
        }

        private void Update()
        {
            if(!gameConsole.IsVisible || !gameConsole.CommandInputTextField.isFocused) return;
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                AutocompleteCommand();
            }
        }
        
        private IEnumerator WaitForCommands()
        {
            yield return new WaitUntil(() => gameConsole.Commands != null);
            
            _suggestionCollection = new SuggestionCollection(new Dictionary<Type, ISuggestion>
            {
                {typeof(ICommand), new CommandSuggestion(gameConsole.Commands)}
            });
            
            gameConsole.CommandInputTextField.onValueChanged.AddListener(OnInputValueChanged);
            gameConsole.CommandInputTextField.onSubmit.AddListener(OnInputSubmit);
        }
        
        public void CleanSuggestions()
        {
            suggestionTextField.text = "";
            suggestionContainer.gameObject.SetActive(false);
        }
        
        private void OnInputSubmit(string input)
        {
            CleanSuggestions();
        }

        private void OnInputValueChanged(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                CleanSuggestions();
                return;
            }
            var data = GameConsoleInputProcessor.ProcessInput(input);
            var (argumentTextIndex, argumentIndex) = GetCursorItemIndex(input, gameConsole.CommandInputTextField.caretPosition);
            
            if (argumentIndex == 0)
            {
                var command = data.Command;
                if(_suggestionCollection.TryGetSuggestion(typeof(ICommand), command, out var suggestions))
                {
                    suggestionTextField.text = string.Join("\n", suggestions);
                    suggestionContainer.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(suggestionContainer);
                }
                else
                {
                    CleanSuggestions();
                }
            }
            else
            {
                var bestCommand = BestFitCommand(data.Command, argumentIndex > data.Args.Length ? argumentIndex : data.Args.Length);
                if (bestCommand == null || bestCommand.CommandArguments.Count < argumentIndex)
                {
                    CleanSuggestions();
                    return;
                }
                
                argumentIndex -= 1;
                var argumentType = bestCommand.CommandArguments[argumentIndex].Type;
                
                var argument = argumentIndex >= data.Args.Length ? string.Empty : data.Args[argumentIndex];
                
                if(_suggestionCollection.TryGetSuggestion(argumentType, argument, out var suggestions))
                {
                    suggestionTextField.text = string.Join("\n", suggestions);
                    suggestionContainer.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(suggestionContainer);
                }
                else
                {
                    CleanSuggestions();
                }
            }
        }
        
        private (int argumentTextIndex, int argumentIndex) GetCursorItemIndex(string input, int cursorIndex)
        {
            var argumentTextIndex = cursorIndex < input.Length ? cursorIndex-1 : input.Length - 1;
            if(argumentTextIndex < 0)
            {
                return (0, 0);
            }
            
            while (argumentTextIndex > 0 && input[argumentTextIndex] != ' ')
            {
                argumentTextIndex--;
            }

            var argumentIndex = 0;
            for (var i = 0; i <= argumentTextIndex; i++)
            {
                if (input[i] == ' ')
                {
                    argumentIndex++;
                }
            }
            
            return (argumentTextIndex, argumentIndex);
        }

        private IEnumerable<ICommand> GetCommands(string command, int argumentsCount = 0)
        {
            return gameConsole.Commands.GetCommands(command)
                .Where(x => x.CommandArguments.Count >= argumentsCount)
                .OrderBy(x => Math.Abs(x.CommandArguments.Count - argumentsCount)) // Order by the closest match in terms of parameter count
                .ThenBy(x => x.CommandArguments.Count); // If same difference, prefer commands with fewer total arguments
        }
        
        private ICommand BestFitCommand(string command, int argumentsCount)
        {
            return GetCommands(command, argumentsCount).FirstOrDefault();
        }
        
        private ICommand FindCommand(string command, int argumentsCount)
        {
            return gameConsole.Commands.Values.Where(x => x.Command.StartsWith(command, StringComparison.OrdinalIgnoreCase))
                .Where(x => x.CommandArguments.Count >= argumentsCount)
                .OrderBy(x => Math.Abs(x.CommandArguments.Count - argumentsCount)) // Order by the closest match in terms of parameter count
                .ThenBy(x => x.CommandArguments.Count) // If same difference, prefer commands with fewer total arguments
                .FirstOrDefault();
        }
        
        private void AutocompleteCommand()
        {
            var input = gameConsole.CommandInputTextField.text;
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }
            
            var data = GameConsoleInputProcessor.ProcessInput(input);
            var (argumentTextIndex, argumentIndex) = GetCursorItemIndex(input, gameConsole.CommandInputTextField.caretPosition);
            
            if (argumentIndex == 0)
            {
                var bestCommand = FindCommand(data.Command, data.Args.Length);
                if(bestCommand == null)
                {
                    return;
                }
                
                gameConsole.CommandInputTextField.text = GameConsoleInputProcessor.CreateInputString(bestCommand.Command, data.Args);
                // gameConsole.CommandInputTextField.text = bestCommand.Command;
                gameConsole.CommandInputTextField.caretPosition = bestCommand.Command.Length;
                CleanSuggestions();
            }
            else
            {
                var bestCommand = BestFitCommand(data.Command, argumentIndex > data.Args.Length ? argumentIndex : data.Args.Length);
                if (bestCommand == null || bestCommand.CommandArguments.Count < argumentIndex)
                {
                    return;
                }
                
                argumentIndex -= 1;
                var argumentType = bestCommand.CommandArguments[argumentIndex].Type;
                
                var argument = argumentIndex >= data.Args.Length ? string.Empty : data.Args[argumentIndex];
                if(_suggestionCollection.TryGetSuggestion(argumentType, argument, out var suggestions))
                {
                    if(data.Args.Length <= argumentIndex)
                    {
                        Array.Resize(ref data.Args, argumentIndex + 1);
                    }
                    
                    var suggestion = suggestions.FirstOrDefault();
                    if(suggestion == null)
                    {
                        return;
                    }
                    
                    data.Args[argumentIndex] = suggestion;
                    gameConsole.CommandInputTextField.text = GameConsoleInputProcessor.CreateInputString(data.Command, data.Args);
                    gameConsole.CommandInputTextField.caretPosition = argumentTextIndex + suggestion.Length + 1;
                    CleanSuggestions();
                }
            }
            
        }
    }
}