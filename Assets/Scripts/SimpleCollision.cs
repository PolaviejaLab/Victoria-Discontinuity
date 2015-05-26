﻿using UnityEngine;
using System.Collections;

public class SimpleCollision : MonoBehaviour {

	public TrialController trialController;
	public TrialEvents triggerEvent;
	public GameObject[] objects;

	public bool CompareByName = false;	

	void OnTriggerStay(Collider col)
	{		
		if(objects.Length == 0) {
			Debug.Log("Collission with " + col.name);
			trialController.HandleEvent(triggerEvent);
		} else {
			for(int i = 0; i < objects.Length; i++) {
				if(CompareByName) {
					if(col.gameObject.name == objects[i].name)
						trialController.HandleEvent(triggerEvent);
				} else {
					if(col.gameObject == objects[i])
						trialController.HandleEvent(triggerEvent);
				}
			}
		}
	}	
}
