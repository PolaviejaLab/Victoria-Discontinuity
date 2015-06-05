using UnityEngine;
using System.Collections;

public class Debbuging : MonoBehaviour {

	public TrialController trialController;

	void Start () {
	}
	
	void Update () {
		if(Input.GetKeyDown (KeyCode.A))
			trialController.HandleEvent(TrialEvents.Wave_0);
		if (Input.GetKeyDown (KeyCode.B))
			trialController.HandleEvent(TrialEvents.Wave_1);
		if (Input.GetKeyDown (KeyCode.Space))
			trialController.HandleEvent(TrialEvents.Wave_Initial);
	}
}
