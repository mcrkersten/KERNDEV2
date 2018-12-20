using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsObject : MonoBehaviour {

    private static SettingsObject instance = null;
    public static SettingsObject Instance
    {
        get {
            if (instance == null) {
                // This is where the magic happens.
                instance = FindObjectOfType(typeof(SettingsObject)) as SettingsObject;
            }

            // If it is still null, create a new instance
            if (instance == null) {
                GameObject i = new GameObject("Setings");
                i.AddComponent(typeof(SettingsObject));
                instance = i.GetComponent<SettingsObject>();
            }
            return instance;
        }
    }
    [Header("Textures")]
    public Texture2D populationMap;
    public Texture2D waterMap;

    [Header("Basic Highway Setings")]
    public int angles;
    public int roadLength;
    public float laserDistance;
    public bool canBranch;
    public Color roadColor;

    [Header("Highway constraint Setings")]
    public bool followPopulationMap;
    public bool followWaterMap;
    public bool followWaterBody;

    [Header("Highway branching")]
    [Tooltip("Is in percentage and by steps of 1"), Range(0, 100)]
    public int branchProbability;
    public int branchAngle;
    public int minimalBranchDistance;
    public int maxHighways;
    [HideInInspector]
    public int currentHighways;

    [HideInInspector]
    public List<int> xPos;
    public List<int> zPos;

    [Header("V1 setting")]
    public int rays;
}
