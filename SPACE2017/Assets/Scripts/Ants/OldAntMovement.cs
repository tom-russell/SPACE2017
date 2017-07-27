using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Ticking;
using Assets.Scripts.Ants;
/*
public class OldAntMovement : MonoBehaviour, ITickable
{
    public AntManager ant;
    public SimulationManager simulation;
    //CharacterController cont;
    BoxCollider cont;

    public bool usePheromones;

    float dir;                              //current direction
    float nextDirChange_dist;               //max distance to be moved before next direction change
    public float nextDirChange_time;               //max time before next direction change
    Vector3 lastTurn;                       //position of last direction change 
    Transform pheromoneParent;              //this will be the parent of the pheromones in the gameobject heirachy
    float nextPheromoneCheck;

    //Parameters
    public GameObject pheromonePrefab;      //the pheromone prefab
    public float maxVar = 22.5f;              //max amount this ant can turn at one time
    public float baseMaxVar = 22.5f;
    public float maxDirChange_time = 3f;//5f;    //maximum time between direction changes

    public Vector3 lastPosition;
    public float assessingDistance = 0f;
    public float intersectionNumber = 0f;

    private int frameNumber = 0;
    private int assessorChangeDirectionPerFrame = 3;

    //greg edit
    public static float gasterHeadDistance = 0f;
    public static float gasterHeadDistanceCount = 0f;

    public bool ShouldBeRemoved { get { return false; } }

    private bool _skipMove;

    private float obstructionCheckRaycastLength = 1;

    private bool _enabled = true;

    private bool _requiresObstructionCheck = false;

    private bool _isIntersectingAssessmentPheromones;

    private NestManager _targetNest;

    // Use this for initialization
    void Start()
    {
        pheromonePrefab = Resources.Load("Pheromone") as GameObject;
        ant = (AntManager)transform.GetComponent(Naming.Ants.Controller);
        //cont = (CharacterController)transform.GetComponent("CharacterController");
        cont = (BoxCollider)transform.GetComponent("BoxCollider");
        lastTurn = transform.position;
        dir = RandomGenerator.Instance.Range(0, 360);
        nextDirChange_time = simulation.TickManager.TotalElapsedSimulatedSeconds + maxDirChange_time;

        pheromoneParent = GameObject.Find(Naming.ObjectGroups.Pheromones).transform;
        nextPheromoneCheck = simulation.TickManager.TotalElapsedSimulatedSeconds;

        //greg edit
        gasterHeadDistance = 0f;
        gasterHeadDistanceCount = 0f;
    }

    private float _elapsedFTR = 0, _elapsedRTR = 0;
    public void Tick(float elapsedSimulatedMS)
    {
        // Mimic the InvokeRepeating for the tandem pheromones
        if (Ticker.Should(elapsedSimulatedMS, ref _elapsedFTR, AntScales.Speeds.PheromoneFrequencyFTR))
        {
            LayPheromoneFTR();
        }
        if (Ticker.Should(elapsedSimulatedMS, ref _elapsedRTR, AntScales.Speeds.PheromoneFrequencyRTR))
        {
            LayPheromoneRTR();
        }

        //if disabled then don't do anything
        if (!IsEnabled())
        {
            return;
        }

        if (ant.state == BehaviourState.Recruiting && ant.recruitmentStage == RecruitmentStage.WaitingInNewNest)
        {
            if (Vector3.Distance(ant.transform.position, ant.myNest.transform.position) <= AntScales.Distances.RecruitingNestMiddle)
                return;
        }

        // ensures that an assessor ant always keeps within the nest cavity 
        // if the assessor randomly leaves the nest it will turn back towards the nest centre 
        //This statements makes assessors in the nest change direction more frequently than those outside the nest.
        if (ant.state == BehaviourState.Assessing && !ant.inNest && ant.assessmentStage == NestAssessmentStage.Assessing)
        {
            ChangeDirection();
        }
        else if (ant.state == BehaviourState.Assessing && ant.inNest && ant.assessmentStage == NestAssessmentStage.Assessing)
        {
            frameNumber++;
            if ((frameNumber) % assessorChangeDirectionPerFrame == 0)
            {
                ChangeDirection();
            }
        }

        // if tandem follower is waiting return and do not update movement
        if (ant.leader != null)
        {
            if (Vector3.Distance(transform.position, ant.estimateNewLeaderPos) > AntScales.Distances.TandemFollowerLagging)
            {
                ChangeDirection();
            }
            if (HasFollowerTouchedLeader())
            {
                return;
            }
        }

        // if tandem leader is waiting return and do not update movement
        if (ant.follower != null)
        {
            if (ShouldTandemLeaderWait())
            {
                return;
            }
        }

        //move ant forwards
        ProcessMovement(elapsedSimulatedMS);

        //TODO: try pheromone and doorcheck in here
        if (!ant.inNest)
        {
            if (ant.state == BehaviourState.Scouting || ant.state == BehaviourState.Inactive)
            {
                GameObject door = DoorCheck();
                if (door != null)
                    FaceObject(door);
                else
                    Turn(dir);
            }
        }

        //wait for specified time until direction change
        if (simulation.TickManager.TotalElapsedSimulatedSeconds < nextDirChange_time)
            return;

        //change direction calculate when next direction change occurs 
        ChangeDirection();
    }

    //
    private void CheckForIntersection()
    {
        //get pheromones in assessmenPheromoneRange (mm) range (on top of)
        ArrayList pheromones = AssessmentPheromonesInRange();
        if (pheromones.Count != 0)
        {
            Pheromone p, pp;
            for (int i = 0; i < pheromones.Count; i++)
            {
                p = (Pheromone)pheromones[i];
                if (p.owner == this && p.assessingPheromoneCounted == false)
                {
                    _isIntersectingAssessmentPheromones = true;
                    intersectionNumber += 1.0f;
                    for (int j = 0; j < pheromones.Count; j++)
                    {
                        pp = (Pheromone)pheromones[j];
                        if (pp.owner == this)
                        {
                            pp.assessingPheromoneCounted = true;
                        }
                    }
                    return;
                }
                else
                {
                    _isIntersectingAssessmentPheromones = false;
                }
            }
        }
    }


    // checks what movement the tandem follower should take
    private bool HasFollowerTouchedLeader()
    {
        // if follower is waiting for leader to move -> return true (follower waits)
        if (ant.followerWait)
        {
            // if follower has lost tactile contact with leader -> begin to move (wait == false) 
            if (Vector3.Distance(transform.position, ant.leader.transform.position) > (AntScales.Distances.AverageAntenna))
            {
                ant.followerWait = false;
            }
            return true;
        }
        // if follower is searching for their leader check if LGUT has expired
        if (ant.HasLGUTExpired())
        {
            // fail tandem run if LGUT has expired
            ant.FailedTandemRun();
            return false;
        }
        // follower has made contact with leader -> reset tandem variables
        if (Vector3.Distance(transform.position, ant.leader.transform.position) < AntScales.Distances.AverageAntenna &&
            ant.LineOfSight(ant.leader.gameObject))
        {
            TandemRegainedContact();
            return true;
        }
        else
        {
            return false;
        }
    }

    // tandem follower has found their leader
    private void TandemRegainedContact()
    {
        // follower waits for leader to move 
        ant.followerWait = true;
        // leader stop waiting and continues
        ant.leader.leaderWaits = false;
        // re-set LGUT and duration of lost contact variables (for both leader and follower)
        ant.TandemContactRegained();
        ant.leader.TandemContactRegained();
        // estimate where the leader will move to while the follower waits
        EstimateNextLocationOfLeader();

        ant.leader.leaderPositionContact = ant.leader.transform.position;
    }

    // calculates the position of where the follower expects next to find the leader
    private void EstimateNextLocationOfLeader()
    {
        Vector3 leaderPos = ant.leader.transform.position;
        float angleToLeader = GetAngleBetweenPositions(transform.position, leaderPos);
        Vector3 directionToLeader = new Vector3(0, Mathf.Sin(angleToLeader), Mathf.Cos(angleToLeader));
        ant.estimateNewLeaderPos = leaderPos + (directionToLeader.normalized * AntScales.Distances.LeaderStopping);
    }

    // checks what movement the tandem leader should take
    private bool ShouldTandemLeaderWait()
    {
        // if leader is waiting for follower ensure follower is allowed to move
        if (ant.leaderWaits)
        {
            ant.follower.followerWait = false;
            return true;
        }

        // if leader is > 2mm away from follower she stops and waits
        // Richardson & Franks, Teaching in Tandem Running
        if (Vector3.Distance(ant.follower.transform.position, transform.position) < (2 * AntScales.Distances.LeaderStopping))
        {
            return false;
        }
        else
        {
            TandemLostContact();
            DistanceBetweenLeaderAndFollower();
            return true;
        }
    }

    private void DistanceBetweenLeaderAndFollower()
    {
        gasterHeadDistance += Vector3.Distance(ant.follower.transform.position, transform.position);
        gasterHeadDistanceCount += 1f;
    }

    // tandem leader has lost tactile contact with the tandem follower 
    private void TandemLostContact()
    {
        // leader waits for follower
        ant.leaderWaits = true;

        // set the tandem lost contact variables (LGUT, time of lost contact)
        ant.TandemContactLost();
        ant.follower.TandemContactLost();
    }


    //turns ant directly to object
    private void FaceObject(GameObject g)
    {
        float a = GetAngleBetweenPositions(transform.position, g.transform.position);
        Turn(a);
    }

    //move ant forwards
    public void ProcessMovement(float elapsed)
    {
        //check for obstructions, turn to avoid if there are
        ObstructionCheck();

        if (_requiresObstructionCheck)
        {
            _requiresObstructionCheck = false;
            // We have just performed an obstruction check but we might have rotated into another obstacle
            // Don't keep rotating, just stop the ant from moving this step
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, obstructionCheckRaycastLength, PhysicsLayers.Walls))
            {
                return;
            }
        }

        //move ant at appropriate speed
        if (ant.state == BehaviourState.Inactive)
        {
            MoveAtSpeed(AntScales.Speeds.Inactive, elapsed);
        }
        else if (ant.state == BehaviourState.Reversing)
        {
            MoveAtSpeed(AntScales.Speeds.TandemRunning, elapsed);
        }
        else if (ant.IsTransporting())
        {
            MoveAtSpeed(AntScales.Speeds.Carrying, elapsed);
        }
        else if (ant.IsTandemRunning())
        {
            MoveAtSpeed(AntScales.Speeds.TandemRunning, elapsed);
        }
        else if (ant.state == BehaviourState.Assessing)
        {
            if (ant.nestAssessmentVisitNumber == 1)
            {
                MoveAtSpeed(AntScales.Speeds.AssessingFirstVisit, elapsed);
            }
            else
            {
                // if this.ant.nestAssessmentVisitNumber == 2 -> second visit
                MoveAtSpeed(_isIntersectingAssessmentPheromones ? AntScales.Speeds.AssessingFirstVisitSecondVisitIntersecting : AntScales.Speeds.AssessingFirstVisitNonIntersecting, elapsed);
            }
        }
        else
        {
            MoveAtSpeed(AntScales.Speeds.Scouting, elapsed);
        }
    }

    private void MoveAtSpeed(float speed, float elapsed)
    {
        // previously this function does it once per update
        // we are now adding a simlated delta (elapsed)
        // correct the original speeds to be in the domain of 1x speed

        speed /= 1000f / 30f;
        speed /= 40;

        cont.transform.position += (transform.forward * speed * elapsed);
    }

    //change direction based on state
    public void ChangeDirection()
    {
        // the maxVar is 40f unless the ant is following a tandem run where it is 20f 
        // Franks et al. Ant Search Strategy After Interrupted Tandem Runs
        if (ant.state == BehaviourState.Scouting)
        {
            maxVar = baseMaxVar * 1.5f;
            ScoutingDirectionChange();
        }
        else if (ant.state == BehaviourState.Following)
        {
            maxVar = baseMaxVar / 2;
            FollowingDirectionChange();
        }
        else if (ant.state == BehaviourState.Inactive)
        {
            maxVar = baseMaxVar * 1.5f;
            InactiveDirectionChange();
        }
        else if (ant.state == BehaviourState.Recruiting)
        {
            maxVar = baseMaxVar / 2;
            RecruitingDirectionChange();
        }
        else if (ant.state == BehaviourState.Reversing)
        {
            maxVar = baseMaxVar;
            ReversingDirectionChange();
        }
        else
        {
            maxVar = baseMaxVar;
            AssessingDirectionChange();
        }
        Turned();
    }

    private void ScoutingDirectionChange()
    {
        GameObject door = DoorCheck();
        if (door == null)
            RandomWalk();
        else
            WalkToGameObject(door, false);
    }

    private void FollowingDirectionChange()
    {
        //if not following an ant then walk towards center of nest
        if (ant.leader == null)
        {
            transform.LookAt(ant.myNest.transform);
            // BUGFIX: if ant in the new nest and follower a RTR leader -> always LookAt(leader)
            //		} else if (this.ant.inNest && this.ant.leader.state == AntManager.State.Reversing) {
            //			transform.LookAt(this.ant.leader.transform);
            // if follower can't see the leader -> walk towards where the follower predicts the leader is
        }
        else if (!ant.LineOfSight(ant.leader.gameObject))
        {
            // first time follower moves "estimateNextLocationOfLeader()" not called 
            // therefore move ant to leader on first move
            if (ant.estimateNewLeaderPos == Vector3.zero)
            {
                ant.estimateNewLeaderPos = ant.leader.transform.position;
            }

            var posToUse = Vector3.Lerp(ant.estimateNewLeaderPos, ant.leader.transform.position, (float)RandomGenerator.Instance.NextDouble());
            float predictedLeaderAngle = GetAngleBetweenPositions(transform.position, posToUse);
            float newDir = RandomGenerator.Instance.NormalRandom(predictedLeaderAngle, maxVar);
            Turn(newDir);

            //Debug.DrawLine(ant.transform.position, ant.estimateNewLeaderPos, Color.red, 1);
        }
        else
        {
            // if the follower can see the leader then turn towards them
            transform.LookAt(ant.leader.transform);
        }
    }

    //inactive ants swarn around center of nest
    private void InactiveDirectionChange()
    {
        WalkToGameObject(NextWaypoint(), true);
    }

    //recruiters go backwards and forwards between the nests they are recruiting from and too (with randomness)
    private void RecruitingDirectionChange()
    {
        if (ant.recruitmentStage == RecruitmentStage.GoingToOldNest)
            WalkToNest(ant.oldNest);
        else if (ant.recruitmentStage == RecruitmentStage.GoingToNewNest)
            WalkToNest(ant.myNest);
        else // ant is in wait mode so just walk to the center of the nest
            WalkToGameObject(ant.myNest.gameObject, true);
    }

    private void ReversingDirectionChange()
    {
        if (ant.socialCarrying)
        {
            WalkToGameObject(ant.myNest.gameObject, true);
            return;
        }

        if (ant.newToOld && ant.OldNestOccupied())
            WalkToGameObject(NextWaypoint(), true);
        //if no ants in old nest then walk randomly
        else if (ant.newToOld)
            RandomWalk();
        else
            WalkToGameObject(NextWaypoint(), true);
    }

    public void AssessingDirectionChange()
    {
        if (ant.assessmentStage == NestAssessmentStage.ReturningToHomeNest)
        {
            WalkToNest(ant.oldNest);
            if (Vector3.Distance(transform.position, ant.oldNest.transform.position) < AntScales.Distances.AssessingNestMiddle)
            {
                ant.assessmentStage = NestAssessmentStage.ReturningToPotentialNest;
                ant.SetPrimaryColour(AntColours.NestAssessment.ReturningToPotentialNest);
            }
            return;
        }
        else if (ant.assessmentStage == NestAssessmentStage.ReturningToPotentialNest)
        {
            WalkToNest(ant.nestToAssess);
            if (Vector3.Distance(transform.position, ant.nestToAssess.transform.position) < AntScales.Distances.AssessingNestMiddle)
            {
                if (ant.inNest)
                {
                    ant.NestAssessmentSecondVisit();
                }
            }
            return;
        }

        if (ant.nestAssessmentVisitNumber == 1)
        {
            assessingDistance += Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
        }
        else if (ant.nestAssessmentVisitNumber == 2)
        {
            assessingDistance += Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
        }

        if (ant.assessTime > 0)
        {
            if (ant.inNest)
            {
                RandomWalk();
            }
            else
            {
                WalkToGameObject(ant.nestToAssess.gameObject, true);
            }
        }
        else
        {
            WalkToGameObject(NextWaypoint(), true);
        }
    }
    //

    //if running along wall this checks that new direction doesn't push ant into it
    float NewDirectionCheck(float newDir)
    {
        return newDir;

        RaycastHit hit;
        bool f, r, l, b;
        f = r = l = b = false;

        //0 <= newDir <= 360
        newDir %= 360;
        if (newDir < 0)
            newDir += 360;

        //checks forwards, backwards and both sides to see if there is a wall there
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, 1, PhysicsLayers.Walls))
            if (hit.collider.tag != Naming.Ants.Tag)
                f = true;
        if (Physics.Raycast(transform.position, -Vector3.forward, out hit, 1, PhysicsLayers.Walls))
            if (hit.collider.tag != Naming.Ants.Tag)
                b = true;
        if (Physics.Raycast(transform.position, Vector3.right, out hit, 1, PhysicsLayers.Walls))
            if (hit.collider.tag != Naming.Ants.Tag)
                r = true;
        if (Physics.Raycast(transform.position, -Vector3.right, out hit, 1, PhysicsLayers.Walls))
            if (hit.collider.tag != Naming.Ants.Tag)
                l = true;

        //this that new direction doesn't make ant try to walk through a wall and adjusts if neccessary 
        if (r && newDir < 180)
            return dir;
        else if (l && newDir > 180)
            return dir;
        else if (f && (newDir > 270 || newDir < 90))
            return dir;
        else if (b && (newDir < 270 && newDir > 90))
            return dir;
        else
            return newDir;
    }

    //tells ants where to direct themselves towards given where they currently are and what they are doing
    private GameObject NextWaypoint()
    {
        //determine if nearer old or new nest
        bool nearerOld = true;
        if (ant.oldNest == null || Vector3.Distance(transform.position, ant.myNest.transform.position) < Vector3.Distance(transform.position, ant.oldNest.transform.position))
            nearerOld = false;

        //if this is a passive ant then always direct them towards center of their nest (because they are either carried or lead between)
        if (ant.passive || ant.state == BehaviourState.Inactive)
            return ant.myNest.gameObject;

        if (ant.state == BehaviourState.Assessing)
        {
            if (ant.assessTime > 0)
            {
                return ant.nestToAssess.gameObject;
            }
            else
            {
                return ant.nestToAssess.door;
            }
        }

        //if reversing
        if (ant.state == BehaviourState.Reversing)
        {
            //If in new nest
            if (ant.inNest && !nearerOld)
            {
                //If not carrying
                if (!ant.IsTandemRunning())
                {
                    //Find an ant to carry
                    return ant.myNest.gameObject;
                }
                else
                {
                    //Head for exit
                    NestManager my = ant.myNest;
                    if (my.door != null)
                        return my.door;
                    else
                        return ant.myNest.gameObject;
                }
            }
            else
            {
                //Go to old nest
                if (!ant.inNest)
                {
                    NestManager old = ant.oldNest;
                    if (old.door != null)
                        return old.door;
                    else
                        return ant.oldNest.gameObject;
                }
                else
                {
                    return ant.oldNest.gameObject;
                }
            }
        }

        //if this ant is in a nest and is going towards their chosen nest
        if (ant.inNest && !ant.newToOld)
        {
            //if in the nest they are recruiting FROM but want to leave then return the position of the nest's door (if this has been marked)
            if (nearerOld)
            {
                NestManager old = ant.oldNest;
                if (old.door != null)
                    return old.door;
                else
                    return ant.myNest.gameObject;
            }
            //if in the nest that they are recruiting TO and don't want to leave returns the position of it's center 
            else
            {
                return ant.myNest.gameObject;
            }
        }
        //if in nest and going to towards nest that they are recruiting FROM
        else if (ant.inNest)
        {
            //in nest that they recruit TO but trying to leave then return the position of the nest's door (if this has been marked)
            if (!nearerOld)
            {
                NestManager my = ant.myNest;
                if (my.door != null)
                    return my.door;
                else
                    return ant.oldNest.gameObject;
            }
            //if in nest that recruiting FROM and are looking for ant to recruit then head towards center
            else
            {
                return ant.oldNest.gameObject;
            }
        }
        //if not in a nest and heading to nest that they recruit TO then return position of door to that nest (if possible)
        else if (!ant.newToOld)
        {
            NestManager my = ant.myNest;
            if (my.door != null)
                return my.door;
            else
                return ant.myNest.gameObject;
        }
        //if not in a nest and heading towards nest that they recruit FROM then return position of that nest's door (if possible)
        else
        {
            NestManager old = ant.oldNest;
            if (old.door != null)
                return old.door;
            else
                return ant.oldNest.gameObject;
        }
    }

    private GameObject DoorCheck()
    {
        foreach (GameObject door in simulation.doors)
        {
            if (Vector3.Distance(door.transform.position, transform.position) < AntScales.Distances.DoorSensing)
            {
                if (transform.InverseTransformPoint(door.transform.position).z >= 0)
                {
                    if (!ant.inNest)
                        return door.transform.parent.gameObject;
                    else
                        return door;
                }
            }
        }
        return null;
    }

    //resets next turn time and distance counters
    private void Turned()
    {
        _requiresObstructionCheck = true;
        //if stuck then wait less time till next change as it may take a few random rotations to get unstuck
        if (Vector3.Distance(transform.position, lastTurn) > 0)
        {
            // Assessing applies to a wide range of behaviours - doubleing the time taken here just confuses things
            //if (ant.state == BehaviourState.Assessing)
            //    nextDirChange_time = simulation.TickManager.TotalElapsedSimulatedSeconds + RandomGenerator.Instance.Range(0, 1f) * maxDirChange_time * 2f;
            //else
            nextDirChange_time = simulation.TickManager.TotalElapsedSimulatedSeconds + RandomGenerator.Instance.Range(0, 1f) * maxDirChange_time;
        }
        else
            nextDirChange_time = simulation.TickManager.TotalElapsedSimulatedSeconds + (RandomGenerator.Instance.Range(0, 1f) * maxDirChange_time) / 10f;
        lastTurn = transform.position;
    }

    public void Enable()
    {
        _enabled = true;
    }

    public void Disable()
    {
        _enabled = false;
    }

    public bool IsEnabled()
    {
        return _enabled;
    }

    public void WalkToNest(NestManager nest)
    {
        if (Vector3.Distance(transform.position, nest.door.transform.position) < AntScales.Distances.AssessingDoor)
        {
            // If the ant is just going to a nest then let them walk right to it, they can see it
            WalkToGameObject(nest.gameObject, false);
        }
        else if (Vector3.Distance(transform.position, nest.door.gameObject.transform.position) <= AntScales.Distances.AssessingDoor * 2)
        {
            // If they get close to the door then just walk in
            WalkToGameObject(nest.door.gameObject, false);
        }
        else
        {
            WalkToGameObject(nest.door.gameObject, true);
        }
    }

    //this finds mid point (angle wise) between current direction and direction of given object
	//then picks direction that is that mid point +/- an angle <= this.maxVar
    public void WalkToGameObject(GameObject target, bool withVariance)
    {
        float goalAngle;
        float currentAngle = transform.eulerAngles.y;

        //find angle mod 360 of current position to goal
        goalAngle = GetAngleBetweenPositions(transform.position, target.transform.position);

        if (Mathf.Abs(goalAngle - currentAngle) > 180)
            currentAngle -= 360;

        if (withVariance)
            goalAngle = RandomGenerator.Instance.NormalRandom(goalAngle, maxVar);

        Turn(goalAngle);

        if (false && ant.state == BehaviourState.Inactive && ant.myNest != ant.oldNest)
        {
            Debug.DrawLine(transform.position, target.transform.position, Color.white, 1);
            Debug.DrawLine(transform.position, transform.position + (5 * transform.forward), Color.red, 1);
        }
    }

    private void RandomWalk()
    {
        float maxVar = this.maxVar;
        if (ant.state == BehaviourState.Assessing)
        {
            maxVar = 10f;
        }
        if (Vector3.Distance(transform.position, lastTurn) == 0)
            maxVar = 180;
        float theta = RandomGenerator.Instance.NormalRandom(0, maxVar);
        float newDir = (dir + theta) % 360;
        Turn(newDir);
    }

    //turn ant to this face this direction (around y axis)
    private void Turn(float newDir)
    {
        _requiresObstructionCheck = true;
        dir = NewDirectionCheck(PheromoneDirection(newDir));
        transform.rotation = Quaternion.Euler(0, dir, 0);
    }

    //gets angle from v1 to v2
    private float GetAngleBetweenPositions(Vector3 v1, Vector3 v2)
    {
        Vector3 dif = v2 - v1;
        if (v2.x < v1.x)
            return 360 - Vector3.Angle(Vector3.forward, dif);
        else
            return Vector3.Angle(Vector3.forward, dif);
    }

    //this returns direction when pheromones are taken into account (using antbox algorithm)
    private float PheromoneDirection(float direction)
    {
        if (direction < 0)
            direction += 360;

        if (simulation.TickManager.TotalElapsedSimulatedSeconds < nextPheromoneCheck)
            return direction;

        //if not using pheromones then just use given direction
        // only followers that are not in a nest uses phereomones
        if (ant.inNest || (ant.state != BehaviourState.Following && ant.state != BehaviourState.Scouting))
            return direction;

        if (!simulation.Settings.AntsLayPheromones.Value)
            return direction;

        //get pheromones in range
        ArrayList pheromones = PheromonesInRange();

        //if none then just use direction
        if (pheromones.Count == 0)
            return direction;

        float strength, p_a;
        Vector3 v = transform.forward;
        //Vector3 v = Vector3.zero;
        Vector3 d;
        Pheromone p;
        Transform p_t;

        //total up weighted direction and strength of pheromones
        for (int i = 0; i < pheromones.Count; i++)
        {
            p = (Pheromone)pheromones[i];
            p_t = p.transform;
            p_a = GetAngleBetweenPositions(transform.position, p_t.position);

            //get strength of this pheromone (using equation from antbox)
            strength = p.strength * Mathf.Exp(-0.5f * (Square(2 * Mathf.Abs(p_a - direction) / maxVar)));

            //add this to the total vector
            d = p_t.position - transform.position;
            d.Normalize();
            v += d * strength;
        }

        //this stops long snake-like chains of ants following the same path over and over again
        if (RandomGenerator.Instance.Range(0f, 1f) < 0.02f)
        {
            nextPheromoneCheck = simulation.TickManager.TotalElapsedSimulatedSeconds + RandomGenerator.Instance.Range(0, 1) * maxDirChange_time;
            return RandomGenerator.Instance.NormalRandom(dir, maxVar);
        }

        //get angle and add noise
        return GetAngleBetweenPositions(transform.position, transform.position + v) + Mathf.Exp(-0.5f * (Square(2 * RandomGenerator.Instance.Range(-180, 180) / maxVar)));

    }

    private float Square(float x)
    {
        return x * x;
    }

    //randomly turns ant
    private void RandomRotate()
    {
        float maxVar = this.maxVar;
        if (Vector3.Distance(transform.position, lastTurn) == 0)
            maxVar = 180;
        float newDir = RandomGenerator.Instance.NormalRandom(0, maxVar);
        Turn(newDir);
    }

    //helps and to avoid obstructions and returns true if action was taken and false otherwise
    RaycastHit hit;
    private bool ObstructionCheck()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstructionCheckRaycastLength, PhysicsLayers.AntsAndWalls))
        {
            Debug.DrawRay(transform.position, transform.forward, Color.red, 0.1f);
            //if there is an ant directly in front of this ant then randomly turn otherwise must be a wall so follow it
            if (hit.collider.transform.tag == Naming.Ants.Tag)
            {
                // follower ant wants to have tactile contact with leader ant
                if (ant.state != BehaviourState.Following)
                {
                    RandomRotate();
                    Turned();
                }
            }
            else
                FollowWall();
            // Ants were following walls too much. I've made it only call Turned() if they collided with another ant
            // This will make them turn direction even if they have recently hit a wall
            //Turned();
            return true;
        }
        return false;
    }

    //this will only work well on right angle corners, randomness included so ant may not always follow round corner but might turn around
    private void FollowWall()
    {
        //find it if there are obstructions infront, behind and to either side of ant
        bool[] rays = new bool[4];
        rays[0] = Physics.Raycast(transform.position, Vector3.forward, obstructionCheckRaycastLength, PhysicsLayers.Walls);
        rays[1] = Physics.Raycast(transform.position, Vector3.right, obstructionCheckRaycastLength, PhysicsLayers.Walls);
        rays[2] = Physics.Raycast(transform.position, -Vector3.forward, obstructionCheckRaycastLength, PhysicsLayers.Walls);
        rays[3] = Physics.Raycast(transform.position, -Vector3.right, obstructionCheckRaycastLength, PhysicsLayers.Walls);
        float a = Mathf.Round(transform.rotation.eulerAngles.y);

        //get direction of ant (0 = forwards, 1 = right, 2 = backwards, 3 = left)
        int d = 0;
        if (a > 45 && a < 135)
            d = 1;
        else if (a > 135 && a < 225)
            d = 2;
        else if (a > 225 && a < 315)
            d = 3;

        //sets weights of directions relative current dirction (weights[0] = direction of travel, [1] = direction ± 90, [2] = direction + 180)
        float[] weights = new float[3];
        weights[0] = 1;
        weights[1] = 0.7f;
        weights[2] = 0.1f;

        //vals[i] = how much direction 'i' (same scale as 'd') contributes towards new direction
        float[] vals = new float[4];
        for (int i = 0; i < vals.Length; i++)
        {
            int dif = Mathf.Abs(i - d);
            if (dif > vals.Length / 2)
                dif = vals.Length - dif;

            //if obstruction detected in this direction then weight == 0 otherwise it equals it's respective weight
            if (rays[i])
                vals[i] = 0;
            else
                vals[i] = weights[dif];
        }

        //roulette wheel selection method for new direction (with directions with larger weight taking up more of the wheel)
        float sum = vals.Sum();
        float rand = RandomGenerator.Instance.Range(0, sum);
        int index = 0;
        float total = 0;
        for (int i = 0; i < 4; i++)
        {
            if (total + vals[i] > rand)
            {
                index = i;
                break;
            }
            else
                total += vals[i];
        }
        dir = 90f * index;
        transform.rotation = Quaternion.Euler(0, dir, 0);
    }


    private void LayPheromoneScouting()
    {
        if (!simulation.Settings.AntsLayPheromones.Value)
            return;

        GameObject pheromone = (GameObject)Instantiate(pheromonePrefab, transform.position, Quaternion.identity);
        pheromone.transform.parent = pheromoneParent;
        if (ant.state == BehaviourState.Scouting)
        {
            ((Pheromone)pheromone.transform.GetComponent(Naming.Ants.Pheromone)).LayScouting(this);
        }
    }

    private void LayPheromoneFTR()
    {
        if (!simulation.Settings.AntsLayPheromones.Value)
            return;

        if (!(ant.state == BehaviourState.Leading || ant.state == BehaviourState.Recruiting) || !usePheromones || ant.inNest)
        {
            return;
        }

        GameObject pheromone = (GameObject)Instantiate(pheromonePrefab, transform.position, Quaternion.identity);
        pheromone.transform.parent = pheromoneParent;
        if (ant.state == BehaviourState.Reversing || ant.state == BehaviourState.Leading || ant.state == BehaviourState.Recruiting)
        {
            ((Pheromone)pheromone.transform.GetComponent(Naming.Ants.Pheromone)).LayTandem(this);
        }
    }

    private void LayPheromoneRTR()
    {
        if (!simulation.Settings.AntsLayPheromones.Value)
            return;

        if (!(ant.state == BehaviourState.Reversing) || !usePheromones || ant.inNest)
        {
            return;
        }
        GameObject pheromone = (GameObject)Instantiate(pheromonePrefab, transform.position, Quaternion.identity);
        pheromone.transform.parent = pheromoneParent;
        if (ant.state == BehaviourState.Reversing || ant.state == BehaviourState.Leading || ant.state == BehaviourState.Recruiting)
        {
            ((Pheromone)pheromone.transform.GetComponent(Naming.Ants.Pheromone)).LayTandem(this);
        }
    }

    private void LayPheromoneAssessing()
    {
        if (!simulation.Settings.AntsLayPheromones.Value)
            return;

        if (ant.state != BehaviourState.Assessing || !simulation.Settings.AntsLayPheromones.Value || !ant.inNest)
        {
            return;
        }
        if (ant.nestToAssess == ant.oldNest)
        {
            return;
        }
        GameObject pheromone = (GameObject)Instantiate(pheromonePrefab, transform.position, Quaternion.identity);
        pheromone.transform.parent = pheromoneParent;
        if (ant.state == BehaviourState.Assessing)
        {
            if (ant.nestToAssess != ant.oldNest)
            {
                ((Pheromone)pheromone.transform.GetComponent(Naming.Ants.Pheromone)).LayAssessing(this);
            }
        }
    }

    private ArrayList PheromonesInRange()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, AntScales.Distances.PheromoneSensing);
        ArrayList pher = new ArrayList();
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].tag == Naming.Ants.Pheromone)
                pher.Add(cols[i].transform.GetComponent(Naming.Ants.Pheromone));
        }
        return pher;
    }

    private ArrayList AssessmentPheromonesInRange()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, AntScales.Distances.AssessmentPheromoneSensing);
        ArrayList pher = new ArrayList();
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].tag == Naming.Ants.Pheromone)
                pher.Add(cols[i].transform.GetComponent(Naming.Ants.Pheromone));
        }
        return pher;
    }

    public void SimulationStarted()
    {
    }

    public void SimulationStopped()
    {
    }
}*/
