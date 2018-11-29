using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour {

    [Header("Ster hoeveelheid")]
    [Range(0f, 10000)]
    public int stars = 4000;

    [Header("Skew Opties")]
    [Tooltip("De rotatie in de in de Xas per Ster-orbit")]
    public float skewAmount = 0;
    [Tooltip("Noise in de rotatie")]
    public float skewNoise = 0;

    [Header("Shape Opties")]
    [Tooltip("Galaxy heeft een bulge center")]
    public bool hasBulge;
    [Tooltip("De rotatie in de Yas per Ster-Orbit")]
    public float orbitRotationStepSize = 0;
    public float orbitYNoise = 0f;

    [Header("Stap-grote over de minorAxis")]
    public float minorAxisStep = 0;
    [Header("Stap-grote over de mayorAxis")]
    public float mayorAxisStep = 0;

    [Header("Ster Opties")]
    [Tooltip("Snelheid afnamen per orbit")]
    public float orbitSpeedDecrease = 0;
    [Tooltip("De ster prefab")]
    public GameObject planetPrefab;

    private float mayorAxis = 0f;
    private float minorAxis = 0f;
    private float orbitRotation = 0;
    private float orbitSkew = 0;

    private float octaveCal;
    private int octave = 8;
    private int currentOct = 0;

    [Header("Ster Kleuren")]
    public Color[] color = new Color[0];
    
    private int tempOct = 0;

    private float CurrenBulgeAngle = 180;


    // Use this for initialization
    void Start () {

        octaveCal = stars / 8;
        for (int i = 0; i < stars; i++) {
            Generate();
            minorAxis = minorAxis + minorAxisStep;
            mayorAxis = mayorAxis + mayorAxisStep;
        }
	}

    private void Generate() {
        GameObject planet = Instantiate(planetPrefab, this.transform);
        if(planet.GetComponent<Orbit>() != null) {
            Orbit localOrbit = planet.GetComponent<Orbit>();

            //Basic Settings
            localOrbit.planet.color = GetColor();
            localOrbit.planet.freqBand = tempOct;
            localOrbit.speed = -orbitSpeedDecrease;

            //Orbit sizes
            localOrbit.mayorAxis = mayorAxis;
            localOrbit.minorAxis = minorAxis;

            //Orbit Yrotation (density wave theory)
            localOrbit.ChangeYrot(orbitRotation);
            orbitRotation += orbitRotationStepSize;

            //Disk Skew rotation
            orbitSkew += skewAmount;
            float randomNoise = Random.Range(-skewNoise, skewNoise);
            localOrbit.ChangeXrot(orbitSkew + randomNoise);
            

            //Yheight Noise (disk thickness)
            localOrbit.ChangeYpos(Random.Range(orbitYNoise, -orbitYNoise));

            if (hasBulge && CurrenBulgeAngle > 0) {
                float toSend = Random.Range(-CurrenBulgeAngle, CurrenBulgeAngle);
                localOrbit.ChangeXrot(toSend);

                if(CurrenBulgeAngle > 0) {
                    CurrenBulgeAngle -= .07f;
                }
            }
        }
    }

    private Color GetColor() {
        if (octaveCal < octave) {
            octave = 0;
            currentOct++;
        }
        tempOct = currentOct;
        octave++;       
        return color[currentOct];
    }
}
