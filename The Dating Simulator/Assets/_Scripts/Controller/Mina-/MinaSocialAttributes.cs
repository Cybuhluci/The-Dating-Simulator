using UnityEngine;

namespace Luci
{
    public class MinaSocialAttributes : MonoBehaviour
    {
        [Header("Player Social Stats")]
        public int Speed = 1;
        public int Strength = 1;
        public int Flight = 1;
        public int Style = 1;
        public int Technique = 1;

        // Method to increase a stat
        public void IncreaseStat(string statName, int amount)
        {
            switch (statName)
            {
                case "Speed":
                    Speed += amount;
                    break;
                case "Strength":
                    Strength += amount;
                    break;
                case "Flight":
                    Flight += amount;
                    break;
                case "Style":
                    Style += amount;
                    break;
                case "Technique":
                    Technique += amount;
                    break;
                default:
                    Debug.LogWarning($"Invalid stat name: {statName}");
                    break;
            }

            Debug.Log($"{statName} increased by {amount}. Current level: {GetStatLevel(statName)}");
        }

        // Method to get the current level of a stat
        public int GetStatLevel(string statName)
        {
            return statName switch
            {
                "Speed" => Speed,
                "Strength" => Strength,
                "Flight" => Flight,
                "Style" => Style,
                "Technique" => Technique,
                _ => 0,
            };
        }
    }
}