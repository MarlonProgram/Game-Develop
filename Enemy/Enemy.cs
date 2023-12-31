using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private GameObject player;
    private Vector3 lastKnownPos;

    public Vector3 LastKnownPos { get => lastKnownPos; set => lastKnownPos = value; }
    public NavMeshAgent Agent { get => agent; }
    public GameObject Player { get => player; }
    //Just for debugging
    public Path path;
    public GameObject debugSphere;
    [Header("Sight Values")]
    public float sightDistance = 20f;
    public float fieldOfView = 85f;
    public float eyeHeight;

    [SerializeField]
    private string currentState;
    
    // Start is called before the first frame update
    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Initialise();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        CanSeePlayer();
        currentState = stateMachine.activestate.ToString();
        debugSphere.transform.position = lastKnownPos;
    }

    public bool CanSeePlayer()
    {
        if(player != null)
        {
            //is the player close enough to be seen?
            if(Vector3.Distance(transform.position, player.transform.position) < sightDistance)
            {
                Vector3 targetDirection = player.transform.position - transform.position - (Vector3.up * eyeHeight);
                float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
                if (angleToPlayer >= -fieldOfView && angleToPlayer <= fieldOfView)
                {
                    Ray ray = new Ray(transform.position + (Vector3.up * eyeHeight), targetDirection);
                    RaycastHit hitInfo = new RaycastHit();
                    if(Physics.Raycast(ray,out hitInfo, sightDistance))
                    {
                        if (hitInfo.transform.gameObject == player)
                        {
                            Debug.Log("Ya funco");
                            
                            
                        }
                        Debug.DrawRay(ray.origin, ray.direction * sightDistance);
                        return true;
                    }
                }
            }
        }

        return false; 

    }
}
