using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace V02 {

    public class BaseRoad : MonoBehaviour {

        public GameObject BranchPrfab;
        public Material Mat;
        public LineRenderer lr;

        protected SettingsObject settings;
        protected Color roadColor;
        protected int minBranchDistance;
        protected int branchProb;
        protected int maxLenght;
        protected int curLenght = 0;
        protected Vector3 debugPos;
        protected GameObject laserPos;
        protected float laserDistance;
        protected Texture2D populationMap;
        protected Texture2D waterMap;
        protected int angle;

        protected int noise;
        protected float roadCrossingSnapDistance;

        // Use this for initialization
        protected virtual void Start() {
            InitSettings();
            InitLaserPosition();
            InitLineRenderer();
        }

        protected void LateStart() {
            InitSettings();
            InitLaserPosition();
            InitLineRenderer();
            debugPos = this.gameObject.transform.position;
        }

        public virtual void InitSettings() {
            settings = SettingsObject.Instance;
            populationMap = settings.populationMap;
            waterMap = settings.waterMap;
        }

        protected void InitLineRenderer() {
            lr = this.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, laserPos.transform.position);
        }

        protected void InitLaserPosition() {
            laserPos = new GameObject("laser");
            laserPos.transform.parent = this.transform;
            laserPos.transform.localPosition = new Vector3(laserDistance, 0, 0);
            laserPos.transform.rotation = this.transform.rotation;
        }

        protected virtual void Update() { }

        protected virtual void GetBestPosition() {
            List<int> x = new List<int>(); //Position
            List<int> z = new List<int>(); //Position
            List<float> y = new List<float>(); //Rotation

            //Get all positions to check.
            int tempAngle = (angle + Random.Range(1, noise));
            float rotationY = this.transform.eulerAngles.y - (angle / 2);
            for (int i = 0; i < angle; i++) {
                rotationY = rotationY + 1;
                this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, rotationY, this.transform.eulerAngles.z);
                x.Add(Mathf.RoundToInt(laserPos.transform.position.x));
                z.Add(Mathf.RoundToInt(laserPos.transform.position.z));
                y.Add(rotationY);
            }

            Vector3 bestOnPopMap = PopulationConstraints(x, z, y);

            bool waterConstraints = WaterConstraints(Mathf.RoundToInt(bestOnPopMap.x), Mathf.RoundToInt(bestOnPopMap.z));
            BuildRoad(waterConstraints, bestOnPopMap);
        }

        protected virtual Vector3 PopulationConstraints(List<int> x, List<int> z, List<float> y) {
            float heighest = 1;
            Vector3 heighestPopPos = new Vector3();
            for (int i = 0; i < x.Count; i++) {
                float heighestPopulation = populationMap.GetPixel(x[i], z[i]).grayscale;
                if (heighestPopulation < heighest) {
                    heighestPopPos = new Vector3(x[i], y[i], z[i]);
                    heighest = heighestPopulation;
                }
            }
            return heighestPopPos;
        }

        protected bool WaterConstraints(int x, int z) {
            float waterAmount = waterMap.GetPixel(x, z).grayscale;
            if (waterAmount < 1) {
                return false; //To much water
            }
            return true;
        }

        protected virtual void DrawNewRoad(string name, float size, int sortOrder) {
            GameObject newLine = new GameObject(name);

            //Line renderer
            newLine.transform.position = this.transform.position;
            LineRenderer nlr = newLine.AddComponent<LineRenderer>();
            nlr.material = Mat;
            Mat.color = roadColor;
            nlr.positionCount = 2;
            nlr.SetPosition(0, this.transform.position);
            nlr.SetPosition(1, debugPos);
            nlr.startWidth = size;
            nlr.endWidth = size;
            nlr.sortingOrder = sortOrder;
            debugPos = this.transform.position;
            nlr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            newLine.transform.parent = settings.transform;
        }

        protected virtual Vector2 RoadCrossing(Vector3 position) {
            List<Vector2> possibleCrossings = new List<Vector2>();

            if (curLenght >= 1) {
                //Check what quad you are in.
                foreach (Quad quad in settings.quads) {
                    if (position.x < quad.quadPosition.x && position.z < quad.quadPosition.y) {

                        //If quad is found, loop through all occupied possitions
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

        protected Vector2 ClosestPoint(List<Vector2> positions, Vector2 curPos) {
            List<float> dist = new List<float>();
            foreach (Vector2 singlePos in positions) {
                dist.Add(Vector2.Distance(singlePos, curPos));
            }
            int index = dist.IndexOf(dist.Min());

            //if it is a new road crossing (No roads have crossed this roadsection)
            //Remember this crossing(Debug)
            if (!settings.existingCrossing.Contains(positions[index])) {
                settings.existingCrossing.Add(positions[index]);
                settings.existingCrossingYrot.Add(this.transform.eulerAngles.y);
            }
            return positions[index];
        }

        protected virtual void BuildRoad(bool noWater, Vector3 position) { }

        protected virtual void DestroyGenerator() {
            Destroy(this.gameObject);
        }
    }
}
