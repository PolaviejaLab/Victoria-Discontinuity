using UnityEngine;
using System.Collections;

public class SubjectCode : MonoBehaviour {

    public string subjectName;

	// Use this for initialization
	void Start () {
	
	}

    public void GetName(string subjectCode) {
        Debug.Log(subjectCode);
        subjectName = subjectCode;
    }

	// Update is called once per frame
	void Update () {
	
	}
}
