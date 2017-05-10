using UnityEngine;
using System.Collections;

public class getGender : MonoBehaviour {

    public int male_ = 0;
    public int female_ = 0;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void isMale() {
        if (female_ == 0) {
            male_ = 1;
            
            Debug.Log("Gender is male");
        } else if (female_ == 1) {
            Debug.Log("Female already selected");
        }
    }

    public void isFemale() {
        if (male_ == 0) {
            female_ = 1;
           
            Debug.Log("Gender is female");
        } else if (male_ == 1) {
            Debug.Log("Male already selected");
        }
    }
}
