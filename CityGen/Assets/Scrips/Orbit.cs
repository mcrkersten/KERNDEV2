using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {
    public float speed = 10;
    public float mayorAxis = 1;
    public float minorAxis = 1;
    public float posVariabele;
    public GameObject child;
    public Planet planet;

    private void Awake() {
        posVariabele = Random.Range(0f, 350f);
    }

    void Update()
    {
        child.transform.localPosition = new Vector3(Mathf.Sin((Time.time + posVariabele) * speed) * mayorAxis, 0.0f, Mathf.Cos((Time.time + posVariabele) * speed) * minorAxis);
    }

    public void ChangeYrot(float amount) {
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, amount, this.transform.eulerAngles.z);
    }

    public void ChangeXrot(float amount) {
        this.transform.eulerAngles = new Vector3(amount, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
    }

    public void ChangeYpos(float amount) {
        this.transform.position = new Vector3(this.transform.position.x, amount, this.transform.position.z);
    }

}
