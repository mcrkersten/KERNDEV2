using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace V02 {
    public class SettingsMenu : EditorWindow {

        SettingsObject settings;
        int minLimit = 85;
        int maxLimit = 95;
        int tab = 0;
        [MenuItem("Window/CityGenerator")]
        static void ShowWindow() {
            GetWindow<SettingsMenu>("City Settings");
        }

        private void OnGUI() {
            settings = SettingsObject.Instance;
            tab = GUILayout.Toolbar(tab, new string[] { "Highways", "Streets" });
            switch (tab) {
                case 0:
                    Highway();
                    break;

                case 1:
                    Street();
                    break;
            }
        }

        void Highway() {
            var style = new GUIStyle(GUI.skin.label) {
                fontSize = 15,
                fixedHeight = 22,
                alignment = TextAnchor.UpperCenter,
            };
            GUILayout.Space(10);
            GUILayout.Label("Texture Maps", style);

            EditorGUILayout.BeginHorizontal();
            settings.populationMap = TextureField("Population", settings.populationMap);
            settings.waterMap = TextureField("Water", settings.waterMap);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Basic Highway Settings", style);
            EditorGUILayout.BeginHorizontal();
            settings.H_angle = VariableIntField("Max Angle", "The angle of search and maximum turn angle", settings.H_angle, 35);
            settings.H_laserDistance = VariableFloatField("Section Lenght", "The lengh of a road section", settings.H_laserDistance);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            settings.H_roadLength = VariableIntField("Max Lenght", "The max lenght a road may reach", settings.H_roadLength, 100);
            settings.H_roadColor = ColorField("Road Color", "Debug line color", settings.H_roadColor);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            settings.canBranch = VariableBoolField("Highway can branch", settings.canBranch);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (settings.canBranch) {
                minSize = new Vector2(200, 520);
                maxSize = new Vector2(200, 520);
                GUILayout.Label("Branch Settings", style);
                settings.H_branchProbability = IntSliderField("Branch probability", settings.H_branchProbability);
                ProgressBar(settings.H_branchProbability / 100.0f, "Branch probability: " + settings.H_branchProbability + "%");

                EditorGUILayout.BeginHorizontal();
                settings.H_branchAngle = VariableIntField("Branch Angle", "The Angle of the new road", settings.H_branchAngle, 90);
                settings.H_minimalBranchDistance = VariableIntField("Dist branches", "The minimum distance for a new branch", settings.H_minimalBranchDistance, 50);
                EditorGUILayout.EndHorizontal();

                settings.maxHighways = VariableIntField("Max highways", "The maximum amount of highways", settings.maxHighways, 30);

            }
            else {
                minSize = new Vector2(200, 375);
                maxSize = new Vector2(200, 375);
            }



            GUILayout.Space(15);

            if (GUILayout.Button("Build City")) {

            }
        }

        void Street() {
            var style = new GUIStyle(GUI.skin.label) {
                fontSize = 15,
                fixedHeight = 22,
                alignment = TextAnchor.UpperCenter,
            };
            GUILayout.Space(10);

            GUILayout.Label("Basic Street Settings", style);

            EditorGUILayout.BeginHorizontal();
            settings.R_angle = VariableIntField("Turn Angle", "The amount a road can turn without being a intersection", settings.R_angle, 35);
            settings.R_laserDistance = VariableFloatField("Section Lenght", "The lengh of a road section", settings.R_laserDistance);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            settings.R_minPopulation = VariableFloatField("Minimal pop", "Minimal population for a road to spawn", settings.R_minPopulation);
            settings.R_roadColor = ColorField("Road Color", "Debug line color", settings.R_roadColor);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Intersection Settings", style);

            EditorGUILayout.BeginHorizontal();
            settings.R_minAngle = VariableIntField("Min Angle", "The minimal angle a intersaction has on it parent", settings.R_minAngle, 1000);
            settings.R_maxAngle = VariableIntField("Max Angle", "The maximal angle a intersaction has on it parent", settings.R_maxAngle, 1000);
            EditorGUILayout.EndHorizontal();
            AngleVisual(settings.R_maxAngle, settings.R_minAngle);

            settings.R_branchProbability = IntSliderField("Branch probability", settings.R_branchProbability);
            ProgressBar(settings.R_branchProbability / 100.0f, "Branch probability: " + settings.R_branchProbability + "%");
        }

        private static Texture2D TextureField(string name, Texture2D texture) {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.UpperCenter,
                fixedWidth = 90
            };
            GUILayout.Label(name, style);
            texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(90), GUILayout.Height(90));
            GUILayout.EndVertical();
            return texture;
        }

        private static Color ColorField(string name, string tooltip, Color color) {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.UpperCenter,
                fixedWidth = 90
            };

            GUIContent content = new GUIContent(name, tooltip);
            GUILayout.Label(content, style);
            color = EditorGUILayout.ColorField(color, GUILayout.Width(90));
            GUILayout.EndVertical();
            return color;
        }

        private static int VariableIntField(string name, string tooltip, int value, int maxRecommend) {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.UpperCenter,
                fixedWidth = 90
            };

            if (value > maxRecommend) {
                style.normal.textColor = Color.red;
                tooltip = "Max value of " + maxRecommend + " is recommended";
            }

            GUIContent content = new GUIContent(name, tooltip);
            GUILayout.Label(content, style);
            value = EditorGUILayout.IntField(value, GUILayout.Width(90));
            GUILayout.EndVertical();
            return value;
        }

        private static float VariableFloatField(string name, string tooltip, float value) {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.UpperCenter,
                fixedWidth = 90
            };
            GUIContent content = new GUIContent(name, tooltip);
            GUILayout.Label(content, style);
            value = EditorGUILayout.FloatField(value, GUILayout.Width(90));
            GUILayout.EndVertical();
            return value;
        }

        private static bool VariableBoolField(string name, bool value) {
            GUILayout.BeginVertical();
            GUILayout.BeginArea(new Rect((Screen.width / 2) - 65, 295, 200, 100));
            value = GUILayout.Toggle(value, name, GUI.skin.toggle);
            GUILayout.EndArea();
            GUILayout.EndVertical();
            return value;
        }

        private static int IntSliderField(string name, int value) {
            GUILayout.BeginVertical();
            value = EditorGUILayout.IntSlider(value, 0, 100);
            GUILayout.EndVertical();
            return value;
        }

        void ProgressBar(float value, string label) {
            // Get a rect for the progress bar using the same margins as a textfield:
            Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
            EditorGUI.ProgressBar(rect, value, label);
            EditorGUILayout.Space();
        }

        void AngleVisual(float min, float max) {

            if (min > 10 && max > 10) {
                if (settings.R_minAngle > settings.R_maxAngle) {
                    settings.R_maxAngle++;
                }
                if (settings.R_maxAngle < settings.R_minAngle) {
                    settings.R_minAngle--;
                }
                if (settings.R_maxAngle > maxLimit) {
                    settings.R_maxAngle = maxLimit;
                }
                if (settings.R_minAngle < minLimit) {
                    settings.R_minAngle = minLimit;
                }
            }
            EditorGUILayout.MinMaxSlider(ref max, ref min, minLimit, maxLimit);
        }
    }
}
