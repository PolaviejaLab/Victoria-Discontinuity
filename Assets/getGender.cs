using UnityEngine;
using System.Collections;

public class getGender : MonoBehaviour {

    public int male = 0;
    public int female = 0;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void isMale() {
        if (female == 0) {
            male = 1;
            Debug.Log("Gender is male");
        } else if (female == 1) {
            Debug.Log("Female already selected");
        }
    }

    public void isFemale() {
        if (male == 0) {
            female = 1;
            Debug.Log("Gender is female");
        } else if (male == 1) {
            Debug.Log("Male already selected");
        }
    }
}
