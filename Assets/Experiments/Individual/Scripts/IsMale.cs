using UnityEngine;
using System.Collections;

public class IsMale : MonoBehaviour {

    public HandSwitcher handSwitcher;

	// Use this for initialization
	void Start () {
    }


    public void isMale()
    {
        handSwitcher.useMale = true;
    }


    // Update is called once per frame
    void Update () {
	
	}
}
