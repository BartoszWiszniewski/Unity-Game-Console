using Console.Attributes;
using Console.Types;
using UnityEditor;
using UnityEngine;

namespace Console.Commands
{
    public class UnityEngineCommands : MonoBehaviour
    {
        [Command("time-scale", "Get or set the time scale", "UnityEngine", CommandTargetType.Single)]
        public static float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }
        
        [Command("quit", "Exits game", "UnityEngine", CommandTargetType.Single)]
        public static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static float _lastTimeScale;
        [Command("pause", "Pause game", "UnityEngine", CommandTargetType.Single)]
        public void Pause()
        {
            _lastTimeScale = Time.timeScale;
            TimeScale = 0;
        }
        
        [Command("resume", "Resume game", "UnityEngine", CommandTargetType.Single)]
        public void Resume()
        {
            TimeScale = _lastTimeScale;
        }
        
        [Command("max-fps", "Maximum FPS. Set to -1 for unlimited.", "UnityEngine", CommandTargetType.Single)]
        private static int MaxFPS
        {
            get => Application.targetFrameRate;
            set => Application.targetFrameRate = value;
        }

        [Command("anti-aliasing", "Anti aliasing value. Valid values are 0, 2, 4 and 8.", "UnityEngine", CommandTargetType.Single)]
        private static int AntiAliasing
        {
            get => QualitySettings.antiAliasing;
            set
            {
                if(value != 0 && value != 2 && value != 4 && value != 8)
                {
                    Debug.LogError("Invalid MSAA value. Valid values are 0, 2, 4 and 8.");
                    return;
                }

                QualitySettings.antiAliasing = value;
            }
        }
        
        [Command("full-screen", "Full screen", "UnityEngine", CommandTargetType.Single)]
        private static bool Fullscreen
        {
            get => Screen.fullScreen;
            set => Screen.fullScreen = value;
        }
    }
}