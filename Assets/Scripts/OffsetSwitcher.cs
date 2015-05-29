using UnityEngine;
using System.Collections;

public class OffsetSwitcher : MonoBehaviour {

	public float offset = 0;

	private float previous = -1; 

	private GameObject controller;

	void Start () {

	}
	
	void Update () {
//		if (selectedOffset < -1) {
//			selectedOffset = 0;
//		}
		if (offset != previous) {
			UpdateOffset ();
			previous = offset;
		}
	}

	protected void UpdateOffset (){
		// method that will make the offset change, get the handcontroller to shift with the offset
		// use a find method 
		controller = GameObject.Find ("HandController");
		controller.transform.localPosition = new Vector3(-offset, 0, 0);

		Debug.Log("Changing offset: " + controller.transform.localPosition);
	}
}
