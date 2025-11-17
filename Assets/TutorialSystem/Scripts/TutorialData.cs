using System.Collections.Generic;
using UnityEngine;

namespace TutorialSystem
{
    [CreateAssetMenu(fileName = "New Tutorial", menuName = "Tutorial System/Tutorial Data", order = 1)]
    public class TutorialData : ScriptableObject
    {
        [Header("Tutorial Information")]
        [Tooltip("The title of the tutorial (for prefab title part)")]
        public string tutorialTitle;

        [Header("Tutorial Steps")]
        [Tooltip("List of steps in this tutorial")]
        public List<TutorialStep> steps = new List<TutorialStep>();

        /// <summary>
        /// Get the total number of steps in this tutorial
        /// </summary>
        public int StepCount => steps != null ? steps.Count : 0;

        /// <summary>
        /// Get a specific step by index (0-based)
        /// </summary>
        public TutorialStep GetStep(int index)
        {
            if (steps == null || index < 0 || index >= steps.Count)
                return null;
            return steps[index];
        }

        /// <summary>
        /// Get a specific step by step number (1-based)
        /// </summary>
        public TutorialStep GetStepByNumber(int stepNumber)
        {
            if (steps == null)
                return null;

            foreach (var step in steps)
            {
                if (step != null && step.stepNumber == stepNumber)
                    return step;
            }
            return null;
        }
    }
}

