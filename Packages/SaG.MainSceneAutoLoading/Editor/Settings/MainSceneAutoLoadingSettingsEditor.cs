﻿using System;
using System.Linq;
using SaG.MainSceneAutoLoading.MainSceneLoadedHandlers;
using SaG.MainSceneAutoLoading.MainSceneProviders;
using SaG.MainSceneAutoLoading.PlaymodeExitedHandlers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaG.MainSceneAutoLoading.Settings
{
    [CustomEditor(typeof(MainSceneAutoLoadingSettings))]
    public class MainSceneAutoLoadingSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            float width = GetViewWidth();
            EditorGUILayout.LabelField($"Width: {width}");
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;//width / 2;


            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings.Enabled)));

            DrawRealization(serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings._mainSceneProvider)),
                typeof(IMainSceneProvider));

            DrawRealization(serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings._mainSceneLoadedHandler)),
                typeof(IMainSceneLoadedHandler));
            
            DrawRealization(serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings._playmodeExitedHandler)),
                typeof(IPlaymodeExitedHandler));

            EditorGUIUtility.labelWidth = labelWidth;
        }

        private Rect _rect;

        private float GetViewWidth()
        {
            GUILayout.Label("hack", GUILayout.MaxHeight(0));
            if (Event.current.type == EventType.Repaint)
            {
                // hack to get real view width
                _rect = GUILayoutUtility.GetLastRect();
            }

            return _rect.width;
        }

        private void DrawRealization(SerializedProperty serializedProperty, Type addType)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent(serializedProperty.displayName));
            var typeName = serializedProperty.managedReferenceFullTypename.Split('.').Last();
            typeName = ObjectNames.NicifyVariableName(typeName);
            if (EditorGUILayout.DropdownButton(new GUIContent(typeName), FocusType.Keyboard, GUILayout.MinWidth(10)))
            {
                var menu = new GenericMenu();

                var foundTypes = TypeCache.GetTypesDerivedFrom(addType);
                for (int i = 0; i < foundTypes.Count; ++i)
                {
                    var type = foundTypes[i];

                    if (type.IsAbstract)
                        continue;
                    
                    if (type.IsSubclassOf(typeof(Object)))
                        continue;

                    // if (RequiredAttribute != null && !Attribute.IsDefined(type, RequiredAttribute))
                    //     continue;

                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                    {
                        serializedProperty.managedReferenceValue = Activator.CreateInstance(type);
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                    });
                }

                if (menu.GetItemCount() == 0)
                {
                    menu.AddDisabledItem(new GUIContent($"No implementations of {addType}"));
                    Debug.LogError($"Something went really wrong. Can't find any implementation of type {addType}");
                }

                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(serializedProperty, GUIContent.none, true);
        }
    }
}