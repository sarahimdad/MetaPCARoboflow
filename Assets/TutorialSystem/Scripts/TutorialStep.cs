using System;
using UnityEngine;
using UnityEngine.Video;

namespace TutorialSystem
{
    [Serializable]
    public class TutorialStep
    {
        [Tooltip("The step number (e.g., 1, 2, 3)")]
        public int stepNumber;

        [Tooltip("The title/heading for this step")]
        public string stepTitle;

        [Tooltip("The text content for this step")]
        [TextArea(3, 10)]
        public string stepText;

        [Header("Media")]
        [Tooltip("Image/Texture to display for this step (optional). Can use Texture2D or Sprite. For GIFs, convert to sprite sequence or video.")]
        public Texture2D stepImage;

        [Tooltip("Video clip to display for this step (optional)")]
        public VideoClip stepVideo;
    }
}

