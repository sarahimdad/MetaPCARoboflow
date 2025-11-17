using UnityEngine;
using UnityEditor;

namespace TutorialSystem
{
    [CustomPropertyDrawer(typeof(TutorialStep))]
    public class TutorialStepDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw the foldout
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), 
                property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Step Number
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("stepNumber"));
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Step Title
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("stepTitle"));
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Step Text
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight * 3),
                    property.FindPropertyRelative("stepText"));
                yOffset += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing;

                // Header for Media
                EditorGUI.LabelField(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight),
                    "Media", EditorStyles.boldLabel);
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Step Image - Custom ObjectField with drag-and-drop and object picker support
                SerializedProperty stepImageProp = property.FindPropertyRelative("stepImage");
                // Make the field taller to provide a larger drop zone
                float imageFieldHeight = EditorGUIUtility.singleLineHeight * 2.5f;
                Rect imageRect = new Rect(position.x, position.y + yOffset, position.width, imageFieldHeight);
                
                // Handle drag and drop events for textures and sprites
                Event evt = Event.current;
                if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
                {
                    if (imageRect.Contains(evt.mousePosition))
                    {
                        bool isValid = false;
                        Texture2D targetTexture = null;
                        
                        foreach (Object obj in DragAndDrop.objectReferences)
                        {
                            if (obj is Texture2D texture)
                            {
                                isValid = true;
                                targetTexture = texture;
                                break;
                            }
                            else if (obj is Sprite sprite)
                            {
                                // Convert sprite to texture by getting its texture
                                if (sprite.texture != null)
                                {
                                    isValid = true;
                                    targetTexture = sprite.texture;
                                    break;
                                }
                            }
                        }
                        
                        if (isValid)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            
                            if (evt.type == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();
                                stepImageProp.objectReferenceValue = targetTexture;
                                stepImageProp.serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(stepImageProp.serializedObject.targetObject);
                                evt.Use();
                                GUI.changed = true;
                            }
                        }
                        else
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                        }
                    }
                }
                
                // Use ObjectField directly - this will show all Texture2D assets in the project
                EditorGUI.BeginChangeCheck();
                Object textureValue = EditorGUI.ObjectField(
                    imageRect,
                    new GUIContent("Step Image", stepImageProp.tooltip),
                    stepImageProp.objectReferenceValue,
                    typeof(Texture2D),
                    false); // false = don't allow scene objects, only assets
                
                if (EditorGUI.EndChangeCheck())
                {
                    stepImageProp.objectReferenceValue = textureValue;
                    stepImageProp.serializedObject.ApplyModifiedProperties();
                }
                yOffset += imageFieldHeight + EditorGUIUtility.standardVerticalSpacing * 2;
                
                // Add extra spacing between image and video fields for easier drag-and-drop
                yOffset += EditorGUIUtility.singleLineHeight; // Add a full line of space

                // Step Video
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("stepVideo"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight; // Foldout
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Step Number
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Step Title
            height += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing; // Step Text (TextArea)
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Media Header
            float imageFieldHeight = EditorGUIUtility.singleLineHeight * 2.5f;
            height += imageFieldHeight + EditorGUIUtility.standardVerticalSpacing * 2; // Step Image (taller for drag-and-drop)
            height += EditorGUIUtility.singleLineHeight; // Extra spacing between image and video
            height += EditorGUIUtility.singleLineHeight; // Step Video

            return height;
        }
    }
}

