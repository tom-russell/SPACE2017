using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Ants;
using System;

public class AntMovement : MonoBehaviour
{
    // Unity object and script references
    public AntManager ant;
    public SimulationManager simulation;

    // Ant movement variables //? Some of these may be able to move to AntScales
    private float nextTurnTime;                     // The time of the next direction change
    private Vector3 lastTurn;                       // The position of the ant at the previous direction change
    public bool isBeingCarried = false;             // If true, this ant is being carried by another ant and therefore cannot move 
    public bool followingWall = false;              // When the ant is following/aligned with a wall this boolean is true.  
    private bool passiveMove = false;               // If true passive ants will be able to move within their home nest.

    private float maxTimeBetweenTurns = 1f;         // The maximum time allowed between direction changes
    private float maxVarBase = 20f;                 // The maximum angle (degrees) an ant can turn at one time
    private float maxVarAssessing = 20f;            // The maxVar value while assessing a new nest
    private float maxVarFollower = 20f;
    private float maxVarCollision = 90f;            // The maxVar value when turning to avoid a collision with another ant
    private float wallFollowBias = 0.9f;

    // Tandem running variables
    private Vector3 lastContactPosition;            // The last position where the leader and follower were in contact

    // Initialisation of parameters 
    void Start () 
	{
        //Time.timeScale = 10f;
        ant = transform.GetComponent<AntManager>();

        TurnAnt(RandomGenerator.Instance.Range(0, 360));
        nextTurnTime = simulation.TotalElapsedSimulatedTime("s") + maxTimeBetweenTurns;
        lastTurn = transform.position;
    }

    //  This is called from simulationManager to ensure movement & state updating happens in a consistent order
    public void Tick()
	{
        CheckIfInArenaBounds(); //? temp bugcheck

        // Ants that are being socially carried cannot move
        if (isBeingCarried == true) return;
        
        // Keeps passive ants still for much faster (~2.5x) max simulation speed and easier debugging
        if (this.ant.passive && passiveMove == false) return;

        // Call the correct movement function based on this ant's current state
        switch (ant.state)
        {
            case BehaviourState.Inactive:
                InactiveMovement();
                break;
            case BehaviourState.Scouting:
                ScoutingMovement();
                break;
            case BehaviourState.Assessing:
                AssessingMovement();
                break;
            case BehaviourState.Recruiting:
                RecruitingMovement();
                break;
            case BehaviourState.Reversing:
                ReversingMovement();
                break;
            case BehaviourState.Following:
                FollowingMovement();
                break;
        }
    }

    private void InactiveMovement()
    {
        // Inactive ants must stay within their own nest - if they leave turn them to face their nest centre
        if (ant.currentNest != ant.myNest)
        {
            WalkToNest(ant.myNest);
            ResetTurnParameters(false);
        }

        // Update direction if the required time has elapsed
        if (simulation.TotalElapsedSimulatedTime("s") >= nextTurnTime)
        {
            // Inactive ants walk randomly around inside their home nest
            RandomWalk(maxVarBase);
            ResetTurnParameters(false);
        }

        // Move the ant forward at the required speed (if there are no obstructions)
        MoveForward(Speed.v[Speed.Inactive], true);
    }

    private void ScoutingMovement()
    {
        // Update direction only if the required time has elapsed 
        if (simulation.TotalElapsedSimulatedTime("s") >= nextTurnTime)
        {
            // If a scouts senses a nest door they will walk through it (in/out the nest)
            GameObject door = DoorSearch(Length.v[Length.DoorSenseRange]);
            if (door != null)
            {
                WalkToGameObject(door, false);
                ResetTurnParameters(false);
            }
            else
            {
                RandomWalk(maxVarBase);
                ResetTurnParameters(false);
            }
        }

        // Move the ant forward at the required speed (if there are no obstructions)
        MoveForward(Speed.v[Speed.Scouting], true);
    }

    private void AssessingMovement()
    {
        // Ensures ants in the assessment process stay within the nest - if they leave they are turned to face the nest center
        if (ant.assessmentStage == NestAssessmentStage.Assessing && ant.inNest == false)
        {
            FaceObject(ant.nestToAssess.gameObject);
            ResetTurnParameters(moreFrequentTurns : true);
        }

        // Update direction only if the required time has elapsed 
        if (simulation.TotalElapsedSimulatedTime("s") >= nextTurnTime)
        {
            // if this is a reassessment (of previously accepted nest) then the nest to return to is the old nest, else the return nest is the ant's current nest
            //? To make this neater scout's mynest could be set to null - then the return nest is always oldNest
            NestManager returnNest;

            if (ant.myNest == ant.nestToAssess) returnNest = ant.oldNest;
            else returnNest = ant.myNest;

            if (ant.assessmentStage == NestAssessmentStage.Assessing)
            {
                RandomWalk(maxVarAssessing);
            }
            else if (ant.assessmentStage == NestAssessmentStage.ReturningToHomeNest)
            {
                WalkToNest(returnNest);

                // If the ant has reached the centre of the home nest, return to the nest being assessed
                if (Vector3.Distance(transform.position, returnNest.transform.position) < Length.v[Length.AssessingNestMiddle])
                {
                    ant.assessmentStage = NestAssessmentStage.ReturningToPotentialNest;
                    ant.SetPrimaryColour(AntColours.NestAssessment.ReturningToPotentialNest);
                }
            }
            else if (ant.assessmentStage == NestAssessmentStage.ReturningToPotentialNest)
            {
                WalkToNest(ant.nestToAssess);

                // If the ant has reached the centre of the assessment nest, begin the second assessment
                if (Vector3.Distance(transform.position, ant.nestToAssess.transform.position) < Length.v[Length.AssessingNestMiddle])
                {
                    ant.NestAssessmentSecondVisit();
                }
            }

            ResetTurnParameters(false);
            //if (ant.assessmentStage == NestAssessmentStage.Assessing) ResetTurnParameters(moreFrequentTurns : true);
            //else ResetTurnParameters(moreFrequentTurns : false);    //? Not sure if morefrequentturns actually achieves anything here
        }

        // Move the ant forward at the required speed (if there are no obstructions)
        // If the assessor is moving between the new and old nests, so moves at standard speed
        if (ant.assessmentStage != NestAssessmentStage.Assessing)
        {
            MoveForward(Speed.v[Speed.Scouting], true);
        }
        // If this is the first assessment of a new nest, move at the first visit speed
        else if (ant.nestAssessmentVisitNumber == 1)
        {
            MoveForward(Speed.v[Speed.AssessingFirstVisit], true);
        }
        // Else this is the second assessment of a new nest, move at the second visit speed 
        else
        {
            MoveForward(Speed.v[Speed.AssessingSecondVisit], true);
        }
    }

    private void RecruitingMovement()
    {
        // If recruiter needs to wait for the follower, ant should not move (next turn time is increased so the tandem leader doesn't turn immediately after regaining contact)
        if (ant.IsTandemRunning() && ShouldTandemLeaderWait() == true)
        {
            nextTurnTime += Time.fixedDeltaTime;
            return;
        }

        // Update direction only if the required time has elapsed
        if (simulation.TotalElapsedSimulatedTime("s") >= nextTurnTime)
        {
            // Recruiters leading a tandem run or transporting (social carry) will need to return to their new nest.
            if (ant.IsTandemRunning() || ant.IsTransporting())
            {
                WalkToNest(ant.myNest);
                UpdateTandemDistance();
            }
            // Recruiters not tandem running or carrying move back and forth between the new and old nests.
            else
            {
                // The target nest is either the old or new nest, depending on the current recruiter direction
                NestManager targetNest = (ant.recruitmentStage == RecruitmentStage.GoingToOldNest) ? ant.oldNest : ant.myNest;

                // If the recruiter has reached the target nest, then walk randomly inside while waiting/searching for recruits
                if (ant.currentNest == targetNest)
                {
                    RandomWalk(maxVarBase);
                }
                else // Else walk towards the target nest
                {
                    WalkToNest(targetNest);
                }
            }

            ResetTurnParameters(false);
        }

        // Move forward at the speed based on the ant's current behaviour/activity
        if (ant.IsTandemRunning())
        {
            MoveForward(Speed.v[Speed.TandemRunLead], true);
        }
        else if (ant.IsTransporting())
        {
            MoveForward(Speed.v[Speed.Carrying], true);
        }
        else // If waiting or moving between nests then move at standard speed
        {
            MoveForward(Speed.v[Speed.Scouting], true);
        }
    }

    //? Need to clarify the exact behaviour of the reversing state
    private void ReversingMovement()
    {
        // If (reverse)tandem leader needs to wait for the follower, ant should not move (next turn time is increased so the tandem leader doesn't turn immediately after regaining contact)
        if (ant.IsTandemRunning() && ShouldTandemLeaderWait() == true)
        {
            nextTurnTime += Time.fixedDeltaTime;
            return;
        }
        // Reversing ants still searching for a tandem follower must stay within their nest
        else if (ant.IsTandemRunning() == false && ant.currentNest != ant.myNest)
        {
            WalkToNest(ant.myNest); //? is this ok
            ResetTurnParameters(false);
        }

        // Update direction only if the required time has elapsed
        if (simulation.TotalElapsedSimulatedTime("s") >= nextTurnTime)
        {
            // If the ant is leader a reverse tandem run, walk towards the old nest
            if (ant.IsTandemRunning())
            {
                WalkToNest(ant.oldNest);
                UpdateTandemDistance();
            }
            else // Else the ant is waiting in the new nest for a potential follower, so randomly walk around nest
            {
                RandomWalk(maxVarBase);
            }

            ResetTurnParameters(false);
        }

        // Move forward at the required speed based on the reverser's current state
        if (ant.IsTandemRunning() == true)
        {
            MoveForward(Speed.v[Speed.TandemRunLead], true);
        }
        else
        {
            MoveForward(Speed.v[Speed.ReverseWaiting], true);
        }
    }

    private void FollowingMovement()
    {
        // If follower is waiting (tactile contact with tandem leader) do not update movement
        if (ShouldTandemFollowerWait() == true) return;

        // If the follower has line of sight of the leader (within antennal contact range) then turn to face them
        if (ant.LineOfSight(ant.leader) == true)
        {
            WalkToGameObject(ant.leader.gameObject, false);
            ResetTurnParameters(moreFrequentTurns: true);
        }

        // Update direction only if the required time has elapsed
        if (simulation.TotalElapsedSimulatedTime("s") >= nextTurnTime)
        {
            
            // If this is the first follower movement estimated leader position is not yet set, move ant towards leader
            if (ant.estimateNewLeaderPos == Vector3.zero)
            {
                ant.estimateNewLeaderPos = ant.leader.transform.position;
            }

            // The follower must walk towards where they predict the leader is
            float predictedLeaderAngle = AngleToFacePosition(ant.estimateNewLeaderPos);
            float newDirection = RandomGenerator.Instance.NormalRandom(predictedLeaderAngle, maxVarFollower);
            TurnAnt(newDirection);

            ResetTurnParameters(moreFrequentTurns: true);
        }

        // Move forward at the required speed (if there are no obstructions)
        MoveForward(Speed.v[Speed.TandemRunFollow], false);//?
    }

    // If no obstructions are detected ahead of the ant, move forward at the required speed
    private void MoveForward(float speed, bool antCollisions) 
    {
        // Only move forward if there are no obstructions in front of the ant
        bool moveAllowed = ObstructionCheck(antCollisions);
        if (moveAllowed == true)
        {
            //?speed = speed * 6;
            //? I've simplified this section significantly due to not re-implementing the ticking. may need to change scales etc. to get it working
            float distanceToMove = Time.fixedDeltaTime * speed;
            transform.position += transform.forward * distanceToMove;
        }

        // Round the x/y/z position values (required for deterministic simulations)
        RoundPosition();
    }
    
    // If there is an obstruction of the given type ahead of the ant attempt to turn to avoid, return true if successful.
    // if avoidAntCollisions is true, the ant will turn to avoid both walls & other ants. 
    private bool ObstructionCheck(bool avoidAntCollisions) 
    {
        // if avoidAntCollisions is false, don't check for collisions with other ants.
        int layerMask = PhysicsLayers.AntsAndWalls;
        if (avoidAntCollisions == false)
        {
            layerMask = PhysicsLayers.Walls;
        }

        RaycastHit hitInfo = new RaycastHit();

        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, AntennaeRayLength(), layerMask) == true)
        {
            // If the obstruction is another ant
            if (hitInfo.transform.CompareTag(Naming.Ants.Tag) == true)
            {
                // If passive movement is disabled then ignore collisions with passive ants (else these ants can block areas off)
                if (passiveMove == false && hitInfo.transform.GetComponent<AntManager>().passive == true)
                {
                    return true;
                }
                
                // To avoid the other ant turn to a random direction within a wide cone
                float newDirection = RandomGenerator.Instance.NormalRandom(CurrentRotation(), maxVarCollision);
                TurnAnt(newDirection);
            }
            // If the obstruction is a wall, turn to move parallel to the wall
            else
            {
                AlignWithWall();
            }
        }

        // retry the obstruction check - if an obstruction still exists return false (ant cannot move)
        if (Physics.Raycast(transform.position, transform.forward, AntennaeRayLength(), layerMask))
        {
            return false;
        }
        else return true;
    }

    // When an ant collides with a wall they begin to follow along it. This function aligns the ant with the wall
    private void AlignWithWall()
    {
        //find the direction of the wall the ant must align with
        bool[] rays = new bool[4];
        rays[0] = Physics.Raycast(transform.position, Vector3.forward, AntennaeRayLength(), PhysicsLayers.Walls);
        rays[1] = Physics.Raycast(transform.position, Vector3.right, AntennaeRayLength(), PhysicsLayers.Walls);
        rays[2] = Physics.Raycast(transform.position, -Vector3.forward, AntennaeRayLength(), PhysicsLayers.Walls);
        rays[3] = Physics.Raycast(transform.position, -Vector3.right, AntennaeRayLength(), PhysicsLayers.Walls);

        float[] dirWeights = { 1f, 1f, 1f, 1f};
        int possibleDirections = 4;
        // Determine the angle of the direction to the wall
        int wallDirection = 0;
        for (int i = 0; i < 4; i++)
        {
            // If there is a wall in this direction then the weighted chance to turn that direction is 0
            if (rays[i] == true)
            {
                dirWeights[i] = 0f;
                possibleDirections--;
                wallDirection = i;
            }
        }

        // If there are only two directions possible then the ant is in a corner - give an equal chance to turn back or to the side
        if (possibleDirections <= 2)
        {
            for (int i = 0; i < 4; i++)
            {
                dirWeights[i] *= 0.5f;
            }
        }
        else // There is only one wall in range so set the forward/side/back weights appropriately
        {
            int closestDirection = FindClosest90DegreeAngle(wallDirection);
            dirWeights[closestDirection] = 0.9f;
            dirWeights[(closestDirection + 1) % 4] = 0f;
            dirWeights[(closestDirection + 2) % 4] = 0.1f;
            dirWeights[(closestDirection + 3) % 4] = 0f;
        }
        
        float total = 0;
        float roulette = RandomGenerator.Instance.Range(0f, 1f);
        
        // Roulette wheel selection
        for (int i = 0; i < 4; i++)
        {
            total += dirWeights[i];
            if (roulette < total)
            {
                TurnAnt(i * 90);
                break;
            }
        }
        
        followingWall = true;
    }

    // returns the angle1 or angle2 which is closest to the ant's current direction
    private int FindClosest90DegreeAngle(int direction)
    {
        float angle1 = (direction * 90) + 90f;
        float angle2 = (direction * 90) - 90f;

        // Calcuate the smallest difference between the two option angles and the ant's current rotation
        float angle1Difference = Mathf.Abs((CurrentRotation() - angle1)) % 360;
        float angle2Difference = Mathf.Abs((CurrentRotation() - angle2)) % 360;
        if (angle1Difference > 180) angle1Difference -= 180;    // If the rotation is > 180 there is an equivalent rotation in the opposite direction
        if (angle2Difference > 180) angle2Difference -= 180;

        if (angle1Difference < angle2Difference)
        {
            return (direction + 1) % 4;
        }
        else
        {
            if (direction - 1 < 0)
            {
                return direction + 3;
            }    
            else 
            {
                return direction - 1;
            }
        }
    }

    private void UpdateTandemDistance()
    {
        // Update the tandem run distance
        ant.tandemDistance += Vector3.Distance(ant.prevLeaderPosition, transform.position);
        ant.prevLeaderPosition = transform.position;
    }

    // returns true if tandem leader must wait for follower, or false if they can move
    private bool ShouldTandemLeaderWait()
    {
        // if leader is waiting for follower ensure follower is allowed to move
        if (ant.leaderWaits == true)
        {
            ant.follower.followerWait = false;
            return true;
        }

        // If the leader is greater than the stopping distance from their last stop position, they stop to wait.
        if (Vector3.Distance(lastContactPosition, transform.position) > StoppingDistance())
        {
            // Leader must wait for follower
            ant.leaderWaits = true;

            return true;
        }
        else // Leader should continue moving until they have moved the 'stopping distance' since the last follower contact
        {
            return false;
        }
    }

    // Returns true if the follower is in contact with the leader and therefore needs to wait.
    private bool ShouldTandemFollowerWait()
    {
        // If the follower is waiting for the leader to move, return true (follower should wait)
        if (ant.followerWait == true)
        {
            // If follower has lost contact with the leader, the follower will move again
            if (AntSeparation(ant.leader) > Length.v[Length.AntennaeLength])
            {
                // set parameters for lost tandem contact (leader give up time LGUT, time of lost contact)
                ant.leader.TandemContactLost();
                ant.TandemContactLost();

                ant.followerWait = false;
                // When contact is lost the follower must estimate the position the leader will next stop at
                EstimateNextLeaderPosition();
            }
            return true;
        }

        // If follower is searching for their leader check if the leader has 'given up' on the tandem run
        if (ant.leader.HasLGUTExpired() == true)
        {
            // If the leader has given up, the tandem run has failed
            ant.FailedTandemRun();
            return true;
        }

        if (ant.DEBUG_ANT) Debug.Log(AntSeparation(ant.leader) + " < " + Length.v[Length.AntennaeLength]);

        // If the follower has regained contact with the leader, reset the tandem variables
        if (AntSeparation(ant.leader) < Length.v[Length.AntennaeLength])
        {
            // Follower must now wait, leader must start to move again.
            ant.followerWait = true;
            ant.leader.leaderWaits = false;

            // Re-set the 'tandem failure' time for leader & follower
            ant.TandemContactRegained();
            ant.leader.TandemContactRegained();

            // While in contact with the leader update the leader's last contact position and estimated leader stopping location
            ant.leader.GetComponent<AntMovement>().lastContactPosition = ant.leader.transform.position;
            EstimateNextLeaderPosition();

            return true;
        }
        return false;   // Else the follower continues to search for the leader
    }

    // Calculates the follower's estimate of where the leader will next stop to wait.
    public void EstimateNextLeaderPosition()
    {
        Vector3 leaderPos = ant.leader.transform.position;
        ant.estimateNewLeaderPos = leaderPos + (ant.leader.transform.forward * Length.v[Length.LeaderStopping]);
    }

    // Turn ant to face directly at the other GameObject
    private void FaceObject(GameObject other)
    {
        float newAngle = AngleToFacePosition(other.transform.position);
        TurnAnt(newAngle);
    }

    // turn ant to this face the new direction (around y axis)
    private void TurnAnt(float newDirection)
    {
        newDirection = newDirection % 360;
        transform.rotation = Quaternion.Euler(0, newDirection, 0);
    }

    // Returns the angle required to face this ant towards the given position
    private float AngleToFacePosition(Vector3 pos2) 
    {
        Vector3 pos1 = transform.position;

        Vector3 dif = pos2 - pos1;

        if (pos2.x < pos1.x)
        {
            return 360 - Vector3.Angle(Vector3.forward, dif);
        }
        else
        {
            return Vector3.Angle(Vector3.forward, dif);
        }
    }

    // Finds mid-point (angle) between the current direction and the direction of the given object, then picks a 
    // direction that is the mid-point +/- an angle <= maxVar
    //? Andy added an option to walk without variance here. may be useful?
    //? I have simplified some stuff - need to check if the multiple mod360s from the original are useful
    //? Andy did this function differently.. did normalrandom on goalAngle, not the midpoint.
    private void WalkToGameObject(GameObject obj, bool withVariance)
    {
        float goalAngle = AngleToFacePosition(obj.transform.position);

        // If walking with variance, calculate a random normally distributed angle around the midpoint of the current 
        // angle and the goal angle
        if (withVariance == true)
        {
            float currentAngle = transform.eulerAngles.y;
            float midPoint = (goalAngle + currentAngle) / 2;
            // This ensures the mid point is always the smaller of the two possible angles
            if (Mathf.Abs(midPoint - currentAngle) > 90f)
            {
                midPoint += 180f;
            }

            float newDirection = RandomGenerator.Instance.NormalRandom(midPoint, maxVarBase);
            TurnAnt(newDirection);
        }
        else // If walking without variance, head directly towards the goal
        {
            TurnAnt(goalAngle);
        }
    }

    // Resets next turn time and last turn position
    private void ResetTurnParameters(bool moreFrequentTurns)
    {
        float maxTime = maxTimeBetweenTurns;

        // Some states require for frequent turns - for these cases the max time to next turn is halved
        if (moreFrequentTurns == true)
        {
            maxTime /= 1.5f;
        }

        // if the ant hasn't moved (is stuck) wait a much shorter time, since more rotations may be required to get unstuck
        if (Vector3.Distance(transform.position, lastTurn) == 0)
        {
            maxTime /= 10;
        }
        
        //? originally assessing turn frequency was halved - Andy removed, seems sensible (assessing state was changed to encompass more behaviours, return trips etc.
        nextTurnTime = simulation.TotalElapsedSimulatedTime("s") + (RandomGenerator.Instance.Range(0, 1f) * maxTime);
        lastTurn = transform.position;
    }

    // Searches around the ant in the given sensing range for any nest doors, if one is found returns the door object
    private GameObject DoorSearch(float senseRange)
    {
        foreach (GameObject door in simulation.doors)
        {   //? doorSenseRange should be 15 not 5. Also seems odd its the same as Greg's - all other parameters have changed
            float doorDistance = Vector3.Distance(door.transform.position, transform.position);
            // InverseTransformPoint gives position relative to this ant & z is the forward direction
            bool doorIsInFront = transform.InverseTransformPoint(door.transform.position).z >= 0;

            if (doorDistance < senseRange && doorIsInFront == true)
            {
                return door;
            }
        }
        return null;
    }

    // The ant follows a random path. If the ant is following a wall, check to see if they stop following and if they do turn the ant away from the wall.
    private void RandomWalk(float maxVar)
    {
        float angleChange;

        // If not currently following a wall, turn randomly in either direction
        if (followingWall == false)
        {
            angleChange = RandomGenerator.Instance.NormalRandom(0, maxVar);
        }
        else
        {
            // If the ant is currently following a wall, check to see if the ant will turn away from the wall. 
            UpdateWallFollow();
            // If the ant is still following the wall, don't change direction.
            if (followingWall == true)
            {
                return;
            }
            else
            {
                // Calculate the direction to turn so the ant moves away from the wall. If there is a wall to the right, turn left (negative rotation)
                int turnDirection = 1; 
                if (Physics.Raycast(transform.position, transform.right, AntennaeRayLength() * 1.1f) == true) turnDirection = -1; //? check this is still working

                // calculate the angle change as usual, but only turn in the direction away from the wall (not into it)
                angleChange = Mathf.Abs(RandomGenerator.Instance.NormalRandom(0, maxVar)) * turnDirection;
            }
        }
        
        TurnAnt(CurrentRotation() + angleChange);
    }

    // Random chance the ant will stop following the wall (biased by the wallFollowBias)
    private void UpdateWallFollow()
    {
        // If a random number between 0-1 is greater than the wallFollowBias, stop following the wall
        if (RandomGenerator.Instance.NextDouble() > wallFollowBias)
        {
            followingWall = false;
        }
    }

    // Based on the ant's current location, return the next waypoint to walk to in order to reach the desired nest.
    // Order of waypoints starting from a different nest is as follows: 
    // current nest door -> desired nest door -> desired nest centre
    private void WalkToNest(NestManager desiredNestManager)
    {
        GameObject desiredNest = desiredNestManager.gameObject;
        GameObject desiredNestDoor = desiredNestManager.door;

        if (ant.currentNest != null)
        {
            GameObject currentNest = ant.currentNest.gameObject;
            GameObject currentNestDoor = ant.currentNest.GetComponent<NestManager>().door;

            // If the ant is in the wrong nest, it must walk towards the nest door and exit the nest
            if (currentNest != desiredNest)
            {
                if (currentNest != desiredNest) //? need to check this equality works properly
                {
                    // If the ant is not close to the door they must walk towards it (prevents getting stuck on walls)
                    if (DoorSearch(Length.v[Length.DoorSenseRange]) == null)
                    {
                        WalkToGameObject(currentNestDoor, false);
                    }
                    // else the ant is close to the door, so they can walk directly out towards their desired nest
                    else
                    {
                        WalkToGameObject(desiredNestDoor, true);
                    }
                }
            }
            // Else the ant is already in the desired nest, so move towards the nest centre
            else
            {
                WalkToGameObject(desiredNest, true);
            }
        }
        // If the ant is not in a nest, walk towards the desired nest. 
        else
        {
            //If the ant is close to the desired nest door, walk directly into the nest.
            if (DoorSearch(Length.v[Length.DoorSenseRange]) == desiredNestDoor)
            {
                WalkToGameObject(desiredNest, false);
            }
            // Otherwise move towards the desired nest door.
            else
            {
                WalkToGameObject(desiredNestDoor, true);
            }
        }
    }

    // Returns the current rotation of the ant in degrees (in the y axis)
    private float CurrentRotation()
    {
        return transform.rotation.eulerAngles.y;
    }

    // Raycasts start from the centre of the ant's body (this helps improve collision avoidance), so the correct distance needs half the body length added
    private float AntennaeRayLength()
    {
        return Length.v[Length.AntennaeLength] + (Length.v[Length.BodyLength] / 2);
    }

    // Since distance is calculated from the centre of each ant, separation is the distance - half the body length of both ants (so - 1x bodylength)
    private float AntSeparation(AntManager other)
    {
        return Vector3.Distance(transform.position, other.transform.position) - Length.v[Length.BodyLength];
    }

    // Stopping distance is twice antennae length. Distance is calculated from the center of each ant, half the body length must be added twice
    private float StoppingDistance()
    {
        return Length.v[Length.LeaderStopping] + Length.v[Length.BodyLength];
    }

    // Rounds the current ant position to 5 decimal places
    // This is required to keep the simulations deterministic across different platforms/builds
    private void RoundPosition()
    {
        int digits = 4;
        Vector3 pos = transform.position;
        Vector3 posRounded = new Vector3((float)Math.Round(pos.x, digits), (float)Math.Round(pos.y, digits), (float)Math.Round(pos.z, digits));
        transform.position = posRounded;
    }

    // Bug test to see if ants leave the arena at any point
    private Transform wall;
    private void CheckIfInArenaBounds()
    {
        if (!(transform.position.y == 0.1f && !isBeingCarried || transform.position.y == 0.2f && isBeingCarried))
        {
            Debug.Log("Ant " + ant.AntId + " has changed z value at position: " + transform.position);
            Time.timeScale = 0;
        }

        if (wall == null) wall = GameObject.Find("Wall(Clone)").transform;

        float arenaSize = (wall.localScale.x > wall.localScale.y) ? wall.localScale.x : wall.localScale.y;

        if (transform.position.x < 0 || transform.position.x > arenaSize ||
            transform.position.z < 0 || transform.position.z > arenaSize)
        {
            Debug.Log("Ant " + ant.AntId + " has left the arena area at position " + transform.position);
            Time.timeScale = 0;
        }
    }
}