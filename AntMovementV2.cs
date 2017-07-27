using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AntMovement : MonoBehaviour 
{   
    // Use this for initialization
	void Start () 
	{
        
    }
    
    void FixedUpdate()
	{
        // Ants that are being socially carried cannot move
        if (isBeingCarried() == true) {
            return;
        }
        
        // Debug option - keeps passive ants still for easier nest viewing
        if (this.ant.passive && passiveMove == false)
        {
            return;
        }
        
        // Call the correct movement function based on this ant's current state
        if      (this.ant.state == AntManage.State.Assessing)  assessingMovement();
        else if (this.ant.state == AntManage.State.Scouting)   scoutingMovement();
        else if (this.ant.state == AntManage.State.Following)  followingMovement();
        else if (this.ant.state == AntManage.State.Inactive)   inactiveMovement();
        else if (this.ant.state == AntManage.State.Recruiting) recruitingMovement();
        else if (this.ant.state == AntManage.State.Reversing)  reversingMovement();
        
    }
    
    private void assessingMovement() 
    {
        //? Need to check Martin's assessing code for this section
        moveForward(AntScales.Speeds.Scouting);
    }
    
    private void scoutingMovement() 
    {
        moveForward(AntScales.Speeds.Scouting);
    }
    
    private void followingMovement() 
    {
        if (HasFollowerTouchedLeader() == true) {
            return;
        }
        
        moveForward(AntScales.Speeds.TandemRunning);    //? Doesn't appear to be a slower follower speed
    }
    
    private void inactiveMovement() 
    {
        moveForward(AntScales.Speeds.Inactive);
    }
    
    private void recruitingMovement() 
    {
        if (ant.isTandemRunning() == true)     tandemLeaderMovement();
            else if (ant.isTransporting() == true) carryingMovement();
            else    
        
        moveForward(AntScales.Speeds.Scouting); //? double check this is the right speed
    }
    
    private void reversingMovement() 
    {
        moveForward(AntScales.Speeds.Scouting);
    }
    
    private void moveForward(float speed) 
    {
        ObstructionCheck();
        //? second obstruction check here, could be possible to move the first obstruction check into the Turn function
    }
    
    // If character controller is disabled, the ant is being socially carried
    private bool isBeingCarried() 
    {
        return !cont.enabled;
    }
    
    //!
    private void ObstructionCheck() 
    {
        
    }
}