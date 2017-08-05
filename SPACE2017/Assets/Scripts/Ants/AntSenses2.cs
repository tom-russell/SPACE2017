using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Ants;
using Assets.Scripts.Extensions;

/* When a recruiting/reversing ant discovers another ant, this script is used to determine if the other ant can be 
 * recruited, and if so by what means (forward tandem run, reverse tandem run, social carry).
 */
public class AntSenses2 : MonoBehaviour {

    AntManager ant;
    AntManager otherAnt;
    
	void Start ()
    {
        ant = transform.parent.GetComponent<AntManager>();
	}

    private void OnTriggerEnter(Collider other)
    {
        // We are only interested in interactions between ants
        if (other.tag != Naming.Ants.Tag)
        {
            return;
        }
        // Recruitment cannot take place if either ant is already part of a tandem run or social carry
        else if (ant.IsTandemRunning() || ant.IsTransporting() || otherAnt.IsTandemRunning() || otherAnt.IsTransporting())
        {
            return;
        }

        otherAnt = other.gameObject.GetComponent<AntManager>();

        // This script is from the perspective of the recruiter, so only recruiting or reversing ants continue
        if (ant.state == BehaviourState.Reversing)
        {
            ReversingSenses();
        }
        else if(ant.state == BehaviourState.Recruiting)
        {
            RecruiterSenses();
        }
    }

    private void ReversingSenses()
    {
        // Reverse tandem runs can only be started with:
        //  - Inactive ants with the same nest allegiance
        //  - Non-passive ants
        //  - Ants that haven't been dropped recently
        if (otherAnt.state == BehaviourState.Inactive && 
            otherAnt.passive == false &&
            otherAnt.droppedRecently == 0)
        {
            ant.ReverseLead(otherAnt);
        }
    }

    private void RecruiterSenses()
    {   
        // If the other ant is eligible for recruitment, then carry them if the quorum is reached, else attempt a tandem run
        if (CanOtherAntBeRecruited() == true)
        {
            if (ant.IsQuorumReached())
            {
                ant.PickUp(otherAnt);
                return;
            }
            else if (otherAnt.passive == false) // Only non-passive ants can follow a tandem run
            {
                ant.Lead(otherAnt);
                return;
            }
        }

        // If the other ant was not able to be recruited, then reduce the recuit try counter
        if (ant.currentNest == ant.oldNest)
        {
            if (ant.recruitTime > 0)
            {
                ant.recruitTime -= 1;
            }
            else
            {
                ant.newToOld = false;
            }
        }
    }

    private bool CanOtherAntBeRecruited()
    {
        // Assessing and following ants can't be recruited
        // The other ant cannot be recruited if:
        //  - It is in the assessing or following state
        //  - It already has the same nest allegiance as this ant
        //  - Cannot be seen by this ant
        if (otherAnt.state == BehaviourState.Assessing || 
            otherAnt.state == BehaviourState.Following ||
            otherAnt.myNest == ant.myNest || 
            ant.LineOfSight(otherAnt) == false)
        {
            return false;
        }

        // If the other ant is a recruiter, use the probability parameters to test if it can be recruited.
        if (otherAnt.state == BehaviourState.Recruiting || otherAnt.state == BehaviourState.Reversing)
        {
            float r = RandomGenerator.Instance.Range(0f, 1f);
            float probability = AntScales.Other.tandRecSwitchProb;

            if (otherAnt.IsQuorumReached())
            {
                probability = AntScales.Other.carryRecSwitchProb;
            }

            if (r < probability)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // All remaining cases are eligible for recruitment, but passive ants can only be carried
        return true;
    }
}
