using UnityEngine;
using System.Collections;

public class MarkerController : MonoBehaviour {

	public ExperimentController experimentController;
	private GameObject marker; 
	private GameObject pointer;

	public bool isStarted; 

	private bool dirRight = true;
	private float speed;

	public float proprioceptiveDrift;

	private float pointerx;
	private float pointery;
	private float pointerz;

	public void Start () {
		marker = GameObject.Find ("Marker");
		pointer = GameObject.Find ("Pointer");

		// get the original position of the pointer
		pointerx = pointer.transform.localPosition.x;
		pointery = pointer.transform.localPosition.y;
		pointerz = pointer.transform.localPosition.z;	
	}

	public void Update(){
		if (!isStarted) {
			marker.SetActive (false);
		}
		if (isStarted) {
			marker.SetActive(true);
			speed = 0.05f;
			StartMarker();
		}
	}

	public void StartMarker(){
		Vector3 movement = new Vector3 (0, 0, 1);
		// marker moving from left to right in the x axis
		if (dirRight){
			pointer.transform.Translate (movement * speed * Time.deltaTime);
			if (pointer.transform.position.z >= 0.26f)
			{
				dirRight = false;
			}
		} 
		else {
			// change to the opposite direction along the axis. 
			pointer.transform.Translate (-movement * speed * Time.deltaTime);
			if (pointer.transform.position.z <= -0.40f) 
			{
				dirRight = true;
			}
		}	
		MeasureProprioceptiveDrift ();
	}

	// Method that will be called when proprioceptive drift needs to be measured
	public float MeasureProprioceptiveDrift(){
		if (Input.GetKeyDown ("space") && isStarted) {
			speed = 0.0f;
			proprioceptiveDrift += pointer.transform.localPosition.z;
			// notifies ExpController that the PD has been measured
			experimentController.HandleEvent (ExperimentEvents.ProprioceptiveDriftMeasured);
			// Restarts the pointer position to its original value (Start())
			pointer.transform.localPosition =  new Vector3(pointerx, pointery, pointerz);
		}
	return proprioceptiveDrift;
	}
}
