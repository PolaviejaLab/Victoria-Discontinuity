using UnityEngine;
using System.Collections;

public class SimpleCollision : MonoBehaviour {

	public TrialController controller;
	public TrialEvents triggerEvent;
	public GameObject[] objects;

	public bool CompareByName = false;	

	void OnTriggerStay(Collider col)
	{		
		if(objects.Length == 0) {
			controller.HandleEvent(triggerEvent);
		} else {
			for(int i = 0; i < objects.Length; i++) {
				if(CompareByName) {
					if(col.gameObject.name == objects[i].name)
						controller.HandleEvent(triggerEvent);
				} else {
					if(col.gameObject == objects[i])
						controller.HandleEvent(triggerEvent);
				}
			}
		}
	}	
}
