using System;
using System.Collections.Generic;
using System.Linq;
using Console.Attributes;
using Console.Converters;
using Console.Suggestions;
using Console.Types;
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
            UnityEditor.EditorApplication.isPlaying = false;
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
#if UNITY_EDITOR     
        [Command("spawn-prefab", "Spawn a prefab works only in unity editor", "UnityEngine", CommandTargetType.Single)]
        public static string SpawnPrefab(PrefabResource prefabResource, string name)
        {
            try
            {
                var prefab = Resources.Load<GameObject>(prefabResource.Path);
                var spawnedObject = Instantiate(prefab);
                spawnedObject.name = name;
                return spawnedObject.name;
            }
            catch
            {
                return "Prefab not found.";
            }
        }
        
        [Command("spawn-prefab", "Spawn a prefab at position works only in unity editor", "UnityEngine", CommandTargetType.Single)]
        public static string SpawnPrefab(PrefabResource prefabResource, string name, Vector3 position)
        {
            try
            {
                var prefab = Resources.Load<GameObject>(prefabResource.Path);
                var spawnedObject = Instantiate(prefab, position, Quaternion.identity);
                spawnedObject.name = name;
                return spawnedObject.name;
            }
            catch
            {
                return "Prefab not found.";
            }
        }
#endif
    }

#if UNITY_EDITOR
    public struct PrefabResource
    {
        public readonly string Path;

        public PrefabResource(string path)
        {
            Path = path;
        }
        
        public class PrefabResourceSuggestions : ISuggestion
        {
            public Type Type => typeof(PrefabResource);
            private readonly List<string> _cachedGameObjects;

            public PrefabResourceSuggestions()
            {
                var prefabGuids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources" });

                // Get the names of the prefabs without loading them
                _cachedGameObjects = prefabGuids
                    .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                    .Select(x => x.Replace("Assets/Resources/", "").Replace(".prefab", ""))
                    .ToList();
            }

            public List<string> GetSuggestions(Type type, string input)
            {
                input = input.Trim('"');
                if(string.IsNullOrWhiteSpace(input))
                {
                    return new List<string>();
                }
            
                return _cachedGameObjects
                    .Where(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => x)
                    .ToList();
            }
        }
        
        public class PrefabResourceConverter : IValueConverter
        {
            public Type Type => typeof(PrefabResource);

            public object Convert(string input)
            {
                return new PrefabResource(input);
            }

            public string Convert(object value)
            {
                return ((PrefabResource) value).Path;
            }
        }
    }
#endif
}