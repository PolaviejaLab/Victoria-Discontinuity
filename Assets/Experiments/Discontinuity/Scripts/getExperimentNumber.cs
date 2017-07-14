using UnityEngine;
using System.Collections;

public class getExperimentNumber : MonoBehaviour {

    public string experimentName;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void getNumber(int expNum)
    {
        Debug.Log("int passed: " + expNum);
        if (expNum == 1)
        {
            experimentName = "Exp2_Repetition";
        }

        Debug.Log("This program will load protocol for " + experimentName);
    }
}
