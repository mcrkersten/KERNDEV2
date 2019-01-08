using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRoad : MonoBehaviour {

    public GameObject BranchPrfab;
    public GameObject mainRoadBranchPrefab;
    public Material Mat;
    public LineRenderer lr;

    protected Color roadColor;
    protected int minBranchDistance;
    protected int branchProb;
    protected int maxLenght;
    protected int curLenght = 0;
    protected Vector3 debugPos;
    protected GameObject laserPos;




    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual void InitSettings() {

    }

    protected void InitLineRenderer() {
        lr = this.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, this.transform.position);
        lr.SetPosition(1, laserPos.transform.position);
    }
}
