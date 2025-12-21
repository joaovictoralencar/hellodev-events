using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HelloDev.Events.Editor
{
    [CustomEditor(typeof(GameEventBase_SO), true)]
    [CanEditMultipleObjects]
    public class GameEventEditor : UnityEditor.Editor
    {
        private bool _showListeners = true;

        private bool _showEventSection = true;
        private bool _showRuntimeSection = true;

        private SerializedProperty _parameterProperty;
        private object _parameterValue;

        private GUIContent _eventIcon;
        private GUIContent _runtimeIcon;

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // Icons (with safe fallback)
            _eventIcon = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
            if (_eventIcon?.image == null) _eventIcon = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow");

            _runtimeIcon = EditorGUIUtility.IconContent("d_PlayButton");
            if (_runtimeIcon?.image == null) _runtimeIcon = EditorGUIUtility.IconContent("PlayButton");
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                Repaint();
        }

        public override void OnInspectorGUI()
        {
            var gameEvent = target as GameEventBase_SO;

            serializedObject.Update();
            DrawDefaultInspector();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(0);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(0);
            DrawEventInfoSection(gameEvent);
            DrawTestingSection(gameEvent);
            GUILayout.Space(0);
            EditorGUILayout.EndVertical();
            GUILayout.Space(0);
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

// =========================
// Event Info Section
// =========================
        private void DrawEventInfoSection(GameEventBase_SO gameEvent)
        {
            EditorGUILayout.BeginVertical();
            // Custom horizontal layout for the header
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(15);
            var headerContent = new GUIContent(" Event Information", _eventIcon?.image);
            _showEventSection = EditorGUILayout.BeginFoldoutHeaderGroup(_showEventSection, headerContent);

            EditorGUILayout.EndHorizontal(); // Close horizontal wrapper

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (_showEventSection)
            {
                EditorGUILayout.Space(6);

                var listeners = gameEvent.GetListenersForEditor();
                int listenerCount = listeners?.Count ?? 0;

                if (!Application.isPlaying)
                {
                    listeners = gameEvent.GetListenersForEditor();
                    listenerCount = listeners?.Count ?? 0;
                }


                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(4);
                _showListeners = EditorGUILayout.Foldout(_showListeners, $"Subscribed Methods ({listenerCount})", true);
                EditorGUI.indentLevel--;

                if (_showListeners)
                {
                    if (listenerCount == 0)
                    {
                        EditorGUILayout.HelpBox("No methods are currently subscribed to this event.", MessageType.Info);
                    }
                    else
                    {
                        DrawListenersList(listeners);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(4);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }


        private void DrawListenersList(System.Collections.IList listeners)
        {
            EditorGUILayout.Space(2);

            for (int i = 0; i < listeners.Count; i++)
            {
                var listener = listeners[i];
                if (listener == null)
                {
                    EditorGUILayout.LabelField($"{i + 1}. <null listener>");
                    continue;
                }

                if (listener is Delegate delegateListener)
                {
                    var methodInfo = delegateListener.Method;
                    var targetObject = delegateListener.Target;
                    string listenerName = GetListenerDisplayName(methodInfo);
                    string targetName = GetTargetDisplayName(targetObject);

                    EditorGUILayout.BeginHorizontal();
                    var numberStyle = new GUIStyle(EditorStyles.label) { fontSize = 12 };
                    EditorGUILayout.LabelField($"{i + 1}.", numberStyle, GUILayout.Width(25));

                    if (targetObject is Component component)
                    {
                        if (GUILayout.Button($"{targetName} → {listenerName}", EditorStyles.linkLabel))
                        {
                            Selection.activeGameObject = component.gameObject;
                            EditorGUIUtility.PingObject(component.gameObject);
                        }
                    }
                    else if (targetObject is ScriptableObject so)
                    {
                        if (GUILayout.Button($"{targetName} → {listenerName}", EditorStyles.linkLabel))
                        {
                            Selection.activeObject = so;
                            EditorGUIUtility.PingObject(so);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"{targetName} → {listenerName}");
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        // =========================
        // Runtime Testing Section
        // =========================
        private void DrawTestingSection(GameEventBase_SO gameEvent)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5);

            // Custom horizontal layout for the header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);

            var headerContent = new GUIContent(" Runtime Testing", _runtimeIcon?.image);
            _showRuntimeSection = EditorGUILayout.BeginFoldoutHeaderGroup(_showRuntimeSection, headerContent);

            EditorGUILayout.EndHorizontal(); // Close horizontal wrapper
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (_showRuntimeSection)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical();

                EditorGUI.BeginDisabledGroup(!Application.isPlaying);

                var paramType = gameEvent.ParameterType;
                EditorGUILayout.LabelField("Parameter Value:");
                _parameterValue = DrawUnityField(paramType, _parameterValue);

                EditorGUILayout.Space(8);

                if (GUILayout.Button("Raise Event", GUILayout.Height(25)))
                {
                    RaiseEvent(gameEvent);
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Listeners are automatically cleared on reset by default...", MessageType.Info);
                EditorGUILayout.Space(4);

                var oldBgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.83f, 0.04f, 0.17f, 1f);
                if (GUILayout.Button("Clear Listeners", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Clear All Listeners",
                            "Remove all listeners from this event?", "Yes", "Cancel"))
                    {
                        gameEvent.RemoveAllListeners();
                    }
                }

                GUI.backgroundColor = oldBgColor;

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space(4);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }


        private object DrawUnityField(Type fieldType, object currentValue)
        {
            if (fieldType == typeof(int))
                return EditorGUILayout.IntField(currentValue as int? ?? 0);
            else if (fieldType == typeof(float))
                return EditorGUILayout.FloatField(currentValue as float? ?? 0f);
            else if (fieldType == typeof(string))
                return EditorGUILayout.TextField(currentValue as string ?? "");
            else if (fieldType == typeof(bool))
                return EditorGUILayout.Toggle(currentValue as bool? ?? false);
            else if (fieldType == typeof(Vector2))
                return EditorGUILayout.Vector2Field("", currentValue as Vector2? ?? Vector2.zero);
            else if (fieldType == typeof(Vector3))
                return EditorGUILayout.Vector3Field("", currentValue as Vector3? ?? Vector3.zero);
            else if (fieldType == typeof(Color))
                return EditorGUILayout.ColorField(currentValue as Color? ?? Color.white);
            else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
                return EditorGUILayout.ObjectField(currentValue as UnityEngine.Object, fieldType, true);
            else if (fieldType.IsEnum)
                return EditorGUILayout.EnumPopup(currentValue as Enum ?? (Enum)Enum.GetValues(fieldType).GetValue(0));
            else
            {
                EditorGUILayout.LabelField($"Complex type: {fieldType.Name}");
                return currentValue ?? GetDefaultValue(fieldType);
            }
        }

        // =========================
        // Helpers
        // =========================
        private void RaiseEvent(GameEventBase_SO gameEvent)
        {
            try
            {
                var value = _parameterValue ?? GetDefaultValue(gameEvent.ParameterType);
        
                // Add type checking
                if (value != null && !gameEvent.ParameterType.IsAssignableFrom(value.GetType()))
                {
                    Debug.LogError($"Parameter type mismatch. Expected: {gameEvent.ParameterType}, Got: {value.GetType()}");
                    return;
                }

                var raiseMethod = gameEvent.GetType().GetMethod("Raise");
                if (raiseMethod == null)
                {
                    Debug.LogError("Raise method not found");
                    return;
                }

                raiseMethod.Invoke(gameEvent, new[] { value });
            }
            catch (TargetInvocationException tie)
            {
                // Unwrap the inner exception to see the real error
                Debug.LogError($"Failed to raise event: {tie.InnerException?.Message ?? tie.Message}");
                Debug.LogError($"Inner exception: {tie.InnerException}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to raise event: {e.Message}");
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string)) return "";
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type == typeof(Vector2)) return Vector2.zero;
            if (type == typeof(Vector3)) return Vector3.zero;
            if (type == typeof(Color)) return Color.white;
            if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        private string GetListenerDisplayName(MethodInfo method)
        {
            if (method.Name.Contains("<") || method.Name.Contains("lambda"))
                return "Lambda Method";
            return method.Name;
        }

        private string GetTargetDisplayName(object target)
        {
            if (target is Component component)
                return component.gameObject.name;
            if (target is ScriptableObject so)
                return so.name;
            return target?.GetType().Name ?? "Unknown";
        }
    }
}