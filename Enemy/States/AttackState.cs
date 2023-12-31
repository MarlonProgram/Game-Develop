using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseState
{
    private float moveTimer;
    private float losePlayerTimer;

    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Perform()
    {
        if (enemy.CanSeePlayer())
        {
            losePlayerTimer = 0;
            moveTimer += Time.deltaTime;
            enemy.transform.LookAt(enemy.Player.transform);

            if(moveTimer > Random.Range(3, 7))
            {
                enemy.Agent.SetDestination(enemy.transform.position + (Random.insideUnitSphere * 5));
                moveTimer = 0;
            }
            enemy.LastKnownPos = enemy.Player.transform.position;
        } 
        else
        {
            losePlayerTimer += Time.deltaTime;
            if(losePlayerTimer > 8)
            {
                //change to search state
                stateMachine.ChangeState(new SearchState());
            }
        }
    }
}
