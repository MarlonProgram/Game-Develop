using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState activestate;
    

    public void Initialise()
    {
        ChangeState(new PatrolState());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(activestate != null)
        {
            activestate.Perform();
        }
    }

    public void ChangeState(BaseState newState)
    {
        if(activestate != null)
        {
            //run cleanup on activeState
            activestate.Exit();
        }
        //change to a new state
        activestate = newState;

        if(activestate != null)
        {
            //Set up new state
            activestate.stateMachine = this;
            activestate.enemy = GetComponent<Enemy>();
            activestate.Enter();
        }
    }

}
