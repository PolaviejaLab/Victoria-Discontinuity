using UnityEngine;
using System.Collections;

public class GetInfo : MonoBehaviour
{

    public HandSwitcher handSwitcher;
    public ExperimentController experimentController;

    public string subjectCode;
    public int expNum;
    public string experimentName;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void getCode(string subjectName)
    {
        subjectCode = subjectName;
        Debug.Log("Subject Code " + subjectCode);
    }

    public void isMale(bool male)
    {
        handSwitcher.useMale = true;
        Debug.Log("Changed gender to male");
    }

    public void isFemale(bool female)
    {
        handSwitcher.useMale = false;
        Debug.Log("Changed gender to female");
    }
    
    public void startExperiment()
    {
        experimentController.ChangeState(ExperimentStates.Start);
        Debug.Log("Experiment has been started");
    }
}
