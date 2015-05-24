using UnityEngine;
using System.Collections;

public class TableLights : MonoBehaviour {



//	GameObject[] lights;
	private GameObject light1;
	private GameObject light2;
	private GameObject light3; 

	public bool isOn;

	// Use this for initialization
	void Start () {
		light1 = GameObject.Find ("TableLights1");
		light2 = GameObject.Find ("TableLights2");
		light3 = GameObject.Find ("TableLights3");

		isOn = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isOn) {
			light1.SetActive (false);
			light2.SetActive (false);
			light3.SetActive (false);
		} else if (isOn) {
			light1.SetActive (true);
			light2.SetActive (true);
			light3.SetActive (true);
		}
	}
}
