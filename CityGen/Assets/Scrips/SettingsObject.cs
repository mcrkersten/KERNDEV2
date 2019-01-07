using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace V02 {
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
        public int H_angle;
        public int H_roadLength;
        public float H_laserDistance;
        public bool canBranch;
        public Color H_roadColor;

        [Header("Highway constraint Setings")]
        public bool followPopulationMap;
        public bool followWaterMap;
        public bool followWaterBody;

        [Header("Highway branching")]
        [Tooltip("Is in percentage and by steps of 1"), Range(0, 100)]
        public int H_branchProbability;
        public int H_branchAngle;
        public int H_minimalBranchDistance;
        public int maxHighways;
        [HideInInspector]
        public int currentHighways;

        [HideInInspector]
        public List<Vector2> occupiedXY;
        public List<Vector2> proposedXY;
        public List<Vector2> existingCrossing;
        public List<float> existingCrossingYrot;

        [Header("V1 setting")]
        public int rays;

        [Header("Street Settings")]
        public int R_angle;
        public int R_minAngle = 89;
        public int R_maxAngle = 91;
        public float R_minPopulation;
        public float R_laserDistance;
        public int R_minimalBranchDistance;
        public int R_branchProbability;
        public Color R_roadColor;

        public List<GameObject> roads = new List<GameObject>();
        public List<GameObject> newRoads = new List<GameObject>();
        public List<GameObject> removeRoads = new List<GameObject>();

        public void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                go();
            }
        }

        void go() {
            StartCoroutine(Example());
        }

        IEnumerator Example() {
            for(int i = 0; i < roads.Count; i++) {
                roads[i].GetComponent<StreetGeneratorV01>().BuildLoop();
                yield return new WaitForSeconds(.0001f);
            }
            NextRound();
        }

        void NextRound() {
            foreach (GameObject i in removeRoads) {
                roads.Remove(i);
                Destroy(i);
            }
            removeRoads.Clear();
            roads.AddRange(newRoads);
            newRoads.Clear();
            if (roads.Count != 0) {
                go();
            }
            else {
                CreateBlocks();
            }
        }

        void CreateBlocks() {
            int x = 0;
            GameObject h = new GameObject("BlockPoints");
            foreach(Vector2 i in existingCrossing) {
                GameObject t = new GameObject("blockPoint" + x);
                t.transform.parent = h.transform;
                t.transform.position = new Vector3(i.x, 0, i.y);
                t.transform.eulerAngles = new Vector3(0, existingCrossingYrot[x], 0);
                x++;
            }
        }
    }
}
