using Console;
using Console.Attributes;
using Console.Types;
using UnityEngine;

namespace Examples
{
    public class TestExamples : MonoBehaviour
    {
        [Command("TestEnum", "Test enum", group: "Test", target: CommandTargetType.Single)]
        public TestEnum TestEnum { get; set; }

        [Command("TestLog", "Test log", group: "Test", target: CommandTargetType.Single)]
        public void TestLog()
        {
            Debug.Log("Test log");
        }
        
        [Command("TestError", "Test error", group: "Test", target: CommandTargetType.Single)]
        public void TestError()
        {
            Debug.LogError("Test error");
        }
        
        [Command("TestWarning", "Test warning", group: "Test", target: CommandTargetType.Single)]
        public void TestWarning()
        {
            Debug.LogWarning("Test warning");
        }
    }
}