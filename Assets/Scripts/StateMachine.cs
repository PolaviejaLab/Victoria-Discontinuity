using UnityEngine;

using System;
using System.Collections;


public class StateMachineStoppedException :Exception { };


/**
 * Generic state machine for use in experiments
 */
public abstract class StateMachine<States, Events> :MonoBehaviour
{
	private States state;	
	private float timeAtStateChange;	
	private bool started;
	
	
	/**
	 * Initial state
	 */
	public States initialState;
	
	/**
	 * Start the state machine when the object is instantiated.
	 */
	public bool StartOnInstantiation = false;

	/**
	 * Start over after being stopped
	 */
	public bool StartOnStopMachine = false;

	/**
	 * Start the state machine
	 */
	public void StartMachine()
	{
		if(!started)
		{
			started = true;
			OnStart();
			state = initialState;	
			timeAtStateChange = Time.time;

			Debug.Log ("Entering state " + state.ToString() + 
			           " (machine just started)");

			OnEnter(state);
		}
	}


	/**
	 * Stop the state machine
	 */
	public void StopMachine()
	{
		if(started)
		{
			OnExit(state);
			timeAtStateChange = Time.time;
			started = false;		
		}
		
		if(StartOnStopMachine) 
			StartMachine();
	}
	

	/**
	 * Returns the time in seconds since entering the current state.
	 */
	public float GetTimeInState()
	{
		float time = Time.time;
		return time - timeAtStateChange;
	}
	
	
	/**
	 * Changes the current state, making sure the
	 * OnExit and OnEnter handlers are being called.
	 */
	public void ChangeState(States newState)
	{
		if(!started)
			throw new StateMachineStoppedException();
	
		OnExit (newState);
		
		States oldState = state;		
		state = newState;	
		timeAtStateChange = Time.time;	
		
		Debug.Log ("Entering state " + state.ToString() + 
				   " (previous state was " + oldState.ToString() + ")");
		
		OnEnter(oldState);
	}	


	/**
	 * Returns the current state.
	 */
	public States GetState()
	{
		if(!started)
			throw new StateMachineStoppedException();
	
		return state;
	}


	public bool IsStarted()
	{
		return started;
	}


	/**
	 * Initializes the state machine
	 */
	void Start () {
		if(StartOnInstantiation)
			StartMachine();
	}


	/**
	 * Called when the state machine is started
	 */
	virtual protected void OnStart() { }


	/**
	 * Called when leaving a state.
	 */	
	abstract protected void OnExit(States newState);
	
	
	/**
	 * Called when entering a state.
	 */	
	abstract protected void OnEnter(States oldState);
}
