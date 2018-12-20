using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace V02 {
    public class HighwayGeneratorV02 : MonoBehaviour {

        private SettingsObject settings;

        [Header("Highway Branch object")]
        public GameObject BranchPrfab;
        public Material Mat;
        private Color roadColor;
        private int branchDistance;
        private bool canBranch;
        private int minBranchDistance;
        private int branchProb;
        private int maxLenght;
        private int curLenght = 0;

        //private Transform approvedPossition;
        private GameObject laserPos;

        //Settings
        private int angle;
        private float laserDistance;

        private Texture2D populationMap;
        private Texture2D waterMap;

        [Header("Debug visualization")]
        public LineRenderer lr;
        private Vector3 debugPos;

        // Use this for initialization
        void Start() {
            InitSettings();
            InitLaserPosition();
            InitLineRenderer();
            debugPos = this.gameObject.transform.position;
            settings.currentHighways++;
        }

        void InitSettings() {
            settings = SettingsObject.Instance;
            angle = settings.angles;
            laserDistance = settings.laserDistance;
            populationMap = settings.populationMap;
            waterMap = settings.waterMap;
            canBranch = settings.canBranch;
            minBranchDistance = settings.minimalBranchDistance;
            branchProb = settings.branchProbability;
            maxLenght = settings.roadLength;
            roadColor = settings.roadColor;
        }

        void InitLineRenderer() {
            //Debug linerenderer
            lr = this.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, laserPos.transform.position);
        }

        void InitLaserPosition() {
            laserPos = new GameObject("laser");
            laserPos.transform.parent = this.transform;
            laserPos.transform.localPosition = new Vector3(laserDistance, 0, 0);
            laserPos.transform.rotation = this.transform.rotation;
        }

        private void Update() {
            if (Input.anyKey) {
                if (this.transform.position.x > 0 && this.transform.position.x < settings.populationMap.width && this.transform.position.z > 0 && this.transform.position.z < settings.populationMap.height) {
                    BuildFreeway();
                }
            }
        }

        void BuildFreeway() {
            if(curLenght < maxLenght) {
                curLenght++;
                GetBestPosition(); //<- Calls Constraints and BuildRoad();
                DrawNewRoad();
            }
        }

        //Get the best position for a new roadsegment.
        //Calls global goals and local constraints;
        void GetBestPosition() {
            List<int> x = new List<int>(); //Position
            List<int> z = new List<int>(); //Position
            List<float> y = new List<float>(); //Rotation

            //Get all positions to check.
            float rotationY = this.transform.eulerAngles.y - (angle / 2);
            for (int i = 0; i < angle; i++) {
                rotationY = rotationY + 1;
                this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, rotationY, this.transform.eulerAngles.z);
                x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
                z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
                y.Add(rotationY);
            }

            Vector3 bestOnPopMap = PopulationConstraints(x, z, y);
            bool waterConstraints = WaterConstraints(Mathf.RoundToInt(bestOnPopMap.z), Mathf.RoundToInt(bestOnPopMap.x));
            BuildRoad(waterConstraints, bestOnPopMap);
        }

        Vector3 PopulationConstraints(List<int> x, List<int> z, List<float> y) {
            float heighest = 1;
            Vector3 heighestPopPos = new Vector3();
            for (int i = 0; i < x.Count; i++) {
                float heighestPopulation = populationMap.GetPixel(x[i], z[i]).grayscale;
                if(heighestPopulation < heighest) {
                    heighestPopPos = new Vector3(x[i], y[i], z[i]);
                    heighest = heighestPopulation;
                }
            }
            return heighestPopPos;
        }

        bool WaterConstraints(int x, int z) {
            float waterAmount = waterMap.GetPixel(x, z).grayscale;
            if(waterAmount < .5) {
                print("k");
                return false; //To much water
            }
            return true;
        }

        //Draws line between old and new point to make road.
        void DrawNewRoad() {
            GameObject newLine = new GameObject("Highway-Section");

            //Line renderer
            newLine.transform.position = this.transform.position;
            LineRenderer nlr = newLine.AddComponent<LineRenderer>();
            nlr.material = Mat;
            nlr.startColor = roadColor;
            nlr.endColor = roadColor;
            nlr.positionCount = 2;
            nlr.SetPosition(0, this.transform.position);
            nlr.SetPosition(1, debugPos);
            nlr.startWidth = 10;
            nlr.endWidth = 10;
            debugPos = this.transform.position;
        }

        void BuildRoad(bool noWater, Vector3 position) {
            if (noWater == true) {

                settings.xPos.Add(Mathf.RoundToInt(position.x));
                settings.zPos.Add(Mathf.RoundToInt(position.z));

                this.transform.position = new Vector3(position.x, 0, position.z);
                this.transform.eulerAngles = new Vector3(0, position.y, 0);
                //Reset Debug LineRenderer
                lr.SetPosition(0, this.transform.position);
                lr.SetPosition(1, laserPos.transform.position);

                if(settings.currentHighways < settings.maxHighways) {
                    if (canBranch) {
                        branchDistance++;
                        int spawnNumber = Random.Range(0, 100 + 1);
                        if (branchDistance > minBranchDistance && spawnNumber < branchProb) {
                            GameObject branch = Instantiate(BranchPrfab, null);
                            branch.GetComponent<HighwayGeneratorV02>().InitBranch(this.transform.eulerAngles, this.transform.position);
                            branchDistance = 0;
                        }
                    }
                }
            }
            else {
                Destroy(this.gameObject);
            }
        }

        public void InitBranch(Vector3 rot, Vector3 pos) {
            Start();
            this.transform.position = pos;

            List<int> x = new List<int>(); //Position
            List<int> z = new List<int>(); //Position
            List<float> y = new List<float>(); //Rotation

            this.transform.eulerAngles = new Vector3(rot.x, rot.y + settings.branchAngle, rot.z);
            x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
            z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
            y.Add(rot.y + settings.branchAngle);

            this.transform.eulerAngles = new Vector3(rot.x, rot.y - settings.branchAngle, rot.z);
            x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
            z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
            y.Add(rot.y - settings.branchAngle);

            Vector3 bestOnPopMap = PopulationConstraints(x, z, y);
            bool waterConstraints = WaterConstraints(Mathf.RoundToInt(bestOnPopMap.z), Mathf.RoundToInt(bestOnPopMap.x));
            this.transform.eulerAngles = new Vector3(rot.x,bestOnPopMap.y, rot.z);
        }
    }
}
