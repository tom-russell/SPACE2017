using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Ticking;
using Assets.Scripts.Ants;
using System;

public class AntMovement : MonoBehaviour, ITickable
{
    // Unity Components/Objects/Scripts
    public AntManager ant;
    public SimulationManager simulation;

    // Ant parameters //? Some of these may be able to move to AntScales
    public float nextTurnTime;                      // The time of the next direction change
    private float maxTimeBetweenTurns = 1f;//5f       // The maximum time allowed between direction changes
    private float maxVarBase = 20f;                 // The maximum angle (degrees) an ant can turn at one time
    private float maxVarAssessing = 20f;            // The maxVar value while assessing a new nest
    private float maxVarFollower = 20f;
    private float maxVarCollision = 90f;           // The maxVar value when turning to avoid a collision with another ant
    private Vector3 lastTurn;                       // The position of the ant at the previous direction change
    public bool isBeingCarried = false;             // If true, this ant is being carried by another ant and therefore cannot move 
    public bool followingWall = false;              // When the ant is following/aligned with a wall this boolean is true.  
    private float wallFollowBias = 0.5f;

    // Tom's parameters
    private bool passiveMove = false;

    //? redundant parameters (to get antmanager working)
    public bool usePheromones;
    public Vector3 lastPosition;
    public float intersectionNumber;
    public float assessingDistance;
    public static float gasterHeadDistance = 0f;
    public static float gasterHeadDistanceCount = 0f;

    // Initialisation of parameters 
    void Start () 
	{
        //Time.timeScale = 10f;
        ant = transform.GetComponent<AntManager>();

        turnAnt(RandomGenerator.Instance.Range(0, 360));
        nextTurnTime = simulation.TickManager.TotalElapsedSimulatedSeconds + maxTimeBetweenTurns;
        lastTurn = transform.position;
    }

    //  This is called from simulationManager to ensure movement & state updating happens in a consistent order
    public void Tick(float placeholder)
	{
        // Ants that are being socially carried cannot move
        if (isBeingCarried == true) return;
        
        // Debug option - keeps passive ants still for easier nest viewing
        if (this.ant.passive && passiveMove == false) return;

        // Call the correct movement function based on this ant's current state
        switch (ant.state)
        {
            case BehaviourState.Inactive:
                inactiveMovement();
                break;
            case BehaviourState.Scouting:
                scoutingMovement();
                break;
            case BehaviourState.Assessing:
                assessingMovement();
                break;
            case BehaviourState.Recruiting:
                recruitingMovement();
                break;
            case BehaviourState.Reversing:
                reversingMovement();
                break;
            case BehaviourState.Following:
                followingMovement();
                break;
        }
    }

    private void inactiveMovement()
    {
        // Inactive ants must stay within their own nest - if they leave turn them to face their nest centre
        if (ant.currentNest != ant.myNest)
        {
            walkToNest(ant.myNest);
            resetTurnParameters(false);
        }

        // Update direction if the required time has elapsed
        if (simulation.TickManager.TotalElapsedSimulatedSeconds >= nextTurnTime)
        {
            // Inactive ants walk randomly around inside their home nest
            randomWalk(maxVarBase);
            resetTurnParameters(false);
        }

        // Move the ant forward at the required speed (if there are no obstructions)
        moveForward(AntScales.Speeds.Inactive, true);
    }

    private void scoutingMovement()
    {
        // Update direction only if the required time has elapsed 
        if (simulation.TickManager.TotalElapsedSimulatedSeconds >= nextTurnTime)
        {
            // If a scouts senses a nest door they will walk through it (in/out the nest)
            GameObject door = doorSearch(AntScales.Distances.DoorSenseRange);
            if (door != null)
            {
                Debug.DrawLine(transform.position, door.transform.position, Color.red);
                walkToGameObject(door, false);
                resetTurnParameters(false);
            }
            else
            {
                randomWalk(maxVarBase);
                resetTurnParameters(false);
            }
        }

        // Move the ant forward at the required speed (if there are no obstructions)
        moveForward(AntScales.Speeds.Scouting, true);
    }

    private void assessingMovement()
    {
        // Ensures ants in the assessment process stay within the nest - if they leave they are turned to face the nest center
        //? Greg had a more complex system here, where direction changes are made much more frequently. Is this needed?
        if (ant.assessmentStage == NestAssessmentStage.Assessing && ant.inNest == false)
        {
            faceObject(ant.nestToAssess.gameObject);
            resetTurnParameters(moreFrequentTurns : true);
        }

        // Update direction only if the required time has elapsed 
        if (simulation.TickManager.TotalElapsedSimulatedSeconds >= nextTurnTime)
        {
            // if this is a reassessment (of previously accepted nest) then the nest to return to is the old nest, else the return nest is the ant's current nest
            //? To make this neater scout's mynest could be set to null - then the return nest is always oldNest
            NestManager returnNest;

            if (ant.myNest == ant.nestToAssess) returnNest = ant.oldNest;
            else returnNest = ant.myNest;

            if (ant.assessmentStage == NestAssessmentStage.Assessing)
            {
                randomWalk(maxVarAssessing);
            }
            else if (ant.assessmentStage == NestAssessmentStage.ReturningToHomeNest)
            {
                walkToNest(returnNest);

                // If the ant has reached the centre of the home nest, return to the nest being assessed
                if (Vector3.Distance(transform.position, returnNest.transform.position) < AntScales.Distances.AssessingNestMiddle)
                {
                    ant.assessmentStage = NestAssessmentStage.ReturningToPotentialNest;
                    ant.SetPrimaryColour(AntColours.NestAssessment.ReturningToPotentialNest);
                }
            }
            else if (ant.assessmentStage == NestAssessmentStage.ReturningToPotentialNest)
            {
                walkToNest(ant.nestToAssess);

                // If the ant has reached the centre of the assessment nest, begin the second assessment
                if (Vector3.Distance(transform.position, ant.nestToAssess.transform.position) < AntScales.Distances.AssessingNestMiddle)
                {
                    ant.NestAssessmentSecondVisit();
                }
            }

            if (ant.assessmentStage == NestAssessmentStage.Assessing) resetTurnParameters(moreFrequentTurns : true);
            else resetTurnParameters(moreFrequentTurns : false);    //? Not sure if morefrequentturns actually achieves anything here
        }

        // Move the ant forward at the required speed (if there are no obstructions)
        // If the assessor is moving between the new and old nests, so moves at standard speed
        if (ant.assessmentStage != NestAssessmentStage.Assessing)
        {
            moveForward(AntScales.Speeds.Scouting, true);
        }
        // If this is the first assessment of a new nest, move at the first visit speed
        else if (ant.nestAssessmentVisitNumber == 1)
        {
            moveForward(AntScales.Speeds.AssessingFirstVisit, true);
        }
        // Else this is the second assessment of a new nest, move at the second visit speed 
        else
        {
            moveForward(AntScales.Speeds.AssessingSecondVisit, true);
        }
    }

    private void recruitingMovement()
    {
        // If recruiter needs to wait for the follower, ant should not move (next turn time is increased so the tandem leader doesn't turn immediately after regaining contact)
        if (ant.IsTandemRunning() && shouldTandemLeaderWait() == true)
        {
            nextTurnTime += Time.fixedDeltaTime;
            return;
        }

        // Update direction only if the required time has elapsed
        if (simulation.TickManager.TotalElapsedSimulatedSeconds >= nextTurnTime)
        {
            // Recruiters leading a tandem run or transporting (social carry) will need to return to their new nest.
            if (ant.IsTandemRunning() || ant.IsTransporting())
            {
                walkToNest(ant.myNest);
            }
            // Recruiters not tandem running or carrying move back and forth between the new and old nests.
            else
            {
                if (ant.newToOld == true && ant.OldNestOccupied()) 
                {
                    // If the ant is inside the old nest then walk randomly inside to search for recruits
                    if (ant.currentNest == ant.oldNest)
                    {
                        randomWalk(maxVarBase);
                    }
                    else // Else return to the old nest to search for a recruit
                    {
                        walkToNest(ant.oldNest);
                    }
                }
                else
                {
                    walkToNest(ant.myNest);
                }
            }

            resetTurnParameters(false);
        }

        // Move forward at the speed based on the ant's current behaviour/activity
        if (ant.IsTandemRunning())
        {
            // If leading a tandem run move at standard speed but ignore collisions with other ants //? collisions confuse followers too much
            moveForward(AntScales.Speeds.TandemRunLead, true);//? 
        }
        else if (ant.IsTransporting())
        {
            moveForward(AntScales.Speeds.Carrying, true);
        }
        else // If waiting or moving between nests then move at standard speed
        {
            moveForward(AntScales.Speeds.Scouting, true);
        }
    }

    //? Need to clarify the exact behaviour of the reversing state
    private void reversingMovement()
    {
        // If (reverse)tandem leader needs to wait for the follower, ant should not move (next turn time is increased so the tandem leader doesn't turn immediately after regaining contact)
        if (ant.IsTandemRunning() && shouldTandemLeaderWait() == true)
        {
            nextTurnTime += Time.fixedDeltaTime;
            return;
        }

        // Reversing ants still searching for a tandem follower must stay within their nest
        else if (ant.IsTandemRunning() == false && ant.currentNest != ant.myNest)
        {
            walkToNest(ant.myNest); //? is this ok
            resetTurnParameters(false);
        }

        // Update direction only if the required time has elapsed
        if (simulation.TickManager.TotalElapsedSimulatedSeconds >= nextTurnTime)
        {
            // If the ant is leader a reverse tandem run, walk towards the old nest
            if (ant.IsTandemRunning())
            {
                walkToNest(ant.oldNest);
            }
            else // Else the ant is waiting in the new nest for a potential follower, so randomly walk around nest
            {
                randomWalk(maxVarBase);
            }

            resetTurnParameters(false);
        }

        // Move forward at the required speed based on the reverser's current state
        if (ant.IsTandemRunning() == true)
        {
            moveForward(AntScales.Speeds.TandemRunLead, false);//?
        }
        else
        {
            moveForward(AntScales.Speeds.ReverseWaiting, true);
        }
    }

    private void followingMovement()
    {
        // If follower is waiting (tactile contact with tandem leader) do not update movement
        if (shouldTandemFollowerWait() == true) return;

        Debug.DrawLine(transform.position, ant.estimateNewLeaderPos, Color.red);

        // If the follower has line of sight of the leader (within antennal contact range) then turn to face them
        if (ant.LineOfSight(ant.leader) == true)
        {
            walkToGameObject(ant.leader.gameObject, false);
            resetTurnParameters(moreFrequentTurns: true);
        }

        // Update direction only if the required time has elapsed
        if (simulation.TickManager.TotalElapsedSimulatedSeconds >= nextTurnTime)
        {
            
            // If this is the first follower movement estimated leader position is not yet set, move ant towards leader
            if (ant.estimateNewLeaderPos == Vector3.zero)
            {
                ant.estimateNewLeaderPos = ant.leader.transform.position;
            }

            // The follower must walk towards where they predict the leader is
            float predictedLeaderAngle = angleToFacePosition(ant.estimateNewLeaderPos);
            float newDirection = RandomGenerator.Instance.NormalRandom(predictedLeaderAngle, maxVarFollower);
            turnAnt(newDirection);

            resetTurnParameters(moreFrequentTurns: true);
        }

        // Move forward at the required speed (if there are no obstructions)
        moveForward(AntScales.Speeds.TandemRunFollow, false);//?
    }

    // If no obstructions are detected ahead of the ant, move forward at the required speed
    private void moveForward(float speed, bool antCollisions) 
    {
        // Only move forward if there are no obstructions in front of the ant
        bool moveAllowed = obstructionCheck(antCollisions);
        if (moveAllowed == true)
        {
            //?speed = speed * 6;
            //? I've simplified this section significantly due to not re-implementing the ticking. may need to change scales etc. to get it working
            float distanceToMove = Time.fixedDeltaTime * speed;
            transform.position += transform.forward * distanceToMove;
        }

        // Round the x/y/z position values (required for deterministic simulations)
        roundPosition();
    }
    
    // If there is an obstruction of the given type ahead of the ant attempt to turn to avoid, return true if successful.
    // if avoidAntCollisions is true, the ant will turn to avoid both walls & other ants. 
    private bool obstructionCheck(bool avoidAntCollisions) 
    {
        // if avoidAntCollisions is false, don't check for collisions with other ants.
        int layerMask = PhysicsLayers.AntsAndWalls;
        if (avoidAntCollisions == false)
        {
            layerMask = PhysicsLayers.Walls;
        }

        RaycastHit hitInfo = new RaycastHit();

        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, antennaeRayLength(), layerMask) == true)
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
                float newDirection = RandomGenerator.Instance.NormalRandom(currentRotation(), maxVarCollision);
                turnAnt(newDirection);
            }
            // If the obstruction is a wall, turn to move parallel to the wall
            else
            {
                alignWithWall();
            }
        }

        // retry the obstruction check - if an obstruction still exists return false (ant cannot move)
        if (Physics.Raycast(transform.position, transform.forward, antennaeRayLength(), layerMask))
        {
            return false;
        }
        else return true;
    }

    // When an ant collides with a wall they begin to follow along it. This function aligns the ant with the wall
    private void alignWithWall()
    {
        //find the direction of the wall the ant must align with
        bool[] rays = new bool[4];
        rays[0] = Physics.Raycast(transform.position, Vector3.forward, antennaeRayLength(), PhysicsLayers.Walls);
        rays[1] = Physics.Raycast(transform.position, Vector3.right, antennaeRayLength(), PhysicsLayers.Walls);
        rays[2] = Physics.Raycast(transform.position, -Vector3.forward, antennaeRayLength(), PhysicsLayers.Walls);
        rays[3] = Physics.Raycast(transform.position, -Vector3.right, antennaeRayLength(), PhysicsLayers.Walls);

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
            int closestDirection = findClosest90DegreeAngle(wallDirection);
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
                turnAnt(i * 90);
                break;
            }
        }
        
        followingWall = true;
    }

    // returns the angle1 or angle2 which is closest to the ant's current direction
    private int findClosest90DegreeAngle(int direction)
    {
        float angle1 = (direction * 90) + 90f;
        float angle2 = (direction * 90) - 90f;

        // Calcuate the smallest difference between the two option angles and the ant's current rotation
        float angle1Difference = Mathf.Abs((currentRotation() - angle1)) % 360;
        float angle2Difference = Mathf.Abs((currentRotation() - angle2)) % 360;
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

    // returns true if tandem leader must wait for follower, or false if they can move
    private bool shouldTandemLeaderWait()
    {
        // if leader is waiting for follower ensure follower is allowed to move //? not sure about this, maybe a bugfix
        if (ant.leaderWaits == true)
        {
            ant.follower.followerWait = false;
            return true;
        }

        // If the leader is greater than the stopping distance from the follower, they stop to wait.
        if (Vector3.Distance(ant.follower.transform.position, transform.position) > stoppingDistance())
        {
            // Leader must wait for follower
            ant.leaderWaits = true;

            // set parameters for lost tandem contact (leader give up time LGUT, time of lost contact)
            ant.TandemContactLost();
            ant.follower.TandemContactLost();

            return true;
        }
        else // follower and leader are close so leader shouldn't wait
        {
            return false;
        }
    }

    //? Not happy with the size/complexity of this function. A lot could be replaced with a function in Manager that ahs a single call here
    // Returns true if the follower is in contact with the leader and therefore needs to wait.is a tandem follower has 
    private bool shouldTandemFollowerWait()
    {
        // If the follower is waiting for the leader to move, return true (follower should wait)
        if (ant.followerWait == true)
        {
            // If follower has lost contact with the leader, the follower will move again
            if (Vector3.Distance(transform.position, ant.leader.transform.position) > antennaeRayLength()) //? AvAntenna -> LeaderStopping
            {
                ant.followerWait = false;
                // When contact is lost the follower must estimate the position the leader will next stop at
                estimateNextLeaderPosition();
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

        
        // If the follower has regained contact with the leader, reset the tandem variables
        if (Vector3.Distance(transform.position, ant.leader.transform.position) < antennaeRayLength())
        {
            // Follower must now wait, leader must start to move again.
            ant.followerWait = true;
            ant.leader.leaderWaits = false;
            // Re-set the 'tandem failure' time for leader & follower
            ant.TandemContactRegained();
            ant.leader.TandemContactRegained();
            // While the follower waits he will estimate the position that the leader will stop at
            //estimateNextLeaderPosition();
            //ant.leader.leaderPositionContact = ant.leader.transform.position;   //? this gets set but is never used

            return true;
        }
        return false;
    }

    // Calculates the follower's estimate of where the leader will next stop to wait.
    public void estimateNextLeaderPosition()
    {
        Vector3 leaderPos = ant.leader.transform.position;
        ant.estimateNewLeaderPos = leaderPos + (ant.leader.transform.forward * AntScales.Distances.LeaderStopping);
    }

    // Turn ant to face directly at the other GameObject
    private void faceObject(GameObject other)
    {
        float newAngle = angleToFacePosition(other.transform.position);
        turnAnt(newAngle);
    }

    // turn ant to this face the new direction (around y axis)
    private void turnAnt(float newDirection)
    {
        newDirection = newDirection % 360;
        transform.rotation = Quaternion.Euler(0, newDirection, 0);
    }

    // Returns the angle required to face this ant towards the given position
    private float angleToFacePosition(Vector3 pos2) 
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
    private void walkToGameObject(GameObject obj, bool withVariance)
    {
        float goalAngle = angleToFacePosition(obj.transform.position);

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
            turnAnt(newDirection);
        }
        else // If walking without variance, head directly towards the goal
        {
            turnAnt(goalAngle);
        }
    }

    // Resets next turn time and last turn position
    private void resetTurnParameters(bool moreFrequentTurns)
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
        nextTurnTime = simulation.TickManager.TotalElapsedSimulatedSeconds + (RandomGenerator.Instance.Range(0, 1f) * maxTime);
        lastTurn = transform.position;
    }

    // Searches around the ant in the given sensing range for any nest doors, if one is found returns the door object
    private GameObject doorSearch(float senseRange)
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
    private void randomWalk(float maxVar)
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
            updateWallFollow();
            // If the ant is still following the wall, don't change direction.
            if (followingWall == true)
            {
                return;
            }
            else
            {
                // Calculate the direction to turn so the ant moves away from the wall. If there is a wall to the right, turn left (negative rotation)
                int turnDirection = 1; 
                if (Physics.Raycast(transform.position, transform.right, antennaeRayLength() * 1.1f) == true) turnDirection = -1; //? check this is still working

                // calculate the angle change as usual, but only turn in the direction away from the wall (not into it)
                angleChange = Mathf.Abs(RandomGenerator.Instance.NormalRandom(0, maxVar)) * turnDirection;
            }
        }
        
        turnAnt(currentRotation() + angleChange);
    }

    // Random chance the ant will stop following the wall (biased by the wallFollowBias)
    private void updateWallFollow()
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
    private void walkToNest(NestManager desiredNestManager)
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
                    if (doorSearch(AntScales.Distances.DoorSenseRange) == null)
                    {
                        walkToGameObject(currentNestDoor, false);
                    }
                    // else the ant is close to the door, so they can walk directly out towards their desired nest
                    else
                    {
                        walkToGameObject(desiredNestDoor, true);
                    }
                }
            }
            // Else the ant is already in the desired nest, so move towards the nest centre
            else
            {
                walkToGameObject(desiredNest, true);
            }
        }
        // If the ant is not in a nest, walk towards the desired nest. 
        else
        {
            //If the ant is close to the desired nest door, walk directly into the nest.
            if (doorSearch(AntScales.Distances.DoorSenseRange) == desiredNestDoor)
            {
                walkToGameObject(desiredNest, false);
            }
            // Otherwise move towards the desired nest door.
            else
            {
                walkToGameObject(desiredNestDoor, true);
            }
        }
    }

    // Returns the current rotation of the ant in degrees (in the y axis)
    private float currentRotation()
    {
        return transform.rotation.eulerAngles.y;
    }

    // Raycasts start from the centre of the ant's body (this helps improve collision avoidance), so the correct distance needs half the body length added
    private float antennaeRayLength()
    {
        return AntScales.Distances.AntennaeLength + (AntScales.Distances.BodyLength / 2);
    }

    // since the distance is calculated from the center of each ant, half the body length must be added twice
    private float stoppingDistance()
    {
        return AntScales.Distances.AntennaeLength + AntScales.Distances.BodyLength;
    }

    // Rounds the current ant position to 5 decimal places
    // This is required to keep the simulations deterministic across different platforms/builds
    private void roundPosition()
    {
        int digits = 4;
        Vector3 pos = transform.position;
        Vector3 posRounded = new Vector3((float)Math.Round(pos.x, digits), (float)Math.Round(pos.y, digits), (float)Math.Round(pos.z, digits));
        transform.position = posRounded;
    }

    //? required to get ITickable to work, remove later
    public bool ShouldBeRemoved { get { return false; } }
    public void SimulationStarted() { }
    public void SimulationStopped() { }
}