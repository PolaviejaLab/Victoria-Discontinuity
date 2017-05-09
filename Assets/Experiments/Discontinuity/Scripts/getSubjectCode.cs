using UnityEngine;
using System.Collections;

public class getSubjectCode : MonoBehaviour {

    public string subjectCode;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void getCode(string subjectName) {
        subjectCode = subjectName;
        Debug.Log("Subject Code " + subjectCode);
    }

}
