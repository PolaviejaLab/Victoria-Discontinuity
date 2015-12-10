using UnityEngine;
using System.Collections;

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

	private bool oldRight, oldLeft;

	// Use this for initialization
	void Start () {
		UpdateModels();
	}
	
	void Update() {
		if(selected < -1 || selected >= leftGraphicsModelMale.Length || selected >= rightGraphicsModelMale.Length)
			selected = 0;		
						
		if(selected != previous || useMale != oldUseMale || showLeftHand != oldLeft || showRightHand != oldRight)
			UpdateModels();

		previous = selected;
		oldUseMale = useMale;
		oldLeft = showLeftHand;
		oldRight = showRightHand;
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
}
