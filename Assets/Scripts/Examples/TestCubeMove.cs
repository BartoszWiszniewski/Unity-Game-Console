using System;
using System.Collections.Generic;
using Console.Attributes;
using Console.Types;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples
{
    public class TestCubeMove : MonoBehaviour
    {
        [SerializeField]
        private float speed = 5.0f;
        
        [SerializeField]
        private List<Transform> waypoints = new List<Transform>();
        
        private int _currentWaypointIndex = 0;
        
        [Command("CubeSpeed", "Set cube speed", group: "Cube", target: CommandTargetType.All)]
        public float Speed
        {
            get => speed;
            set => speed = value;
        }
        
        [Command("CubesSetSpeed", "Set target cube speed", group: "Cube", target: CommandTargetType.Single)]
        public static void SetTargetCubeSpeed(GameObject cube, float speed)
        {
            if (cube == null)
            {
                Debug.LogError("GameObject is null");
            }
            
            var cubeMove = cube.GetComponent<TestCubeMove>();
            if (cubeMove != null)
            {
                cubeMove.speed = speed;
            }
            else
            {
                Debug.LogError($"GameObject {cube.name} does not have TestCubeMove component");
            }
        }
        
        [Command("CubeAddWayPoint", "Add waypoint to cube", group: "Cube", target: CommandTargetType.Single)]
        public static void AddWayPoint(GameObject cube, Vector3 position)
        {
            if (cube == null)
            {
                Debug.LogError("GameObject is null");
            }
            
            var cubeMove = cube.GetComponent<TestCubeMove>();
            if (cubeMove != null)
            {
                var newWaypoint = new GameObject($"Waypoint{cubeMove.waypoints.Count}")
                {
                    transform =
                    {
                        position = position
                    }
                };
                newWaypoint.transform.SetParent(cube.transform.parent);
                cubeMove.waypoints.Add(newWaypoint.transform);
            }
            else
            {
                Debug.LogError($"GameObject {cube.name} does not have TestCubeMove component");
            }
        }

        private void Update()
        {
            if (waypoints.Count == 0)
            {
                return;
            }
            
            var targetPosition = waypoints[_currentWaypointIndex].position;
            var direction = targetPosition - transform.position;
            var distance = speed * Time.deltaTime;
            
            if (direction.magnitude <= distance)
            {
                transform.position = targetPosition;
                _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
            }
            else
            {
                transform.position += direction.normalized * distance;
            }
            
        }
    }
}