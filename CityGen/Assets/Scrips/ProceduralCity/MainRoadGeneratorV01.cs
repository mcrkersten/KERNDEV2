using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace V02 {
    public class MainRoadGeneratorV01 : MonoBehaviour {

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
        private int minLength;
        private int length;
        private int curLenght = 0;
        private int curStreetBranchDistance;

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
            settings.currentMainRoads++;
            length = Random.Range(minLength, maxLenght);
        }

        void LateStart() {
            InitSettings();
            InitLaserPosition();
            InitLineRenderer();
            debugPos = this.gameObject.transform.position;
        }

        //Get all settings from settingsObject
        void InitSettings() {
            settings = SettingsObject.Instance;
            angle = settings.MR_angle;
            laserDistance = settings.MR_laserDistance;
            populationMap = settings.populationMap;
            waterMap = settings.waterMap;
            canBranch = settings.MR_canBranch;
            minBranchDistance = settings.MR_minimalBranchDistance;
            branchProb = settings.MR_branchProbability;
            maxLenght = settings.MR_MaxRoadLength;
            roadColor = settings.MR_roadColor;
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
            laserPos = new GameObject("laserMainRoad");
            laserPos.transform.parent = this.transform;
            laserPos.transform.localPosition = new Vector3(laserDistance, 0, 0);
            laserPos.transform.rotation = this.transform.rotation;
        }

        //Create new highway
        public void InitBranch(Vector3 rot, Vector3 pos) {
            LateStart();
            this.transform.position = pos;

            List<int> x = new List<int>(); //Position
            List<int> z = new List<int>(); //Position
            List<float> y = new List<float>(); //Rotation

            this.transform.eulerAngles = new Vector3(rot.x, rot.y + settings.MR_branchAngle, rot.z);
            x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
            z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
            y.Add(rot.y + settings.MR_branchAngle);

            this.transform.eulerAngles = new Vector3(rot.x, rot.y - settings.MR_branchAngle, rot.z);
            x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
            z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
            y.Add(rot.y - settings.MR_branchAngle);

            Vector3 bestOnPopMap = PopulationConstraints(x, z, y);
            bool waterConstraints = WaterConstraints(Mathf.RoundToInt(bestOnPopMap.z), Mathf.RoundToInt(bestOnPopMap.x));
            this.transform.eulerAngles = new Vector3(rot.x, bestOnPopMap.y, rot.z);
        }

        private void Update() {
            if (Input.anyKey) {
                if (this.transform.position.x > 0 && this.transform.position.x < settings.populationMap.width && this.transform.position.z > 0 && this.transform.position.z < settings.populationMap.height && curLenght < maxLenght) {
                    if(settings.R_minimalBranchDistance < curStreetBranchDistance) {
                        NewStreet();
                        curStreetBranchDistance = 0;
                    }
                    else {
                        curStreetBranchDistance++;
                    }
                    BuildFreeway();
                    curLenght++;
                }
                else {
                    DestroyRoadGenerator();
                }
            }
        }

        //Start loop for highway creation
        void BuildFreeway() {
            if(curLenght < length) {
                GetBestPosition(); //<- Calls Constraints and BuildRoad();
                DrawNewRoad();
            }
            else {
                DestroyRoadGenerator();
            }
        }

        //Get the best position for a new roadsegment.
        //Calls global goals and local constraints;
        void GetBestPosition() {
            List<int> x = new List<int>(); //Position
            List<int> z = new List<int>(); //Position
            List<float> y = new List<float>(); //Rotation

            //Get all positions to check.
            angle += Random.Range(1,5);
            float rotationY = this.transform.eulerAngles.y - (angle / 2);
            for (int i = 0; i < angle; i++) {
                rotationY = rotationY + 1;
                this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, rotationY, this.transform.eulerAngles.z);
                x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
                z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
                y.Add(rotationY);
            }
            angle = settings.MR_angle;

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
            if (settings.R_minPopulation < heighest) {
                DestroyRoadGenerator();
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
            GameObject newLine = new GameObject("Main-Road-Section");

            //Line renderer
            newLine.transform.position = this.transform.position;
            LineRenderer nlr = newLine.AddComponent<LineRenderer>();
            nlr.material = Mat;
            Mat.color = roadColor;
            nlr.positionCount = 2;
            nlr.SetPosition(0, this.transform.position);
            nlr.SetPosition(1, debugPos);
            nlr.startWidth = 2.5f;
            nlr.endWidth = 2.5f;
            nlr.sortingOrder = 1;
            debugPos = this.transform.position;
            nlr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            newLine.transform.parent = settings.transform;
        }

        //Place new point if all tests are correct or disable this object
        void BuildRoad(bool noWater, Vector3 position) {
            if (noWater == true) {
                Vector2 p = RoadCrossing(position);

                //If p is not 0,0, that means a crossing has been found or created on this point (Kills RoadGenerator)
                if (p != new Vector2(0, 0)) {
                    this.transform.position = new Vector3(p.x, 0, p.y);
                    this.transform.eulerAngles = new Vector3(0, 0, 0);
                    DestroyRoadGenerator();
                    return;
                }
                //Set new occupied position in a Quad
                foreach (Quad quad in settings.quads) {
                    if (position.x < quad.quadPosition.x && position.z < quad.quadPosition.y) {
                        quad.occupied.Add(new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z)));
                        break; //stay in own Quad
                    }
                }

                this.transform.position = new Vector3(position.x, 0, position.z);
                this.transform.eulerAngles = new Vector3(0, position.y, 0);
                //Reset Debug LineRenderer
                lr.SetPosition(0, this.transform.position);
                lr.SetPosition(1, laserPos.transform.position);

                if(settings.currentMainRoads < settings.maxMainRoads) {
                    if (canBranch) {
                        branchDistance++;
                        int spawnNumber = Random.Range(0, 100 + 1);
                        if (branchDistance > minBranchDistance && spawnNumber < branchProb) {
                            GameObject branch = Instantiate(BranchPrfab, null);
                            branch.GetComponent<MainRoadGeneratorV01>().InitBranch(this.transform.eulerAngles, this.transform.position);
                            branchDistance = 0;
                        }
                    }
                }
            }
            else {
                DestroyRoadGenerator();
            }
        }

        //Check for roadcrossings and handle them
        Vector2 RoadCrossing(Vector3 position) {
            List<Vector2> possibleCrossings = new List<Vector2>();

            if (curLenght >= 1) {
                //Check what quad you are in.
                foreach (Quad quad in settings.quads) {
                    if (position.x < quad.quadPosition.x && position.z < quad.quadPosition.y) {

                        //If quad is found, loop through all occupied possitions
                        foreach (Vector2 x in quad.occupied) {

                            //if there is a occupied possition found within range of our current position of X and Z
                            if (position.x < x.x + (laserDistance / 1.5) && position.x > x.x - (laserDistance / 1.5)) {
                                if (position.z < x.y + (laserDistance / 1.5) && position.z > x.y - (laserDistance / 1.5)) {
                                    possibleCrossings.Add(x);
                                }
                            }
                        }
                        if (possibleCrossings.Count >= 1) {
                            return ClosestPoint(possibleCrossings, position);
                        }
                    }
                }
            }
            return new Vector2(0, 0);
        }

        public Vector2 ClosestPoint(List<Vector2> positions, Vector2 curPos) {
            List<float> dist = new List<float>();
            foreach (Vector2 singlePos in positions) {
                dist.Add(Vector2.Distance(singlePos, curPos));
            }
            int index = dist.IndexOf(dist.Min());
            print(index);

            //if it is a new road crossing (No roads have crossed this roadsection)
            //Remember this crossing(Debug)
            if (!settings.existingCrossing.Contains(positions[index])) {
                print("Crossing");
                settings.existingCrossing.Add(positions[index]);
                settings.existingCrossingYrot.Add(this.transform.eulerAngles.y);
            }
            return positions[index];
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

        void DestroyRoadGenerator() {
            settings.removeRoads.Add(this.gameObject);
        }
    }
}
