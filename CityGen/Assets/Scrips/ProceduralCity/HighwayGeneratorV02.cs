using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace V02 {
    public class HighwayGeneratorV02 : MonoBehaviour {

        private SettingsObject settings;

        [Header("Highway Branch object")]
        public GameObject BranchPrfab;
        public GameObject roadBranch;
        public Material Mat;
        private Color roadColor;
        private int branchDistance;
        private bool canBranch;
        private int minBranchDistance;
        private int branchProb;
        private int maxLenght;
        private int curLenght = 0;

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

        //Get all settings from settingsObject
        void InitSettings() {
            settings = SettingsObject.Instance;
            angle = settings.H_angle;
            laserDistance = settings.H_laserDistance;
            populationMap = settings.populationMap;
            waterMap = settings.waterMap;
            canBranch = settings.canBranch;
            minBranchDistance = settings.H_minimalBranchDistance;
            branchProb = settings.H_branchProbability;
            maxLenght = settings.H_roadLength;
            roadColor = settings.H_roadColor;
        }

        //Instantiate LineRenderer
        void InitLineRenderer() {
            //Debug linerenderer
            lr = this.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, laserPos.transform.position);
        }

        //Create Laser
        void InitLaserPosition() {
            laserPos = new GameObject("laserHighway");
            laserPos.transform.parent = this.transform;
            laserPos.transform.localPosition = new Vector3(laserDistance, 0, 0);
            laserPos.transform.rotation = this.transform.rotation;
        }

        private void Update() {
            if (Input.anyKey) {
                if (this.transform.position.x > 0 && this.transform.position.x < settings.populationMap.width && this.transform.position.z > 0 && this.transform.position.z < settings.populationMap.height) {
                    NewStreet();
                    BuildFreeway();
                }
            }
        }

        //Start loop for highway creation
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

        //Test on populationMap
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

        //Test on waterMap and return result
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
            nlr.startWidth = 5;
            nlr.endWidth = 5;
            nlr.sortingOrder = 1;
            debugPos = this.transform.position;
            newLine.transform.parent = settings.transform;
        }

        //Place new point if all tests are correct or disable this object
        void BuildRoad(bool noWater, Vector3 position) {
            if (noWater == true) {
                Vector2 p = RoadCrossing(position);
                if (p != new Vector2(0, 0)) {
                    this.transform.position = new Vector3(p.x, 0, p.y);
                    this.transform.eulerAngles = new Vector3(0, 0, 0);
                    DestroyHighwayGenerator();
                }



                //Set new occupied position
                settings.occupiedXY.Add(new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z)));

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
                DestroyHighwayGenerator();
            }
        }

        //Check for roadcrossings and handle them
        Vector2 RoadCrossing(Vector3 position) {
            if(curLenght > 3) {
                foreach (Vector2 x in settings.occupiedXY) {
                    if (position.x < x.x + (laserDistance/3) && position.x > x.x - (laserDistance/3)) {
                        if (position.z < x.y + (laserDistance / 3) && position.z > x.y - (laserDistance / 3)) {
                            return x;
                        }
                    }
                }
            }
            return new Vector2(0,0);
        }

        //Create new highway
        public void InitBranch(Vector3 rot, Vector3 pos) {
            Start();
            this.transform.position = pos;

            List<int> x = new List<int>(); //Position
            List<int> z = new List<int>(); //Position
            List<float> y = new List<float>(); //Rotation

            this.transform.eulerAngles = new Vector3(rot.x, rot.y + settings.H_branchAngle, rot.z);
            x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
            z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
            y.Add(rot.y + settings.H_branchAngle);

            this.transform.eulerAngles = new Vector3(rot.x, rot.y - settings.H_branchAngle, rot.z);
            x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
            z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
            y.Add(rot.y - settings.H_branchAngle);

            Vector3 bestOnPopMap = PopulationConstraints(x, z, y);
            bool waterConstraints = WaterConstraints(Mathf.RoundToInt(bestOnPopMap.z), Mathf.RoundToInt(bestOnPopMap.x));
            this.transform.eulerAngles = new Vector3(rot.x,bestOnPopMap.y, rot.z);
        }

        //Create new street
        public void NewStreet() {
            Vector3 x = this.transform.eulerAngles;

            //Kijk naar +~90 graden
            float rot = Random.Range(settings.R_minAngle, settings.R_maxAngle);
            this.transform.Rotate(new Vector3(0,rot,0));
            if(populationMap.GetPixel(Mathf.RoundToInt(laserPos.transform.position.x), Mathf.RoundToInt(laserPos.transform.position.z)).grayscale < settings.R_minPopulation) {
                Instantiate(roadBranch, this.transform.position, this.transform.rotation, null);
            }
            this.transform.eulerAngles = x;

            //Kijk naar -~90 graden
            this.transform.Rotate(new Vector3(0, -rot, 0));
            if (populationMap.GetPixel(Mathf.RoundToInt(laserPos.transform.position.x), Mathf.RoundToInt(laserPos.transform.position.z)).grayscale < settings.R_minPopulation) {
                Instantiate(roadBranch, this.transform.position, this.transform.rotation, null);
            }
            this.transform.eulerAngles = x;
        }

        //Detroy object
        void DestroyHighwayGenerator() {
            Destroy(this.gameObject);
        }
    }
}
