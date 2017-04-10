using UnityEngine;
using System.Collections;

public class StartExperiment : MonoBehaviour {

    public ExperimentController experimentController;

	// Use this for initialization
	void Start () {
	
	}

    public void StartEx() {
        experimentController.ChangeState(ExperimentStates.Start);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
