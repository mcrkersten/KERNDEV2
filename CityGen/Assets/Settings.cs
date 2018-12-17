using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour {

    private static Settings instance = null;
    public static Settings Instance
    {
        get {
            if (instance == null) {
                // This is where the magic happens.
                instance = FindObjectOfType(typeof(Settings)) as Settings;
            }

            // If it is still null, create a new instance
            if (instance == null) {
                throw new System.ArgumentException("FATAL ERROR: No Settings?");
            }
            return instance;
        }
    }

    [Header("Textures")]
    public Texture2D populationMap;
    public Texture2D waterMap;

    [Header("Highway Setings")]
    public int angles;
    public int rays;
    public float laserDistance;
    public bool followPopulationMap;
    public bool followWaterMap;
    public bool followWaterBody;

    [Header("Highway branching")]
    [Tooltip("Is in percentage and by steps of 1"), Range(0,100)]
    public int branchProbability;
    public int branchAngle;
    public int minimalBranchDistance;
}
