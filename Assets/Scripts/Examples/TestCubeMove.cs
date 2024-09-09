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
        
        [Command("SetCubeSpeed", "Set target cube speed", group: "Cube", target: CommandTargetType.Single)]
        public static void SetTargetCubeSpeed(GameObject gameObject, float speed)
        {
            if (gameObject == null)
            {
                Debug.LogError("GameObject is null");
            }
            
            var cubeMove = gameObject.GetComponent<TestCubeMove>();
            if (cubeMove != null)
            {
                cubeMove.speed = speed;
            }
            else
            {
                Debug.LogError($"GameObject {gameObject.name} does not have TestCubeMove component");
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