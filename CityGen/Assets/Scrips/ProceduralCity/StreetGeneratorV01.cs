using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace V02 {
    public class StreetGeneratorV01 : MonoBehaviour {
        private SettingsObject settings;

        public GameObject BranchPrfab;
        public Material Mat;
        private Color roadColor;
        private int branchDistance;
        private bool canBranch;
        private int minBranchDistance;
        private int branchProb;
        private int curLenght = 0;
        private int angle;
        private float minimalPopulation;
        public int id;
        private int generatedStreets;

        private GameObject laserPos;

        private Texture2D populationMap;
        private Texture2D waterMap;

        [Header("Debug visualization")]
        public LineRenderer lr;
        private Vector3 debugPos;

        //Settings
        private int minAngle;
        private int maxAngle;

        private float laserDistance;

        void Start() {
            InitSettings();
            InitLaserPosition();
            InitLineRenderer();
        }

        void InitSettings() {
            settings = SettingsObject.Instance;
            settings.newRoads.Add(this.gameObject);
            waterMap = settings.waterMap;
            angle = settings.R_angle;
            minAngle = settings.R_minAngle;
            maxAngle = settings.R_maxAngle;
            laserDistance = settings.R_laserDistance;
            populationMap = settings.populationMap;
            minBranchDistance = settings.R_minimalBranchDistance;
            branchProb = settings.R_branchProbability;
            roadColor = settings.R_roadColor;
            minimalPopulation = settings.R_minPopulation;
        }

        void InitLaserPosition() {
            laserPos = new GameObject("laserStreet");
            laserPos.transform.parent = this.transform;
            laserPos.transform.localPosition = new Vector3(laserDistance, 0, 0);
            laserPos.transform.rotation = this.transform.rotation;
        }

        void InitLineRenderer() {
            //Debug linerenderer
            lr = this.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, laserPos.transform.position);
            debugPos = this.transform.position;
        }

        public void BuildLoop () {
            if (this.transform.position.x > 0 && this.transform.position.x < settings.populationMap.width && this.transform.position.z > 0 && this.transform.position.z < settings.populationMap.height) {
                BuildStreet();
                DrawNewRoad();
                branchDistance++;
            }
            else{
                DestroyRoadGenerator();
                return;
            }
        }

        void BuildStreet() {
            Vector3 t = GetBestPosition();
            BuildRoad(WaterConstraints(Mathf.RoundToInt(t.x), Mathf.RoundToInt(t.z)), t);
            curLenght++;
        }

        Vector3 GetBestPosition() {
            List<int> x = new List<int>(); //Position
            List<int> z = new List<int>(); //Position
            List<float> y = new List<float>(); //Rotation

            float rotationY = this.transform.eulerAngles.y - (angle / 2);
            for (int i = 0; i < angle; i++) {
                rotationY = rotationY + 1;
                this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, rotationY, this.transform.eulerAngles.z);
                x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
                z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
                y.Add(rotationY);
            }
            Vector3 bestOnPopMap = PopulationConstraints(x, z, y);
            return bestOnPopMap;
        }

        Vector3 PopulationConstraints(List<int> x, List<int> z, List<float> y) {
            float heighest = 1;
            Vector3 heighestPopPos = new Vector3();
            for (int i = 0; i < x.Count; i++) {
                float heighestPopulation = populationMap.GetPixel(x[i], z[i]).grayscale;
                if (heighestPopulation < heighest) {
                    heighestPopPos = new Vector3(x[i], y[i], z[i]);
                    heighest = heighestPopulation;
                }
            }
            if(minimalPopulation < heighest) {
                DestroyRoadGenerator();
            }
            return heighestPopPos;
        }

        bool WaterConstraints(int x, int z) {
            float waterAmount = waterMap.GetPixel(x, z).grayscale;
            if (waterAmount < .5) {
                return false; //To much water
            }
            return true;
        }

        //Place new point if all tests are correct or disable this object
        void BuildRoad(bool noWater, Vector3 position) {
            if (noWater == true && generatedStreets < settings.maxGeneratedStreets) {
                generatedStreets++;
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

                if (branchDistance > minBranchDistance) {
                    int spawnNumber = Random.Range(0, 100 + 1);
                    if (branchDistance > minBranchDistance && spawnNumber < branchProb) {
                        NewStreet();
                        branchDistance = 0;
                    }
                }
            }
            else {
                DestroyRoadGenerator();
                return;
            }
        }
        //Check for roadcrossings and handle them
        Vector2 RoadCrossing(Vector3 position) {
            List<Vector2> possibleCrossings = new List<Vector2>();

            if (curLenght >= 1) {
                //Check what quad you are in.
                foreach(Quad quad in settings.quads) {
                    if(position.x < quad.quadPosition.x && position.z < quad.quadPosition.y) {

                        //If quad is found, loop through all occupied possitions
                        foreach (Vector2 x in quad.occupied) {

                            //if there is a occupied possition found within range of our current position of X and Z
                            if (position.x < x.x + (laserDistance / 1.25) && position.x > x.x - (laserDistance / 1.25)) {
                                if (position.z < x.y + (laserDistance / 1.25) && position.z > x.y - (laserDistance / 1.25)) {
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
            foreach(Vector2 singlePos in positions) {
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

        public void NewStreet() {
            Vector3 x = this.transform.eulerAngles;

            //Kijk naar +~90 graden
            float rot = Random.Range(settings.R_minAngle, settings.R_maxAngle);
            this.transform.Rotate(new Vector3(0, rot, 0));
            if (populationMap.GetPixel(Mathf.RoundToInt(laserPos.transform.position.x), Mathf.RoundToInt(laserPos.transform.position.z)).grayscale < settings.R_minPopulation) {
                Vector2 p = RoadCrossing(laserPos.transform.position);
                if (p != new Vector2(0, 0)) {
                    return;
                }
                else {
                    Instantiate(BranchPrfab, this.transform.position, this.transform.rotation, null);
                }
            }
            this.transform.eulerAngles = x;

            //Kijk naar -~90 graden
            this.transform.Rotate(new Vector3(0, -rot, 0));
            if (populationMap.GetPixel(Mathf.RoundToInt(laserPos.transform.position.x), Mathf.RoundToInt(laserPos.transform.position.z)).grayscale < settings.R_minPopulation) {
                Vector2 p = RoadCrossing(laserPos.transform.position);
                if (p != new Vector2(0, 0)) {
                    return;
                }
                else {
                    Instantiate(BranchPrfab, this.transform.position, this.transform.rotation, null);
                }            
            }
            this.transform.eulerAngles = x;
        }

        void DestroyRoadGenerator() {
            settings.removeRoads.Add(this.gameObject);
        }

        void DrawNewRoad() {
            GameObject newLine = new GameObject("Street-Section");

            //Line renderer
            newLine.transform.position = this.transform.position;
            LineRenderer nlr = newLine.AddComponent<LineRenderer>();
            nlr.material = Mat;
            Mat.color = roadColor;
            nlr.endColor = roadColor;
            nlr.positionCount = 2;
            nlr.SetPosition(0, this.transform.position);
            nlr.SetPosition(1, debugPos);
            nlr.startWidth = 1;
            nlr.endWidth = 1;
            nlr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            debugPos = this.transform.position;

            newLine.transform.parent = settings.transform;
        }
    }
}
