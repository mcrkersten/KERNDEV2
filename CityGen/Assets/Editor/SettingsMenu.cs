using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SettingsMenu : EditorWindow {
    SettingsObject settings;

    [MenuItem("Window/CityGenerator")]
    static void ShowWindow() {
        GetWindow<SettingsMenu>("City Settings");
    }

    private void OnGUI() {
        settings = SettingsObject.Instance;

        var style = new GUIStyle(GUI.skin.label) {
            fontSize = 15,
            fixedHeight = 22,
            alignment = TextAnchor.UpperCenter,
        };
        GUILayout.Space(15);
        GUILayout.Label("Texture Maps",style);

        EditorGUILayout.BeginHorizontal();
            settings.populationMap = TextureField("Population", settings.populationMap);
            settings.waterMap = TextureField("Water", settings.waterMap);            
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Label("Basic Highway Settings", style);
        EditorGUILayout.BeginHorizontal();
            settings.angles = VariableIntField("Max Angle", "The angle of search and maximum turn angle", settings.angles,35);
            settings.laserDistance = VariableFloatField("Section Lenght", "The lengh of a road section", settings.laserDistance);          
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
            settings.roadLength = VariableIntField("Max Lenght", "The max lenght a road may reach", settings.roadLength, 100);
            settings.roadColor = ColorField("Road Color", "Debug line color", settings.roadColor);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();
            settings.canBranch = VariableBoolField("Highway can branch", settings.canBranch);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (settings.canBranch) {
            minSize = new Vector2(200, 490);
            maxSize = new Vector2(200, 490);
            GUILayout.Label("Branch Settings", style);
            settings.branchProbability = IntSliderField("Branch probability", settings.branchProbability);
            ProgressBar(settings.branchProbability / 100.0f, "Branch probability: " + settings.branchProbability +"%");

            EditorGUILayout.BeginHorizontal();
            settings.branchAngle = VariableIntField("Branch Angle", "The Angle of the new road", settings.branchAngle,90);
            settings.minimalBranchDistance = VariableIntField("Dist branches", "The minimum distance for a new branch", settings.minimalBranchDistance,50);
            EditorGUILayout.EndHorizontal();

            settings.maxHighways = VariableIntField("Max highways", "The maximum amount of highways",settings.maxHighways,30);

        }
        else {
            minSize = new Vector2(200, 350);
            maxSize = new Vector2(200, 350);
        }



        GUILayout.Space(15);
        
        if (GUILayout.Button("Build City")) {

        }
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
        GUILayout.BeginArea(new Rect((Screen.width / 2) - 65, 275, 200, 100));
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
}
