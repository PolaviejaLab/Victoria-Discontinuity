﻿using NSubstitute;
using NUnit.Framework;

enum StateMachineTestStates
{
    Initial
}

enum StateMachineTestEvents
{
}

class TestStateMachine: StateMachine<StateMachineTestStates, StateMachineTestEvents>
{
    override protected void OnEnter(StateMachineTestStates state) 
    {
    }
    
    override protected void OnExit(StateMachineTestStates state)
    {
    }
}


public class StateMachineTest
{
    /**
     * Makes sure starting and stoppping of the state machine works correctly.
     */
    [Test]
    public void StartAndStop()
    {
        TestStateMachine stateMachine = new TestStateMachine();
        stateMachine.Start();
        
        Assert.Throws<StateMachineStoppedException>(() => stateMachine.GetState() );        
        Assert.That(stateMachine.IsStarted(), Is.EqualTo(false));
        stateMachine.StartMachine();
        Assert.That(stateMachine.GetState(), Is.EqualTo(StateMachineTestStates.Initial));
        Assert.That(stateMachine.IsStarted(), Is.EqualTo(true));
        stateMachine.StopMachine();
        Assert.Throws<StateMachineStoppedException>(() => stateMachine.GetState() );        
        Assert.That(stateMachine.IsStarted(), Is.EqualTo(false));        
    }
    
    
    /**
     * Makes sure the state machine is started on instantiation if
     * the StartOnInstantiation flag is set. Note that this relies on
     * the Start() function being called by unity after instantiation.
     */
    [Test]
    public void StartOnInstantiation()
    {
        TestStateMachine stateMachine = new TestStateMachine();
        stateMachine.StartOnInstantiation = true;

        Assert.Throws<StateMachineStoppedException>(() => stateMachine.GetState() );
        Assert.That(stateMachine.IsStarted(), Is.EqualTo(false));
        stateMachine.Start();
        Assert.That(stateMachine.GetState(), Is.EqualTo(StateMachineTestStates.Initial));
        Assert.That(stateMachine.IsStarted(), Is.EqualTo(true));
    }


    /**
     * Makes sure the state machine starts again after being stopped
     * if the StartOnStopMachine flag is set.
     */
    [Test]
    public void StartOnStop()
    {
        TestStateMachine stateMachine = new TestStateMachine();
        stateMachine.StartOnStopMachine = true;
        stateMachine.Start();
        
        Assert.Throws<StateMachineStoppedException>(() => stateMachine.GetState() );        
        Assert.That(stateMachine.IsStarted(), Is.EqualTo(false));
        stateMachine.StartMachine();
        Assert.That(stateMachine.GetState(), Is.EqualTo(StateMachineTestStates.Initial));
        Assert.That(stateMachine.IsStarted(), Is.EqualTo(true));
        stateMachine.StopMachine();
        Assert.That(stateMachine.GetState(), Is.EqualTo(StateMachineTestStates.Initial));
        Assert.That(stateMachine.IsStarted(), Is.EqualTo(true));     
    
    }    
}
