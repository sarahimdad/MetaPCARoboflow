using System.Collections.Generic;
using UnityEngine;

namespace TutorialSystem
{
    /// <summary>
    /// Static utility class for accessing tutorials without requiring a MonoBehaviour.
    /// Other developers can use this to read tutorial data directly.
    /// </summary>
    public static class TutorialSystem
    {
        /// <summary>
        /// Load a tutorial from Resources folder
        /// </summary>
        public static TutorialData LoadTutorial(string tutorialName)
        {
            return Resources.Load<TutorialData>($"Tutorials/{tutorialName}");
        }

        /// <summary>
        /// Get all step titles from a tutorial
        /// </summary>
        public static List<string> GetStepTitles(TutorialData tutorial)
        {
            if (tutorial == null || tutorial.steps == null)
                return new List<string>();

            var titles = new List<string>();
            foreach (var step in tutorial.steps)
            {
                if (step != null)
                    titles.Add(step.stepTitle);
            }
            return titles;
        }

        /// <summary>
        /// Get all step texts from a tutorial
        /// </summary>
        public static List<string> GetStepTexts(TutorialData tutorial)
        {
            if (tutorial == null || tutorial.steps == null)
                return new List<string>();

            var texts = new List<string>();
            foreach (var step in tutorial.steps)
            {
                if (step != null)
                    texts.Add(step.stepText);
            }
            return texts;
        }

        /// <summary>
        /// Get a specific step by index from a tutorial
        /// </summary>
        public static TutorialStep GetStep(TutorialData tutorial, int stepIndex)
        {
            if (tutorial == null)
                return null;

            return tutorial.GetStep(stepIndex);
        }

        /// <summary>
        /// Get a specific step by step number from a tutorial
        /// </summary>
        public static TutorialStep GetStepByNumber(TutorialData tutorial, int stepNumber)
        {
            if (tutorial == null)
                return null;

            return tutorial.GetStepByNumber(stepNumber);
        }
    }
}

