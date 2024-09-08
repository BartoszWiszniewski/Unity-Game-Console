# Unity Game Console

The `GameConsole` class provides an in-game console for executing commands, displaying output, and managing console UI elements.


![Example Image](Assets/Textures/Example.png)

## Properties

- **consoleCanvas**: The `Canvas` component that contains the console UI.
- **style**: The `GameConsoleStyle` object that defines the console's visual style.
- **backgroundImage**: The `Image` component for the console's background.
- **textField**: The `TextMeshProUGUI` component for displaying console output.
- **textBackgroundImage**: The `Image` component for the background of the text field.
- **commandInputTextField**: The `TMP_InputField` component for entering commands.
- **commandTextField**: The `TextMeshProUGUI` component for displaying the current command.
- **commandBackgroundImage**: The `Image` component for the background of the command input field.
- **openConsoleKey**: The `KeyCode` used to toggle the console.
- **foregroundControlsGraphics**: An array of `Graphic` components for the foreground controls.
- **backgroundControlsGraphics**: An array of `Graphic` components for the background controls.
- **suggestionContainer**: The `RectTransform` for the suggestion container.
- **suggestionTextField**: The `TextMeshProUGUI` component for displaying command suggestions.

## Methods

- **Awake**: Initializes the console text field and performs cleanup.
- **Start**: Initializes command and converter collections and applies the style settings.
- **OnEnable**: Adds listeners for command input changes and submission.
- **OnDisable**: Removes listeners for command input changes and submission.
- **GetCommands**: Lists all available commands.
- **Print**: Prints text to the console.
- **PrintError**: Prints an error message to the console.
- **ExecuteCurrentCommand**: Executes the current command in the input field.
- **Clear**: Clears the console output.
- **Close**: Closes the console UI.
- **Open**: Opens the console UI.
- **Update**: Handles input for toggling the console and autocompleting commands.
- **ExecuteCommand**: Executes a specified command.
- **Cleanup**: Resets the input field and suggestion container.
- **GetCommands**: Retrieves commands matching the input text.
- **OnCommandTextChanged**: Updates command suggestions based on input text.
- **AutocompleteCommand**: Autocompletes the current command based on suggestions.


## Usage

1. **Add `GameConsole` to the Scene**:
   - Create an empty GameObject and attach the `GameConsole` component or use included prefab.
   - Configure the `consoleCanvas`, `style`, and other properties as needed.

2. **Open and Close the Console**:
   - Press the key specified in `openConsoleKey` (default is BackQuote) to toggle the console.

3. **Execute Commands**:
   - Type a command in the input field and press Enter to execute it.
   - Use the `list-commands` command to see all available commands.
   - Use the `clear` command to clear the console output.

4. **Autocomplete Commands**:
   - Press `Tab` to autocomplete the current command.

5. **Commands hisotry**:
   - Use the `Up` and `Down` arrow keys to navigate through the command history.

6. **Customize Console Style**:
   - Modify the `GameConsoleStyle` object to change the appearance of the console.

## Creating Commands using CommandAttribute
The `CommandAttribute` is used to define commands that can be executed in the `GameConsole`.
### Example
```
public class GameCommands : MonoBehaviour
{
    [Command("greet", "Prints a greeting message")]
    public void Greet()
    {
        Debug.Log("Hello, Player!");
    }

    [Command("add", "Adds two numbers")]
    public int Add(int a, int b)
    {
        return a + b;
    }
}
```
### Usage
1. **Define Commands**:  
    - Use the CommandAttribute to mark methods, properties, or fields as console commands.
2. **Execute Commands**:
    - Type the command name in the console input field and press Enter to execute it.

## Creating Value Converters
The `IValueConverter` interface is used to create custom value converters for converting between strings and specific types. Converters are automatically loaded and registered by the `ValueConverterCollection`

### Example
```
public class CustomValueConverter : IValueConverter
{
    public Type Type => typeof(CustomType);

    public object Convert(string value)
    {
        // Implement conversion from string to CustomType
    }

    public string Convert(object value)
    {
        // Implement conversion from CustomType to string
    }
}
```

### Built-in Converters
The following built-in converters are available:  
- ByteValueConverter
- ShortValueConverter
- IntValueConverter
- LongValueConverter
- FloatValueConverter
- DoubleValueConverter
- StringValueConverter
- BoolValueConverter
- DateTimeValueConverter
- Vector2ValueConverter
- Vector3ValueConverter
- Vector4ValueConverter
- ColorValueConverter
- Vector2IntValueConverter
- Vector3IntValueConverter
- QuaternionValueConverter
- RectValueConverter
- BoundsValueConverter
- EnumValueConverter
- GameObjectValueConverter