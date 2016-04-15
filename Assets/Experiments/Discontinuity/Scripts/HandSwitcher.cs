using UnityEngine;
using System.Collections;
using Leap.Unity;

public class HandSwitcher : MonoBehaviour {
	
	public GameObject[] leftGraphicsModelMale;
	public GameObject[] rightGraphicsModelMale;

	public GameObject[] leftGraphicsModelFemale;
	public GameObject[] rightGraphicsModelFemale;

	public int selected;	
	private int previous = -1;

	public bool useMale;
	private bool oldUseMale;

	public bool showLeftHand = true;
    public bool showRightHand = true;

    public float noiseLevelLeft;
    public float noiseLevelRight;

    private bool oldRight, oldLeft;
    private float oldNoiseRight, oldNoiseLeft;

	// Use this for initialization
	void Start () {
		UpdateModels();
        UpdateNoiseLevels();
	}
	
	void Update() {
		if(selected < -1 || selected >= leftGraphicsModelMale.Length || selected >= rightGraphicsModelMale.Length)
			selected = 0;		
						
        if (selected != previous || useMale != oldUseMale || 
            showLeftHand != oldLeft || showRightHand != oldRight || 
            noiseLevelLeft != oldNoiseLeft || noiseLevelRight != oldNoiseRight) {
            UpdateModels();
            UpdateNoiseLevels();
        }
		

		previous = selected;
		oldUseMale = useMale;
		oldLeft = showLeftHand;
		oldRight = showRightHand;
        oldNoiseRight = noiseLevelRight;
        oldNoiseLeft = noiseLevelLeft;

	}
	
	protected void UpdateModels () {
		for(int i = 0; i < leftGraphicsModelMale.Length; i++)
			leftGraphicsModelMale[i].SetActive(i == selected && useMale && showLeftHand);			
		for(int i = 0; i < rightGraphicsModelMale.Length; i++)		
			rightGraphicsModelMale[i].SetActive(i == selected && useMale && showRightHand);
		
		for(int i = 0; i < leftGraphicsModelFemale.Length; i++)
			leftGraphicsModelFemale[i].SetActive(i == selected && !useMale && showLeftHand);
		for(int i = 0; i < rightGraphicsModelFemale.Length; i++)
			rightGraphicsModelFemale[i].SetActive(i == selected && !useMale && showRightHand);
	}

    protected void UpdateNoiseLevels() {
        for(int i = 0; i < leftGraphicsModelMale.Length; i++) {
            RiggedHand hand = leftGraphicsModelMale[i].GetComponent<RiggedHand>();
//            if (noiseLevelLeft == 0.0f) {
//                hand.enableNoise = false;
//            } else {
//                hand.enableNoise = true;
//            }
             hand.enableNoise = noiseLevelLeft > 0.5f;
        }

        for(int i = 0; i <  rightGraphicsModelMale.Length; i++) {
            RiggedHand hand = rightGraphicsModelMale[i].GetComponent<RiggedHand>();
//            if (noiseLevelRight == 0.0f) {
//                hand.enableNoise = false;
//            } else {
//                hand.enableNoise = true;
//            }
            hand.enableNoise = noiseLevelLeft > 0.5f;

        }

        for(int i = 0; i < leftGraphicsModelFemale.Length; i++) {
            RiggedHand hand = leftGraphicsModelFemale[i].GetComponent<RiggedHand>();
            hand.enableNoise = noiseLevelLeft > 0.5f;
        }
        
        for(int i = 0; i <  rightGraphicsModelFemale.Length; i++) {
            RiggedHand hand = rightGraphicsModelFemale[i].GetComponent<RiggedHand>();
            hand.enableNoise = noiseLevelLeft > 0.5f;
        }

    }
}
