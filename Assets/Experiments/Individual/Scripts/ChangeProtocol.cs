using UnityEngine;
using System.Collections;

public class ChangeProtocol : MonoBehaviour {

    public int expNum;
    public string expName;

    public ExperimentController experimentController;
   

    // Use this for initialization
	void Start () {
	
	}

    public void getExperimentName(int expNum)
    {
        Debug.Log("int passed: " + expNum);
        if (expNum == 1)
            expName = "Exp2_Experiment1";
        else if (expNum == 2)
            expName = "Exp2_Experiment";

        Debug.Log("This program will load protocol for " + expName);

        experimentController.experimentName = expName;
    }

    // Update is called once per frame
    void Update () {
	
	}


}
