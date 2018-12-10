using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {
    bool freq64;
    AudioPeer AP;
    public Renderer RD;
    public int freqBand;
    public Color color;

    private void Start() {
        RD.material.SetColor("_TintColor", color);
        AP = AudioPeer.Instance;
        RD = this.GetComponent<Renderer>();
    }

    private void Update() {
        RD.material.SetColor("_EmissionColor", new Color(AP._audioBandBuffer[freqBand] *2, AP._audioBandBuffer[freqBand] *2, AP._audioBandBuffer[freqBand] *2));
        transform.localScale = new Vector3(AP._audioBandBuffer[freqBand]/10, AP._audioBandBuffer[freqBand]/10, AP._audioBandBuffer[freqBand]/10);
    }
}
