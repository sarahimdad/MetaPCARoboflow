using System.Collections.Generic;
using UnityEngine;

namespace TutorialSystem
{
    /// <summary>
    /// Helper class to manage and access tutorial data.
    /// Other developers can use this to read step names and display them.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("Tutorial References")]
        [Tooltip("List of tutorial data assets to manage")]
        public List<TutorialData> tutorials = new List<TutorialData>();

        /// <summary>
        /// Get a tutorial by index
        /// </summary>
        public TutorialData GetTutorial(int index)
        {
            if (tutorials == null || index < 0 || index >= tutorials.Count)
                return null;
            return tutorials[index];
        }

        /// <summary>
        /// Get all step titles from a specific tutorial
        /// </summary>
        public List<string> GetStepTitles(int tutorialIndex)
        {
            var tutorial = GetTutorial(tutorialIndex);
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
        /// Get all step texts from a specific tutorial
        /// </summary>
        public List<string> GetStepTexts(int tutorialIndex)
        {
            var tutorial = GetTutorial(tutorialIndex);
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
        /// Get all steps from a specific tutorial
        /// </summary>
        public List<TutorialStep> GetSteps(int tutorialIndex)
        {
            var tutorial = GetTutorial(tutorialIndex);
            if (tutorial == null || tutorial.steps == null)
                return new List<TutorialStep>();

            return new List<TutorialStep>(tutorial.steps);
        }
    }
}

