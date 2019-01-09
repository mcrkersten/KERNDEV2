using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace V02 {
    public class StreetGeneratorV01 : BaseRoad {
        private int branchDistance;
        private float minimalPopulation;
        private int generatedStreets;
        private int minAngle;
        private int maxAngle;
        private List<LineRenderer> madeRoads = new List<LineRenderer>();

        protected override void Start() {
            InitSettings();
            InitLaserPosition();
            InitLineRenderer();
            debugPos = this.gameObject.transform.position;
        }

        override public void InitSettings() {
            settings = SettingsObject.Instance;
            settings.newRoads.Add(this.gameObject);
            noise = settings.R_noise;
            roadCrossingSnapDistance = settings.R_roadCrossingSnapDistance;
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

        public void BuildLoop () {
            if (this.transform.position.x > 0 && this.transform.position.x < settings.populationMap.width && this.transform.position.z > 0 && this.transform.position.z < settings.populationMap.height) {
                BuildStreet();
                DrawNewRoad("Street",1,-1);
                branchDistance++;
            }
            else{
                DestroyGenerator();
                return;
            }
        }

        void BuildStreet() {
            GetBestPosition();
            curLenght++;
        }

        protected override Vector3 PopulationConstraints(List<int> x, List<int> z, List<float> y) {
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
                DestroyGenerator();
            }
            return heighestPopPos;
        }

        //Place new point if all tests are correct or disable this object
        protected override void BuildRoad(bool noWater, Vector3 position) {
            if (noWater == true && generatedStreets < settings.maxGeneratedStreets) {
                generatedStreets++;
                Vector2 p = RoadCrossing(position);

                //If p is not 0,0, that means a crossing has been found or created on this point (Kills RoadGenerator)
                if (p != new Vector2(0, 0)) {
                    this.transform.position = new Vector3(p.x, 0, p.y);
                    this.transform.eulerAngles = new Vector3(0, 0, 0);
                    DestroyGenerator();
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
                DestroyGenerator();
                return;
            }
        }

        protected override Vector2 RoadCrossing(Vector3 position) {
            List<Vector2> possibleCrossings = new List<Vector2>();
            if (curLenght >= 1) {
                //Check what quad you are in.
                foreach (Quad quad in settings.quads) {
                    if (position.x < quad.quadPosition.x && position.z < quad.quadPosition.y) {

                        //If quad is found, loop through all occupied possitions
                        foreach (Vector2 x in quad.occupiedHighway) {
                            //if there is a occupied possition found within range of our current position of X and Z
                            if (position.x < x.x + (settings.highwayClearance) && position.x > x.x - (settings.highwayClearance)) {
                                if (position.z < x.y + (settings.highwayClearance) && position.z > x.y - (settings.highwayClearance)) {
                                    DestroyGenerator();
                                    this.gameObject.SetActive(false);
                                }
                            }
                        }
                        foreach (Vector2 x in quad.occupied) {
                            //if there is a occupied possition found within range of our current position of X and Z
                            if (position.x < x.x + (laserDistance / roadCrossingSnapDistance) && position.x > x.x - (laserDistance / roadCrossingSnapDistance)) {
                                if (position.z < x.y + (laserDistance / roadCrossingSnapDistance) && position.z > x.y - (laserDistance / roadCrossingSnapDistance)) {
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

        protected override void DestroyGenerator() {
            settings.removeRoads.Add(this.gameObject);
        }
    }
}
