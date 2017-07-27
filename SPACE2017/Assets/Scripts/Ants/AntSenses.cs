using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Ants;

public class AntSenses : MonoBehaviour
{
    AntManager ant;

    void Start()
    {
        ant = transform.parent.GetComponent<AntManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        // We only care about interactions between ants
        if (other.tag != Naming.Ants.Tag)
        {
            return;
        }

        

        AntManager otherAnt = other.transform.GetComponent<AntManager>();
        if (ant.DEBUG_ANT == true && otherAnt.state == BehaviourState.Scouting) Debug.Log("Found a scout");

        if (ant.state == BehaviourState.Reversing && !ant.IsTandemRunning())
        {
            //only inactive scouts can be reverse tandem run
            if (otherAnt.state == BehaviourState.Inactive && otherAnt.passive == false && otherAnt.droppedRecently == 0 && otherAnt.leader == null) //? I added otherAnt.leader == null to prevent multiple leaders per follower
            {
                ant.ReverseLead(otherAnt);

                return;
            }
            /*//?
			if(otherAnt.state == AntManager.State.Scouting)
			{
				ant.ReverseLead(otherAnt);
				return;
			}
			*/

        }

        //? This bit was slightly different
        /*//only continue if this ant is recruiting or reversing, the collision was with an ant and this ant isn't currently leading or carrying
        if (!(ant.state == BehaviourState.Recruiting || ant.state == BehaviourState.Reversing || ant.state == BehaviourState.ReversingLeading) || ant.IsTransporting() || ant.IsTandemRunning())
            return;*/
        // Only continue if this ant is recruiting and isn't currently part of a tandem run or social carry
        if (ant.state != BehaviourState.Recruiting || ant.IsTransporting() || ant.IsTandemRunning())
        {
            return;
        }

        //assessing and following ants can't be recruited
        if (otherAnt.state == BehaviourState.Assessing || otherAnt.state == BehaviourState.Following)
        {
            return;
        }

        //if ant already has allegiance to the same nest, or the other ant is currently transporting or tandem running or we can't see the other ant then ignore
        if (otherAnt.myNest == ant.myNest || otherAnt.IsTransporting() || otherAnt.IsTandemRunning() || !ant.LineOfSight(otherAnt))
        {
            if (ant.inNest && ant.NearerOld())
            {
                if (ant.recTime > 0)
                {
                    ant.recTime -= 1;
                }
                else
                {
                    ant.newToOld = false;
                }
            }
            return;
        }

        //if the ant is recruiting then use probabilities to decide wether they can be recruited
        if (otherAnt.state == BehaviourState.Recruiting)
        {
            float r = RandomGenerator.Instance.Range(0f, 1f);
            if (otherAnt.IsQuorumReached())
            {
                if (r > otherAnt.carryRecSwitchProb)
                    if (ant.recTime > 0)
                    {
                        ant.recTime -= 1;
                    }
                    else
                    {
                        ant.newToOld = false;
                    }
                return;
            }
            else
            {
                if (r > otherAnt.tandRecSwitchProb)
                {
                    if (ant.recTime > 0)
                    {
                        ant.recTime -= 1;
                    }
                    else
                    {
                        ant.newToOld = false;
                    }
                    return;
                }

            }
        }
        if (ant.DEBUG_ANT == true && otherAnt.state == BehaviourState.Scouting) Debug.Log("Made it to the bottom");

        //if quorum reached then carry the other ant, otherwise lead them
        if (ant.IsQuorumReached())
        {
            ant.PickUp(otherAnt);
        }
        else if (otherAnt.passive == false)
        {
            ant.Lead(otherAnt);
        }
        else
        {
            if (ant.recTime > 0)
            {
                ant.recTime -= 1;
            }
            else
            {
                ant.newToOld = false;
            }
        }
    }
}
