using UnityEngine;
using System.Collections;

public class SimpleCollision : MonoBehaviour {

	public WaveController waveController;
	public WaveEvents triggerEvent;
	public GameObject[] objects;

	public bool CompareByName = false;	

	void OnTriggerStay(Collider col)
	{
        Debug.Log(col.name);
        if (col.name == "HandContainer"){
            Debug.Log("Hello");
			if(objects.Length == 0) {
                waveController.HandleEvent(triggerEvent);
			} else {
				for(int i = 0; i < objects.Length; i++) {
					if(CompareByName) {
						if(col.gameObject.name == objects[i].name)
                            waveController.HandleEvent(triggerEvent);
					} else {
						if(col.gameObject == objects[i])
                            waveController.HandleEvent(triggerEvent);
					}
				}
			}
		}
	}	
}
