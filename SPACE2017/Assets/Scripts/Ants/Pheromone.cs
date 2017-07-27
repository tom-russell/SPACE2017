using Assets.Scripts.Ants;
using Assets.Scripts.Ticking;
using UnityEngine;

public class Pheromone : MonoBehaviour, ITickable
{
    public float strength;              //strength of pheromone between 0 and 1
    public float duration;              //total duration of pheromone in seconds, pheromone weakens linearly from 1 to 0 over duration
    public AntMovement owner;           //the ant who laid this pheromone 
    public bool assessingPheromone;     //identifies if the pheromone is for assessing
    public bool assessingPheromoneCounted;

    private BehaviourState _behaviour;

    public bool ShouldBeRemoved { get; private set; }

    float _elapsed = 0;
    public void Tick(float elapsedSimulationMS)
    {
        // This tick simulates the behaviour of a tick every second from InvokeRepeating
        if (Ticker.Should(elapsedSimulationMS, ref _elapsed, 1000))
        {
            switch (_behaviour)
            {
                case BehaviourState.Assessing:
                    UpdateAssessing();
                    break;
                case BehaviourState.Recruiting:
                case BehaviourState.ReversingLeading:
                    UpdateTandeom();
                    break;
                case BehaviourState.Scouting:
                    UpdateScouting();
                    break;
                default:
                    DestroyPheromone();
                    break;
            }
        }
    }

    //greg edit
    public void LayScouting(AntMovement owner)
    {
        _behaviour = BehaviourState.Scouting;

        SphereCollider myCollider = transform.GetComponent<SphereCollider>();
        //myCollider.radius = 0.5f;
        myCollider.radius = 2f;

        this.owner = owner;
        strength = -1.0f;
        assessingPheromone = false;
    }

    //greg edit
    private void UpdateScouting()
    {
        strength += (0.5f / duration);
        if (strength >= 0)
            DestroyPheromone();
    }

    public void LayTandem(AntMovement owner)
    {
        SphereCollider myCollider = transform.GetComponent<SphereCollider>();
        myCollider.radius = 0.5f;

        this.owner = owner;

        if (this.owner.ant.state == BehaviourState.Recruiting)
        {
            strength = 1.05f;
            _behaviour = BehaviourState.Recruiting;
        }
        else
        {
            _behaviour = BehaviourState.ReversingLeading;
            strength = 1.0f;
        }

        assessingPheromone = false;
    }

    private void UpdateTandeom()
    {
        strength -= (0.5f / duration);
        if (strength <= 0)
            DestroyPheromone();
    }
    
    public void LayAssessing(AntMovement owner)
    {
        _behaviour = BehaviourState.Assessing;

        SphereCollider myCollider = transform.GetComponent<SphereCollider>();
        myCollider.radius = 0.125f;

        this.owner = owner;
        strength = 0.0f;
        assessingPheromone = true;
        assessingPheromoneCounted = false;
    }

    private void UpdateAssessing()
    {
        if (owner.ant.state != BehaviourState.Assessing)
        {
            DestroyPheromone();
        }

        if (assessingPheromoneCounted)
        {
            if (Vector3.Distance(transform.position, owner.transform.position) > (AntScales.Distances.AssessmentPheromoneSensing * 1.1f))
            {
                assessingPheromoneCounted = false;
            }
        }
    }

    public void DestroyPheromone()
    {
        Destroy(gameObject);
        ShouldBeRemoved = true;
    }

    public void SimulationStarted()
    {
    }

    public void SimulationStopped()
    {
    }
}

/*
  
     \path [line] (init) -- node[midway, above, color=black] 
    	{Scouting\ \ \ \ \ \ \ \ \ \ \ \ } (switchS);
    \path [line] (init) -- node[midway, above, color=black] 
    	{\ \ \ \ \ \ \ \ \ \ \ \ Recruiting} (switchR);
    \path [line] (switchS) -- node[midway, above, color=black] 
    	{\ \ \ \ \ \ Yes} (switchSY);
    \path [line] (switchS) -- node[midway, above, color=black] 
    	{No\ \ \ \ } (switchSN);
    \path [line] (switchR) -- node[midway, above, color=black] 
    	{\ \ Yes} (switchRY);
    \path [line] (switchR) -- node[midway, above, color=black] 
    	{No\ \ \ \ \ \ } (switchRN);
    \path [line] (switchRN) -- node[midway, above, color=black] 
    	{\ \ \ \ \ \ \  $>$ 0} (passive);
    \path [line] (switchRN) -- node[midway, above, color=black] 
    	{0\ \ } (nopassive);
    

 * */
