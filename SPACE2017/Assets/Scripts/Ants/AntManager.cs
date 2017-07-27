using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Extensions;
using Assets.Scripts.Ticking;
using Assets.Scripts.Ants;
using System;

// Selection Base means clicking on an ant in the Unity scene view selects the Ant parent object rather than the child mesh
[SelectionBase]
public class AntManager : MonoBehaviour, ITickable
{
    public int AntId { get; set; }

    //Individuals properties
    public SimulationManager simulation;    // The simulation manager for the current simulation
    private AntMovement move;               // Controls movement
    private Collider sensesCol;             // The collider used to sense ants and doors
    private Transform carryPosition;        // Where to carry ant 
    public BehaviourState state;            // Which state the ant is currently in
    private BehaviourState previousState;   // The state which the ant was in prior to assessing
    public NestManager myNest;              // Recruit to this nest
    public NestManager oldNest;             // Recruit from this nest
    public NestManager nestToAssess;        // Nest that the ant is currently assessing
    public NestManager currentNest;         // Nest that the ant is inside
    public AntManager leader, follower;     // The ants that are leading or following this ant
    public bool inNest;                     // True when this ant is in a nest (needed for direction
    public bool newToOld;                   // True when the ant is head from their new nest (recruiting TO) to the old nest (recruiting FROM)
    private bool finishedRecruiting;        // True when this ant has finished recruiting and is returning to its nest
    public float nextAssessment;            // When the next assessment of the nest that this ant is will be carried out
    private float perceivedQuality;         // The quality that this ant perceives this.myNest to be
    private float perceivedQuorum;          // The quorum that this ant perceives this.myNest to have
    private float nestThreshold;            // This individual's threshold for nest quality
    public int quorumThreshold;             // Quorum threshold where recruiting ant carries rather than tandem runs
    private int revTime;                    // The countdown for recruiters attempting to find a tandem run
    public int recTime;
    private int assessTime;

    // tandem run variables
    //private int tandemTimeSteps;        //? seems to be redundant
    //private bool forwardTandemRun;      //? Think this was just used for history, so probably redundant
    //private bool reverseTandemRun;      //? Think this was just used for history, so probably redundant
    //private Vector3 startPos;           // Used to record the starting position of tandem runs or social carries (for data output)
    //private int carryingTimeSteps;      //? seems to be redundant
    //private bool socialCarrying;        //? redundant?
    public bool followerWait = true;
    public bool leaderWaits = false;
    private float startTandemRunSeconds = 0f;
    private float timeWhenTandemLostContact = 0f;
    private float leaderGiveUpTime;

    public Vector3 estimateNewLeaderPos = Vector3.zero;
    public Vector3 estimateNewLeaderPos2 = Vector3.zero;
    //private bool failedTandemLeader = false;        //? this seems redundant
    //public Vector3 leaderPositionContact;        //? redundant

    // buffon needle
    public int nestAssessmentVisitNumber = 0;
    //private float assessmentFirstLengthHistory = 0f;  //? buffon needle not implemented
    //private float assessmentSecondLengthHistory = 0f; //? buffon needle not implemented
    //private int assessmentFirstTimeHistory = 0;       //? buffon needle not implemented
    //private int assessmentSecondTimeHistory = 0;      //? buffon needle not implemented
    public NestAssessmentStage assessmentStage;
    //private float currentNestArea = 0f;               //? buffon needle not implemented

    //Parameters
    public bool passive = true;                 //is this a passive ant or not
    private bool comparisonAssess = true;       //when true this allows the ant to compare new nests it encounters to the nest it currently has allegiance to
    private float quorumAssessNoise = 1f;     //stdev of normal distribution with mean equal to the nests actual qourum from which percieved qourum is drawn
    private float assessmentNoise = 0.1f;       //stdev of normal distribution with mean equal to the nests actual quality from which percieved quality is drawn
    private float maxAssessmentWait = 45f;      //maximum wait between asseesments of a nest when a !passive ant is in the Inactive state in a nest
    private float qualityThreshNoise = 0.2f;    //the stdev of the normal distibution from which this ants quality threshold for nests is picked
    private float qualityThreshMean = 0.5f;     //the mean of the normal distibution from which this ants quality threshold for nests is picked
    public float tandRecSwitchProb = 0.3f;      //the probability that an ant that is recruiting via tandem (though not leading at this time) can be recruited by another ant
    public float carryRecSwitchProb = 0.1f;     //the probability that an ant that is recruiting via transports (though not at this time) can be recruited by another ant
    private float pRecAssessOld = 0.05f;         //the probability that a recruiter assesses its old nest when it enters it
    private float pRecAssessNew = 0.2f;         //? was 0.5 in greg's but probably for testing? //the probability that a recruiter assesses its new nest when it enters it
    private int timeStep = 0;                   //Stores time through emigration    //? seems to be redundant
    private int revTryTime = 5;                 //No. of seconds an ant spends trying to RTR.

    public int droppedRecently;                 //Flag to show if an ant has been recently dropped or not. //? i changed this to -1 to prevent reversers leading ants currently carried
    private int droppedWait = 5;
    private int recTryTime = 40;                //No. of collisions with passive ants before a recruiter gives up. //? not sure if this is the best method, makes waiting time dependent on total colony size

    //Other	
    public bool ShouldBeRemoved { get { return false; } }

    public int PerceivedTicks { get; set; }

    //?private bool _wasColourOn = false;
    //?private float _colourFlashTime = 0;
    private Color? _temporaryColour;
    private Color _primaryColour;

    private float _elapsed;
    private float _recruitmentWaitStartSeconds;
    private bool _recruitingGivingUp;
    private RecruitmentStage recruitmentStage;

    public bool DEBUG_ANT = false;

    // Use this for initialization
    void Start()
    {
        oldNest = GameObject.Find(Naming.World.InitialNest).NestManager();
        carryPosition = transform.Find(Naming.Ants.CarryPosition);
        sensesCol = transform.Find(Naming.Ants.SensesArea).GetComponent<Collider>();
        move = gameObject.AntMovement();
        nestThreshold = RandomGenerator.Instance.NormalRandom(qualityThreshMean, qualityThreshNoise);
        perceivedQuality = float.MinValue;
        finishedRecruiting = false;
        //make sure the value is within contraints
        if (nestThreshold > 1)
            nestThreshold = 1;
        else if (nestThreshold < 0)
            nestThreshold = 0;
    }

    public void Tick(float elapsedSimulationMS)
    {
        PerceivedTicks++;
        float simtime = simulation.TickManager.TotalElapsedSimulatedSeconds;
        if (AntId == 5 && simtime % 5 == 0 && simtime <= 500)
            Debug.Log("ID=" + AntId + " pos=" + transform.position);

        // If 1 second of simulated time has elapsed
        if (Ticker.Should(elapsedSimulationMS, ref _elapsed, 1000))
        {
            WriteHistory(); //? Probably doesn't need its own function any more, only one line
            DecrementCounters();    //? May be better to call every tick instead, but decrement 50ms instead of 1s (current method lines up everything to 1s intervals)
        }
        /* //? This part has been added new. Some may be useful (recruitmentStage)
        if (state == BehaviourState.Recruiting && recruitmentStage == RecruitmentStage.WaitingInNewNest)
        {
            //CheckQuorum(myNest); //?

            // Check if ant is giving up recruiting
            if (simulation.TickManager.TotalElapsedSimulatedSeconds - _recruitmentWaitStartSeconds >= AntScales.Times.RecruiterWaitSeconds)
            {
                Debug.Log("Function called");
                recruitmentStage = RecruitmentStage.GoingToOldNest;
            }
        }*/

        /*//? Test removing this, I changed the triggerExit code and nest wall thickness & movement code so hopefully it's fixed
        //BUGFIX: sometimes assessors leave nest without triggering OnExit in NestManager
        if (state == BehaviourState.Assessing && Vector3.Distance(nestToAssess.transform.position, transform.position) >
           Mathf.Sqrt(Mathf.Pow(nestToAssess.transform.localScale.x, 2) + Mathf.Pow(nestToAssess.transform.localScale.z, 2)))
            LeftNest();*/

        //? Again test this, this is a workaround not a bug fix
        //BUGFIX: occasionally when followers enter a nest there EnteredNest function doesn't get called, this forces that
        if (state == BehaviourState.Following && Vector3.Distance(LeadersNest().transform.position, transform.position) < LeadersNest().transform.localScale.x / 2f)
            EnteredNest(LeadersNest().NestManager());

        //makes Inactive and !passive ants assess nest that they are in every so often
        if (!passive && state == BehaviourState.Inactive && nextAssessment > 0 && simulation.TickManager.TotalElapsedSimulatedSeconds >= nextAssessment)
        {
            AssessNest(myNest);
            nextAssessment = simulation.TickManager.TotalElapsedSimulatedSeconds + RandomGenerator.Instance.Range(0.5f, 1f) * maxAssessmentWait;
        }

        //if an ant is carrying another and is within x distance of their nest's centre then drop the ant
        if (carryPosition.childCount > 0 && Vector3.Distance(myNest.transform.position, transform.position) < AntScales.Distances.RecruitingNestMiddle)
        {
            AntManager carriedAnt = carryPosition.Find(Naming.Ants.Tag).GetComponent<AntManager>();
            carriedAnt.Dropped(myNest);
            
            /*//? This if will always equate to true?
            // drop social carry "follower" calculate total timesteps for social carry
            if (socialCarrying == true)
            {
                carryingTimeSteps = -1 * (carryingTimeSteps - timeStep);
            }
            // get end position of social carry
            Vector3 endPos = transform.position;
            // calculate total distance and speed of social carry
            float TRDistance = Vector3.Distance(endPos, startPos);  //? These seem to no longer be used
            float TRSpeed = TRDistance / carryingTimeSteps;
            // update history with social carry and social carry speed //? This no longer happens
            if (socialCarrying == true)
            {
                socialCarrying = false;
            }*/

            // If RTRs are enabled attempt to find an ant to lead
            if (simulation.Settings.AntsReverseTandemRun.Value)
            {
                Reverse(myNest);
            }
                
        }

        //BUGFIX: Sometimes new to old is incorrectly set for recruiters - unclear why as of yet.
        if (state == BehaviourState.Recruiting && follower != null && inNest && NearerOld())
        {
            newToOld = false;
        }

        move.Tick(elapsedSimulationMS); //? Moved this to the end
    }

    private void DecrementCounters()
    {
        //Only try reverse tandem runs for a certain amount of time
        if (state == BehaviourState.Reversing && inNest && !NearerOld() && follower == null)
        {
            if (revTime < 1)
            {
                RecruitToNest(myNest);
            }
            else
            {
                revTime -= 1;
            }
        }

        if (droppedRecently > 0)
        {
            droppedRecently -= 1;
        }

        if (state == BehaviourState.Assessing && assessTime > 0)
        {
            assessTime -= 1;
        }
        else if (state == BehaviourState.Assessing && assessmentStage == NestAssessmentStage.Assessing)
        {
            NestAssessmentVisit();
        }

    }

    private void WriteHistory() //? Most of this function is removed, should be fine. but can probably be deleted
    {
        //Update timestep
        timeStep++;
    }

    // Assigns the ant to the correct parent gameobject, based on state and current nest allegiance
    private void AssignParent()
    {
        if (transform.parent.tag == Naming.Ants.CarryPosition)
        {
            return;
        }
        else if (state == BehaviourState.Recruiting)
        {
            AssignParentFromNest(myNest, Naming.Ants.BehavourState.Recruiting, AntColours.States.Recruiting);
        }
        else if (state == BehaviourState.Inactive)
        {
            Color inactiveColour = AntColours.States.Inactive;
            if (passive == true) inactiveColour = AntColours.States.InactivePassive;
            AssignParentFromNest(myNest, Naming.Ants.BehavourState.Inactive, inactiveColour);
        }
        else if (state == BehaviourState.Scouting && transform.parent.name != "S")
        {
            AssignParentFromNest(null, Naming.Ants.BehavourState.Scouting, AntColours.States.Scouting);
        }
        else if (state == BehaviourState.Assessing)
        {
            AssignParentFromNest(nestToAssess, Naming.Ants.BehavourState.Assessing, AntColours.States.Assessing);
        }
        else if (state == BehaviourState.Reversing)
        {
            AssignParentFromNest(myNest, Naming.Ants.BehavourState.Reversing, AntColours.States.Reversing);
        }
    }

    private void AssignParentFromNest(NestManager nest, string prefix, Color? colour)
    {
        if (nest != null)
        {
            prefix += simulation.GetNestID(nest);
        }
        if (transform.parent.name != prefix)
        {
            transform.parent = GameObject.Find(prefix).transform;
            if (colour.HasValue)
            {
                SetPrimaryColour(colour.Value);
            }
        }
    }

    //returns true if this ant is carrying another or is being carried
    public bool IsTransporting()
    {
        if (transform.parent.tag == Naming.Ants.CarryPosition || carryPosition.childCount > 0)
        {
            return true;
        }
        else {
            return false;
        }
    }

    //returns true if this ant is leading or being led
    public bool IsTandemRunning()
    {
        if (follower != null || leader != null)
        {
            return true;
        }
        else {
            return false;
        }
    }

    private GameObject LeadersNest()
    {
        return leader.myNest.gameObject;
    }

    // Tell this ant to lead 'follower' to preferred nest //? This could probably be merged with the reverseLead function below
    public void Lead(AntManager follower)
    {
        /*//? Reset the failedTandemLeader boolean (could be done elsewhere perhaps)
        if (failedTandemLeader == true && state == BehaviourState.Recruiting)
        {
            failedTandemLeader = false;
        }*/

        // set variables for start of forward tandem run (log start position and timestep)
        startTandemRunSeconds = simulation.TickManager.TotalElapsedSimulatedSeconds;

        // Reset the leader giving up time
        leaderGiveUpTime = AntScales.Times.startingLeaderGiveUpTime;

        /*forwardTandemRun = true;
        startPos = transform.position;
        tandemTimeSteps = timeStep;
        leaderPositionContact = transform.position;*/

        // allow FTR leader to lay pheromones at rate 1.85 per sec (Basari, Trail Laying During Tandem Running)
        move.usePheromones = true;

        //let following ant know that you're leading it
        this.follower = follower;
        this.follower.Follow(this);
        newToOld = false;

        //turn this ant around to face towards chosen nest
        transform.LookAt(myNest.transform);
    }

    public void ReverseLead(AntManager follower)
    {
        // set start of reverse tandem run (log start position and timestep)
        startTandemRunSeconds = simulation.TickManager.TotalElapsedSimulatedSeconds;

        // Reset the leader giving up time
        leaderGiveUpTime = AntScales.Times.startingLeaderGiveUpTime;

        /*reverseTandemRun = true;
        startPos = transform.position;
        tandemTimeSteps = timeStep;
        leaderPositionContact = transform.position;*/

        // allow FTR leader to lay pheromones at rate 1.85 per sec (Basari, Trail Laying During Tandem Running)
        move.usePheromones = true;  //? pheromones currently not implemented

        //let following ant know that you're leading it
        this.follower = follower;
        this.follower.Follow(this);
        newToOld = true;

        //turn this ant around to face towards chosen nest
        transform.LookAt(oldNest.transform);
    }

    public void StopLeading()
    {
        startTandemRunSeconds = 0;
        followerWait = true;
        leaderWaits = false;

        /*// get total time steps taken for tandem run
        tandemTimeSteps = -1 * (tandemTimeSteps - timeStep);

        // get end poistion of tandem run
        Vector3 endPos = transform.position;
        // calculate distance covered for tandem run
        float TRDistance = Vector3.Distance(endPos, startPos);      //? no longer used anywhere
        // calculate the speed of tandem run (Unity Distance / Unity timesteps) 
        float TRSpeed = TRDistance / tandemTimeSteps;

        // update forward / reverse tandem run speed and successful tandem run
        if (forwardTandemRun == true)
        {
            forwardTandemRun = false;
        }
        else if (reverseTandemRun ==true)
        {
            reverseTandemRun = false;
        }*/

        if (follower.currentNest == myNest) simulation.simData.successFTR++;    //? ftr failure data collection
        else simulation.simData.successRTR++;

        // leader has stopped leader => does not lay pheromones [as frequently]
        // (Basari, Trail Laying During Tandem Running)
        move.usePheromones = false; //? pheromones not in use currently
         
        follower = null;
        RecruitToNest(myNest);
    }

    //returns true if there is a line of sight between this ant and the given other ant
    public bool LineOfSight(AntManager otherAnt)
    {
        float distance = 2f;    //? These were unchanged so I divided both by 10
        if (leader != null) { distance = 2f; } //? was 0.45f before
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, otherAnt.transform.position - transform.position, out hit, distance))
        {
            // The collider hit will be the mesh, and the mesh parent is the ant object
            if (hit.collider.transform.parent == otherAnt.transform)
            {
                return true;
            }   
        }

        return false;
    }

    //follow the leader ant 
    public void Follow(AntManager leader)
    {
        startTandemRunSeconds = simulation.TickManager.TotalElapsedSimulatedSeconds;

        //start following leader towards nest
        ChangeState(BehaviourState.Following);
        newToOld = false;
        this.leader = leader;

        //we want to turn to follow the leader now
        //? move.ChangeDirection();
        transform.LookAt(leader.transform);

        followerWait = true;
    }

    public void StopFollowing()
    {
        followerWait = true;
        leaderWaits = false;
        startTandemRunSeconds = 0;
        estimateNewLeaderPos = Vector3.zero;
        leader = null;
    }

    //makes this ant pick up 'otherAnt' and carry them back to preffered nest
    public void PickUp(AntManager otherAnt)
    {
        /*socialCarrying = true;
        startPos = transform.position;
        carryingTimeSteps = timeStep;*/

        otherAnt.PickedUp(transform);
        newToOld = false;
        transform.LookAt(myNest.transform);
    }

    //lets this ant know that it has been picked up by carrier
    public void PickedUp(Transform carrier)
    {
        // Disable movement for carried ants
        move.isBeingCarried = true;

        //get into position
        Transform carryPosition = carrier.Find(Naming.Ants.CarryPosition);
        transform.parent = carryPosition;
        transform.position = carryPosition.position;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        //turn off senses
        sensesCol.enabled = false;
    }

    //lets this ant know that it has been put down, sets it upright and turns senses back on 
    public void Dropped(NestManager nest)
    {
        //turn the right way up 
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(transform.position.x, 0.15f, transform.position.z);
        move.isBeingCarried = false;        //? could consider moving isBeingCarried into antmanager

        if (transform.parent.tag == Naming.Ants.CarryPosition)
        {
            int id = simulation.GetNestID(nest);
            transform.parent = GameObject.Find("P" + id).transform;
        }

        //make ant inactive in this nest
        // oldNest = nest; //? This is commented out in Gregs version
        myNest = nest;
        droppedRecently = droppedWait;
        //? commented out in both versions?
        nextAssessment = simulation.TickManager.TotalElapsedSimulatedSeconds + RandomGenerator.Instance.Range(0.5f, 1f) * maxAssessmentWait;
        ChangeState(BehaviourState.Inactive);

        //turns senses on if non passive ant
        if (passive == false)
        {
            sensesCol.enabled = true;
        }
    }

    //? very poorly named function. bit weird this is called from antmovement too. parts/all could be redundant
    //? function doesnt make much sense to me? why is newToOld set true if the oldnest is empty 
    //returns true if ant is within certain range of nest centre and there are no more passive ants ro recruit there
    public bool OldNestOccupied()
    {
        if (oldNest == null) return false;

        int id = simulation.GetNestID(oldNest);
        if (GameObject.Find("P" + id).transform.childCount == 0 && Vector3.Distance(oldNest.transform.position, transform.position) < 1f)   //? changed range 10 to 1 here
        {
            //oldNest = null;
            newToOld = true;
            return false;
        }
        else return true;
    }

    //this is called whenever an ant enters a nest
    public void EnteredNest(NestManager nest)
    {
        /*if (failedTandemLeader == true && state == BehaviourState.Recruiting && nest != oldNest)
        {
            failedTandemLeader = false; //? this seems redundant
        }*/

        currentNest = nest;
        inNest = true;

        /* // this part isn't in gregs? may be from somewhere else
        if (state == BehaviourState.Recruiting)
        {
            if (nest == oldNest)
            {
                recruitmentStage = RecruitmentStage.GoingToNewNest;
                return;
            }
            else if (nest == myNest && !IsQuorumReached()) // don't wait if quorum reached
            {
                recruitmentStage = RecruitmentStage.WaitingInNewNest;
                _recruitmentWaitStartSeconds = simulation.TickManager.TotalElapsedSimulatedSeconds;
                return;
            }
        } */

        //? I think this case would be covered by the block below
        //ignore ants that have just been dropped here
        if (nest == myNest && state == BehaviourState.Inactive)
            return;

        //ignore ants that are carrying or are being carried
        //? transform.parent.tag == Naming.Ants.CarryPosition is being used for carried ants - could just move isBeingCarried into antmanager
        if (carryPosition.childCount > 0 || transform.parent.tag == Naming.Ants.CarryPosition)
            return;

        //if this ant has been lead to this nest then tell leader that it's done its job
        if (state == BehaviourState.Following && leader != null)
        {
            if (leader.state == BehaviourState.Recruiting && nest != leader.oldNest)
            {
                leader.StopLeading();
                StopFollowing();
                if (passive)    //? i don't think passive ants can be tandem followers
                {
                    myNest = nest;
                    ChangeState(BehaviourState.Inactive);
                    return;
                }
                else
                {
                    nestToAssess = nest;
                    ChangeState(BehaviourState.Assessing);
                }
            }
            else if (leader.state == BehaviourState.Reversing && nest == leader.oldNest)
            {
                myNest = leader.myNest;
                oldNest = leader.oldNest;
                leader.StopLeading();
                StopFollowing();
                RecruitToNest(myNest);
            }
        }

        //if entering own nest and finished recruiting then become inactive
        if (state == BehaviourState.Recruiting && nest == myNest)
        {

            if (finishedRecruiting == true)
            {
                ChangeState(BehaviourState.Inactive);
                finishedRecruiting = false;
                return;
            }
            else
            {
                // If the quorum is not yet reached, there 
                if (follower == null && RandomGenerator.Instance.Range(0f, 1f) < pRecAssessNew && !IsQuorumReached())
                {
                    nestToAssess = nest;
                    ChangeState(BehaviourState.Assessing);
                }
                else
                {
                    RecruitToNest(nest);
                }
            }
        }

        if (state == BehaviourState.Recruiting && nest == oldNest)
        {

            //if no passive ants left in old nest then turn around and return home
            if (finishedRecruiting == true || nest.GetPassive() == 0)
            {
                newToOld = false;
                finishedRecruiting = true;
                return;
            }
            //if recruiting and this is old nest then assess with probability pRecAssessOld
            else if (follower == null && RandomGenerator.Instance.Range(0f, 1f) < pRecAssessOld)
            {
                nestToAssess = nest;
                ChangeState(BehaviourState.Assessing);
                return;
            }
        }

        if (state == BehaviourState.Reversing && nest == oldNest)
        {
            if (nest.GetPassive() == 0)
            {
                newToOld = false;
                ChangeState(BehaviourState.Recruiting);
                finishedRecruiting = true;
                return;
            }
        }


        //if either ant is following or this isn't one of the ants known nests then assess it
        if (((state == BehaviourState.Scouting || state == BehaviourState.Recruiting) && nest != oldNest && nest != myNest) || state == BehaviourState.Following && leader.state != BehaviourState.Reversing && nest != leader.oldNest)
        {
            if (follower != null)
            {
                follower.FailedTandemFollowerBehaviour();
                StopLeading();//? Obscure bugfix
            }
            nestToAssess = nest;
            ChangeState(BehaviourState.Assessing);
        }
        else
        {
            /* //? This bit is commented out
            int id = simulation.GetNestID(nest);
            //if this nest is my old nest and there's nothing to recruit from it then stop coming here
            if (nest == oldNest && GameObject.Find("P" + id).transform.childCount == 0)     //? could have a better way to check passive ants left in nest
                //oldNest = null;    
                */

            //if recruiting and this is your nest then go back to looking around for ants to recruit 
            if (state == BehaviourState.Recruiting && nest == myNest && follower == null)
            {
                RecruitToNest(myNest);
            }
        }

        /* //? not sure what this is for
        // Assessing ant has entered a nest, make sure it updates the assement bit
        if (state == BehaviourState.Assessing)
        {
            //? move.AssessingDirectionChange();
        }*/
    }

    // Called after the first or second nest visit has finished (and the ant has left the nest)
    private void NestAssessmentVisit()
    {
        if (nestAssessmentVisitNumber == 1)
        {
            // store lenght of first visit and reset length to zero
            //?assessmentFirstLengthHistory = move.assessingDistance;
            //?move.assessingDistance = 0f;
            assessmentStage = NestAssessmentStage.ReturningToHomeNest;
            SetPrimaryColour(AntColours.NestAssessment.ReturningToHomeNest);
        }
        else // Second visit has finished, so assessment is complete
        {
            //?assessmentSecondLengthHistory = move.assessingDistance;
            //?move.assessingDistance = 0f;

            // store buffon needle assessment values and reset (once assessment has finishe)
            //?StoreAssessmentHistory();

            AssessNest(nestToAssess);
        }
    }

    //? This is called from antmovement, for some reason
    public void NestAssessmentSecondVisit()
    {
        SetPrimaryColour(AntColours.NestAssessment.SecondVisit);
        assessmentStage = 0;
        nestAssessmentVisitNumber = 2;
        assessTime = GetAssessTime();
        move.usePheromones = false;
    }

    /*//?private void StoreAssessmentHistory()
    {
        if (nestAssessmentVisitNumber != 2)
        {
            return;
        }

        if (move.intersectionNumber != 0f)
        {
            float area = (2.0f * assessmentFirstLengthHistory * assessmentSecondLengthHistory) / (3.14159265359f * move.intersectionNumber);
            currentNestArea = area;
        }

        // reset values
        assessmentFirstLengthHistory = 0f;
        assessmentSecondLengthHistory = 0f;
        assessmentFirstTimeHistory = 0;
        assessmentSecondTimeHistory = 0;
        nestAssessmentVisitNumber = 0;
        move.intersectionNumber = 0f;
    }*/

    //assesses nest and takes appropriate action
    private void AssessNest(NestManager nest)
    {
        move.usePheromones = false;

        //reset current nest area
        //?currentNestArea = 0f;

        // Nest quality measurement (not buffon's needle, random value from normal distribution, constrained between 0-1)
        float q = RandomGenerator.Instance.NormalRandom(nest.quality, assessmentNoise);
        if (q < 0f)
            q = 0f;
        else if (q > 1f)
            q = 1f;


        //if an inactive (& non-passive) ant decides that his current isn't good enough then go look for another
        if (state == BehaviourState.Inactive && nest == myNest)
        {
            perceivedQuality = q;
            if (q < nestThreshold)
            {
                // oldNest = myNest; //? this is commented in greg's version
                ChangeState(BehaviourState.Scouting);
            }
        }
        else
        {
            //if not using comparison then check if this nest is as good or better than threshold
            if (comparisonAssess == false && q >= nestThreshold)
            {
                if (nest != myNest)
                {
                    oldNest = myNest; //? this is commented in greg's version
                }
                if (follower != null)
                {
                    follower.myNest = nest;
                    StopLeading();
                }
                perceivedQuality = q;
                RecruitToNest(nest);
            }
            //if using comparison then check if this reaches threshold and is better than previous nest
            else if (comparisonAssess == true && q >= nestThreshold && (myNest == null || q > perceivedQuality))
            {
                if (DEBUG_ANT == true) Debug.Log("comparison assess successful: " + q + "/" + nestThreshold);
                if (nest != myNest)
                {
                    oldNest = myNest; //? this is commented in greg's version
                }

                if (follower != null)
                {
                    follower.myNest = nest;     //? Should the follower do this?
                    StopLeading();
                }
                perceivedQuality = q;
                RecruitToNest(nest);

            }
            else // The new nest failed the assessment, so resume the previous state
            {
                if (DEBUG_ANT == true) Debug.Log("comparison assess failed: " + q + "/" + nestThreshold);
                if (previousState == BehaviourState.Scouting)
                    ChangeState(BehaviourState.Scouting);
                else if (previousState == BehaviourState.Recruiting)
                    RecruitToNest(myNest);
            }
        }
    }

    //returns true if quorum has been reached in this.myNest 
    public bool IsQuorumReached()
    {
        return perceivedQuorum >= quorumThreshold;
    }

    //called whenever an ant leaves a nest
    public void LeftNest()
    {
        currentNest = null;
        inNest = false;
        if (DEBUG_ANT == true) Debug.Log("Left nest");
        //when an assessor leaves the nest then make decision about wether to recruit TO that nest
        if (state == BehaviourState.Assessing && assessTime == 0)
        {
            if (assessmentStage == 0)
            {
                NestAssessmentVisit();
            }
        }
    }

    //changes state of ant and assigns the correct parent in gameobject heirachy
    public void ChangeState(BehaviourState newState)
    {
        if (newState == BehaviourState.Assessing)
        {
            ChangeToAssessingState();
        }
        if (this.state != BehaviourState.Following && this.state != BehaviourState.Assessing)
        {
            previousState = this.state;
        }
        this.state = newState;
        /* //? not sure where this code block comes from
        if (this.state == BehaviourState.Recruiting)
        {
            _recruitingGivingUp = false;
            recruitmentStage = RecruitmentStage.GoingToOldNest;
        }*/

        AssignParent();
    }

    private void ChangeToAssessingState()
    {
        // make this nest assessment their first visit
        nestAssessmentVisitNumber = 1;
        assessTime = GetAssessTime();
        // get start position of assessor ant
        move.lastPosition = move.transform.position;
        move.usePheromones = true;
    }

    private int GetAssessTime()
    {
        // Eamonn B. Mallon and Nigel R. Franks - Ants estimate area using Buffon's needle
        // The median time that a scout spends within a nest cavity assessing a potential nest is 110 s per visit (interquartile range 140 s and n = 115)
        // range must be +/- 55. As 125+55=180 (110+70). And 95-55=40 (110-70)

        int averageAssessTime;
        if (nestAssessmentVisitNumber == 1)
        {
            averageAssessTime = AntScales.Times.averageAssessTimeFirstVisit;
        }
        else
        {
            averageAssessTime = AntScales.Times.averageAssessTimeSecondVisit;
        }

        float deviate = (float) RandomGenerator.Instance.UniformDeviate(-1, 1);
        int duration = (averageAssessTime + (int)(AntScales.Times.halfIQRangeAssessTime * deviate));

        /*if (nestAssessmentVisitNumber == 1)
        {
            assessmentFirstTimeHistory = duration;
        }
        else
        {
            assessmentSecondTimeHistory = duration;
        }*/
        return duration;
    }
    //

    //switches ants allegiance to this nest and sends them back to their old one to recruit some more
    private void RecruitToNest(NestManager nest)
    {
        newToOld = true;
        myNest = nest;
        CheckQuorum(nest);

        recTime = recTryTime;

        leader = null; //? BUGFIX == random case where an assessor -> recruiter but still had leader set to something, so caused a null exception

        ChangeState(BehaviourState.Recruiting);
    }

    private void CheckQuorum(NestManager nest)
    {
        //check the quorum of this nest until quorum is met once.
        if (IsQuorumReached())
        {
            perceivedQuorum = quorumThreshold;
        }
        else
        {
            perceivedQuorum = RandomGenerator.Instance.NormalRandom(nest.GetQuorum(), quorumAssessNoise);
        }
    }

    private void Reverse(NestManager nest)
    {
        ChangeState(BehaviourState.Reversing);
        revTime = revTryTime;
    }

    //? Can probably remove this, its used to check of the ant is in a certain nest, which can now be done with currentNest
    //returns true if this ant is nearer it's old nest than new
    public bool NearerOld()
    {
        return Vector3.Distance(transform.position, oldNest.transform.position) < Vector3.Distance(transform.position, myNest.transform.position);
    }

    // called once leader is 2*antennaReach away from follower
    public void TandemContactLost()
    {
        if (startTandemRunSeconds == 0)
        {
            return;
        }

        // log the time that the tandem run was lost
        timeWhenTandemLostContact = simulation.TickManager.TotalElapsedSimulatedSeconds;
        // calculate the Leader Give-Up Time (LGUT)
        CalculateLGUT();
    }

    // calculate the LGUT that the leader and follower will wait for a re-connection
    private void CalculateLGUT()
    {
        /* //? Greg commented out this block and used a simpler method (added in below)
        float tandemDuration = (simulation.TickManager.TotalElapsedSimulatedSeconds - startTandemRunSeconds);
        double exponent = 0.9651 + 0.3895 * Mathf.Log10(tandemDuration);
        tandemLeaderGiveUpTime = Mathf.Pow(10, (float)exponent);*/

        /*if (this.inNest)
        {
            this.leaderGiveUpTime = this.prevLeaderGiveUpTime - .1f;
        }
        else
        {
            this.leaderGiveUpTime = this.prevLeaderGiveUpTime + 0.1f;
        }*/
        leaderGiveUpTime += AntScales.Times.leaderGiveUpTimeIncrement;

        leaderGiveUpTime = Mathf.Min(leaderGiveUpTime, AntScales.Times.maxLeaderGiveUpTime);
    }

    // called once a follower has re-connected with the tandem leader (re-sets values)
    public void TandemContactRegained()
    {
        /*(prevLeaderGiveUpTime = leaderGiveUpTime;
        leaderGiveUpTime = 0.0f;*/ //?
        timeWhenTandemLostContact = 0;
    }

    // every time step this function is called from a searching follower
    public bool HasLGUTExpired()
    {
        if (leaderGiveUpTime == 0.0 || timeWhenTandemLostContact == 0) return false;

        float durationLostContact = (simulation.TickManager.TotalElapsedSimulatedSeconds - timeWhenTandemLostContact);

        // if duration since lost contact is longer than LGUT then tandem run has failed  
        return durationLostContact > leaderGiveUpTime;
    }

    // if duration of lost contact is greater than LGUT fail tandem run
    public void FailedTandemRun()
    {
        if (state == BehaviourState.Following && leader != null)
        {
            if (leader.state == BehaviourState.Recruiting || leader.state == BehaviourState.Reversing)
            {
                // failed tandem leader behaviour
                leader.FailedTandemLeaderBehvaiour();
                // failed tandem follower behaviour
                FailedTandemFollowerBehaviour();
            }
        }
    }

    // failed tandem leader behaviour
    private void FailedTandemLeaderBehvaiour()
    {
        startTandemRunSeconds = 0;
        
        /*//?// log failed tandem run in history
        if (forwardTandemRun == true)
        {
            forwardTandemRun = false;
        }
        else if (reverseTandemRun)
        {
            reverseTandemRun = false;
        }*/

        // reset tandem variables
        leaderWaits = false;
        followerWait = true;
        // turn off pheromones
        move.usePheromones = false; //? 
        follower = null;


        // behaviour after failed tandem run
        ChangeState(BehaviourState.Recruiting);
        //?failedTandemLeader = true;


        //? Not sure if this is necessary? can probably just set both failures to go to the new nest or smthn (i guess maybe it stops reverse runs being tryed over and over)
        if (previousState == BehaviourState.Reversing)
        {
            simulation.simData.failRTR++; //? tandem run success counters
            newToOld = true;
        }
        else
        {
            simulation.simData.failFTR++; //? tandem run success counters
            newToOld = false;
        }
    }

    // failed tandem follower behaviour
    private void FailedTandemFollowerBehaviour()
    {
        StopFollowing();
        ChangeState(previousState);

        //? changed this bit below, was caused weird behaviour
        /*// greg edit
        //TODO need to make accurate behaviour after
        if (myNest == oldNest)
        {
            ChangeState(BehaviourState.Scouting);
        }
        else
        {
            ChangeState(BehaviourState.Inactive);
        }*/
    }

    // Returns the position of this ant's antennae (the front of the 'capsule')
    public Vector3 antennaePosition()
    {
        return transform.position + transform.forward * (GetComponent<CapsuleCollider>().height / 2);
    }

    public void SetPrimaryColour(Color primary)
    {
        _primaryColour = primary;
        if (!_temporaryColour.HasValue)
            this.ChangeColour(_primaryColour);
    }

    public void ClearTemporaryColour()
    {
        _temporaryColour = null;
        this.ChangeColour(_primaryColour);
    }

    public void SetTemporaryColour(Color temporary)
    {
        _temporaryColour = temporary;
        //?_wasColourOn = false;
        //?_colourFlashTime = 0;
        this.ChangeColour(_temporaryColour.Value);
    }

    public void SimulationStarted()
    {

    }

    public void SimulationStopped()
    {

    }
}



