using UnityEngine;
using System.Collections;

public class IsFemale : MonoBehaviour {

    public HandSwitcher handSwitcher;

	// Use this for initialization
	void Start () {
	
	}

    public void isFemale() {
        handSwitcher.useMale = false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
