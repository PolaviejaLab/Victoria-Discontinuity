using UnityEngine;
using System.Collections;

public class HandSwitcher : MonoBehaviour {
	
	public GameObject[] leftGraphicsModel;
	public GameObject[] rightGraphicsModel;

	public int selected;
	
	private int previous = -1;


	// Use this for initialization
	void Start () {
	
	}
	
	void Update() {
		if(selected < -1 || selected >= leftGraphicsModel.Length || selected >= rightGraphicsModel.Length)
			selected = 0;
		
		if(selected != previous)
			UpdateModels();
		previous = selected;
	}
	
	protected void UpdateModels () {
		for(int i = 0; i < leftGraphicsModel.Length; i++)
			leftGraphicsModel[i].SetActive(i == selected);
		for(int i = 0; i < leftGraphicsModel.Length; i++)
			rightGraphicsModel[i].SetActive(i == selected);
	}
}
