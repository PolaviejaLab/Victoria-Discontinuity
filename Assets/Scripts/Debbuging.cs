using UnityEngine;
using System.Collections;

public class Debbuging : MonoBehaviour {

	public TrialController trialController;

	void Start () {
	}
	
	void Update () {
		if(Input.GetKeyDown (KeyCode.A)) {
			trialController.HandleEvent(TrialEvents.Wave_1);
		}
		if (Input.GetKeyDown (KeyCode.B)) {
			trialController.HandleEvent(TrialEvents.Wave_2);
		}		
		if (Input.GetKeyDown (KeyCode.C)) {
			trialController.HandleEvent(TrialEvents.Wave_3);
		}
	}
}
