using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Console
{
    [CreateAssetMenu(fileName = "GameConsoleStyle", menuName = "GameConsole/GameConsoleStyle")]
    [Serializable]
    public class GameConsoleStyle : ScriptableObject
    {
        public Color backgroundColor = new Color(0, 0, 0, 0.5f);
        
        public Color textColor = Color.white;
        public Color textBackgroundColor = new Color(0, 0, 0, 0.5f);
        public float textFontSize = 16;
        
        public Color inputTextColor = Color.white;
        public Color inputBackgroundColor = new Color(0, 0, 0, 0.5f);
        public float inputTextFontSize = 16;
        
        public Color errorColor = new Color(1.0f, 0.3f, 0.3f);
        public Color warningColor = Color.yellow;
        public Color infoColor = Color.cyan;
        public Color assertColor = new Color(1.0f, 0.3f, 0.3f);
        public Color logColor = new Color(0.0f, 0.5f, 1.0f, 1.0f);
        
        public Color controlColor = new Color(1.0f, 0.5686275f, 0.0f);
        public Color controlBackground = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    }
}