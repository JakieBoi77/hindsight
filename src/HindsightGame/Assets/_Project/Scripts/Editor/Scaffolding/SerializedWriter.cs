using System;
using System.Collections.Generic;
using Hindsight.Procedures;
using Hindsight.Procedures.Steps;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hindsight.Editor.Scaffolding
{
    /// <summary>
    /// Fluent helper for writing private [SerializeField] values from editor tooling, so runtime
    /// classes need no public setters. Unknown field names throw immediately, which keeps the
    /// scaffolder honest when fields are renamed.
    /// </summary>
    internal sealed class SerializedWriter
    {
        private readonly SerializedObject serializedObject;

        private SerializedWriter(Object target)
        {
            serializedObject = new SerializedObject(target);
        }

        public static SerializedWriter For(Object target)
        {
            return new SerializedWriter(target);
        }

        public SerializedWriter Ref(string field, Object value)
        {
            Find(field).objectReferenceValue = value;
            return this;
        }

        public SerializedWriter Refs<T>(string field, IReadOnlyList<T> values)
            where T : Object
        {
            var property = Find(field);
            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            return this;
        }

        public SerializedWriter String(string field, string value)
        {
            Find(field).stringValue = value;
            return this;
        }

        public SerializedWriter Int(string field, int value)
        {
            Find(field).intValue = value;
            return this;
        }

        public SerializedWriter Float(string field, float value)
        {
            Find(field).floatValue = value;
            return this;
        }

        public SerializedWriter Bool(string field, bool value)
        {
            Find(field).boolValue = value;
            return this;
        }

        public SerializedWriter Color(string field, Color value)
        {
            Find(field).colorValue = value;
            return this;
        }

        public SerializedWriter Enum(string field, int index)
        {
            Find(field).enumValueIndex = index;
            return this;
        }

        public SerializedWriter Line(string field, DialogueLine line)
        {
            WriteLine(Find(field), line);
            return this;
        }

        public SerializedWriter Lines(string field, params DialogueLine[] lines)
        {
            var property = Find(field);
            property.arraySize = lines.Length;
            for (var i = 0; i < lines.Length; i++)
            {
                WriteLine(property.GetArrayElementAtIndex(i), lines[i]);
            }

            return this;
        }

        public SerializedWriter Answers(string field, params QuizAnswer[] answers)
        {
            var property = Find(field);
            property.arraySize = answers.Length;
            for (var i = 0; i < answers.Length; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("text").stringValue = answers[i].Text;
                element.FindPropertyRelative("icon").objectReferenceValue = answers[i].Icon;
            }

            return this;
        }

        public SerializedWriter Poses(string field, params (DoctorPose Pose, Sprite Sprite)[] poses)
        {
            var property = Find(field);
            property.arraySize = poses.Length;
            for (var i = 0; i < poses.Length; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("pose").enumValueIndex = (int)poses[i].Pose;
                element.FindPropertyRelative("sprite").objectReferenceValue = poses[i].Sprite;
            }

            return this;
        }

        public void Apply()
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WriteLine(SerializedProperty property, DialogueLine line)
        {
            property.FindPropertyRelative("text").stringValue = line.Text;
            property.FindPropertyRelative("pose").enumValueIndex = (int)line.Pose;
            property.FindPropertyRelative("narration").objectReferenceValue = line.Narration;
        }

        private SerializedProperty Find(string field)
        {
            var property = serializedObject.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"{serializedObject.targetObject.GetType().Name} has no serialized field '{field}'.");
            }

            return property;
        }
    }
}
