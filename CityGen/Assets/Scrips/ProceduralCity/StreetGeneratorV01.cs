using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace V02 {
    public class StreetGeneratorV01 : BaseRoad {
        public Material buildingMaterial;
        private int branchDistance;
        private float minimalPopulation;
        private int generatedStreets;
        private int minAngle;
        private int maxAngle;
        private float heighest = 1;
        private List<LineRenderer> madeRoads = new List<LineRenderer>();


        private float maxBuildingSize;
        private float minBuildingSize; 

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

            maxBuildingSize = laserDistance;
            minBuildingSize = laserDistance / 2;
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
            heighest = 1;
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
            bool corner = false;
            Vector3 lastPosition = this.transform.position;

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
                        corner = true;
                    }
                }

                if(branchDistance == 0) {
                    BuildBuildings(lastPosition, position, true, corner);
                }
                else {
                    BuildBuildings(lastPosition, position, false, corner);
                }
            }
            else {
                DestroyGenerator();
                return;
            }
        }

        void BuildBuildings(Vector3 start, Vector3 end, bool newGen, bool corner) {
            GameObject building = new GameObject("building");
            building.transform.position = new Vector3(start.x, 0, start.z);
            building.transform.eulerAngles = new Vector3(0, end.y, 0);
            building.transform.parent = settings.transform;

            GenerateBuildingMesh(building, start, end, true, newGen, corner);
            GenerateBuildingMesh(building, start, end, false, newGen, corner);

            SingleMesh(building);
        }

        void GenerateBuildingMesh(GameObject streetBuilding, Vector3 start, Vector3 end, bool leftStreet, bool newGen, bool corner) {
            float offset = 0;
            float roadOffset = 1;
            float cornerOrEndDecrease = 1;
            float streetLenght = (Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z)));

            if (corner && newGen) {              
                cornerOrEndDecrease = (streetLenght / 8);
            }else if (corner) {
                cornerOrEndDecrease = (streetLenght / 4);
            }

            int divider = Random.Range(1, 3);
            float sum;

            List<float> divisions = new List<float>();
            for (int i = 0; i < divider; i++) {
                sum = Random.Range((streetLenght / divider), streetLenght);
                streetLenght -= sum;
                divisions.Add(sum);
            }


            //Top
            foreach (float d in divisions) {
                float size = d;
                float height = Random.Range(5 + heighest, 20 + heighest) * (heighest + 1);

                Vector3 p0 = new Vector3(0, 0, size);
                Vector3 p1 = new Vector3(size, 0, size);

                Vector3 p2 = new Vector3(size, 0, 0);
                Vector3 p3 = new Vector3(0, 0, 0);

                Vector3 p4 = new Vector3(0, height, size);

                Vector3 p5 = new Vector3(size, height, size);
                Vector3 p6 = new Vector3(size, height, 0);
                Vector3 p7 = new Vector3(0, height, 0);


                Vector3[] vertices = new Vector3[]
                {
	                // Bottom
	                p0, p1, p2, p3,
 
	                // Left
	                p7, p4, p0, p3,
 
	                // Front
	                p4, p5, p1, p0,
 
	                // Back
	                p6, p7, p3, p2,
 
	                // Right
	                p5, p6, p2, p1,
 
	                // Top
	                p7, p6, p5, p4
                };

                Vector3 up = Vector3.up;
                Vector3 down = Vector3.down;
                Vector3 front = Vector3.forward;
                Vector3 back = Vector3.back;
                Vector3 left = Vector3.left;
                Vector3 right = Vector3.right;

                Vector3[] normales = new Vector3[]
                {
	                // Bottom
	                down, down, down, down,
 
	                // Left
	                left, left, left, left,
 
	                // Front
	                front, front, front, front,
 
	                // Back
	                back, back, back, back,
 
	                // Right
	                right, right, right, right,
 
	                // Top
	                up, up, up, up
                };

                Vector2 _00 = new Vector2(0f, 0f);
                Vector2 _10 = new Vector2(1f, 0f);
                Vector2 _01 = new Vector2(0f, 1f);
                Vector2 _11 = new Vector2(1f, 1f);

                Vector2[] uvs = new Vector2[]
                {
	                // Bottom
	                _11, _01, _00, _10,
 
	                // Left
	                _11, _01, _00, _10,
 
	                // Front
	                _11, _01, _00, _10,
 
	                // Back
	                _11, _01, _00, _10,
 
	                // Right
	                _11, _01, _00, _10,
 
	                // Top
	                _11, _01, _00, _10,
                };

                int[] triangles = new int[]
                {
	                // Bottom
	                3, 1, 0,
                    3, 2, 1,			
 
	                // Left
	                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                    3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
	                // Front
	                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                    3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
	                // Back
	                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                    3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
	                // Right
	                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                    3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
	                // Top
	                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                    3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
                };

                Mesh tm = new Mesh {
                    vertices = vertices,
                    normals = normales,
                    triangles = triangles,
                    uv = uvs

                }; ;
                GameObject temp = new GameObject("temp");
                temp.transform.parent = streetBuilding.transform;

                temp.transform.rotation = streetBuilding.transform.rotation;
                if (leftStreet) {
                    if(newGen && corner){
                        temp.transform.localPosition = new Vector3(offset + (cornerOrEndDecrease/2), 0, roadOffset);
                    }
                    else if (newGen) {
                        temp.transform.localPosition = new Vector3(offset + cornerOrEndDecrease, 0, roadOffset);
                    }
                    else{
                        temp.transform.localPosition = new Vector3(offset, 0, roadOffset);
                    }                  
                }
                else {
                    if (newGen && corner) {
                        temp.transform.localPosition = new Vector3(offset + (cornerOrEndDecrease), 0, (-roadOffset) - size);
                    }
                    else if (newGen) {
                        temp.transform.localPosition = new Vector3(offset + cornerOrEndDecrease, 0, (-roadOffset) - size);
                    }
                    else {
                        temp.transform.localPosition = new Vector3(offset, 0, (-roadOffset) - size);
                    }
                }      
                
                offset += size;
                MeshFilter m = temp.AddComponent<MeshFilter>();
                m.mesh = tm;
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

        void SingleMesh(GameObject building) {
            MeshFilter[] meshFilters = building.GetComponentsInChildren<MeshFilter>();

            building.AddComponent<MeshFilter>();
            building.AddComponent<MeshRenderer>();
            building.GetComponent<MeshRenderer>().material = buildingMaterial;

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            Matrix4x4 pTransform = building.transform.worldToLocalMatrix;
            int i = 0;
            while (i < meshFilters.Length) {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = pTransform * meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);
                i++;
            }
            building.GetComponent<MeshFilter>().mesh = new Mesh();
            building.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);

            for (int x = meshFilters.Length - 1; x >= 0; x--) {
                Destroy(meshFilters[x].gameObject);
            }

            building.gameObject.SetActive(true);
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
                    branchDistance = 0;
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
                    branchDistance = 0;
                }            
            }
            this.transform.eulerAngles = x;
        }

        protected override void DestroyGenerator() {
            settings.removeRoads.Add(this.gameObject);
        }
    }
}
