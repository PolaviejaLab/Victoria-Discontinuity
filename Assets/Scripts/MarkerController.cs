using UnityEngine;
using System.Collections;

public class MarkerController : MonoBehaviour {

	public ExperimentController experimentController;
	public GameObject marker; 

	public bool isStarted; 

	private bool dirRight = true;
	private float speed = 0.3f;

	public float proprioceptiveDrift;


	void Start ()
	{
		marker = GameObject.Find ("Marker");
		marker.SetActive (false);
	}
	
	void Update () 
	{
		if (isStarted) 
		{
			marker.SetActive(true);
			StartMarker();
		}
	}

	private void StartMarker()
	{
		Vector2 movement = new Vector2 (1, 0);
		// marker moving from left to right in the x axis
		if (dirRight) 
		{
			transform.Translate (movement * speed * Time.deltaTime);
			if (transform.position.x >= 0.35f)
			{
				dirRight = false;
			}
		} 
		else 
			// change to the opposite direction along the axis. 
		{
			transform.Translate (-movement * speed * Time.deltaTime);
			if (transform.position.x <= -0.35f) 
			{
				dirRight = true;
			}
		}	
		MeasureProprioceptiveDrift ();
	}

	// Method that will be called when proprioceptive drift needs to be measured
	public float MeasureProprioceptiveDrift()
	{
		if (Input.GetKeyDown ("space") && isStarted) 
		{
			speed = 0.0f;
			proprioceptiveDrift += transform.position.x;
			experimentController.HandleEvent (ExperimentEvents.ProprioceptiveDriftMeasured);
			isStarted = false;
		}
	return proprioceptiveDrift;
	}
}
