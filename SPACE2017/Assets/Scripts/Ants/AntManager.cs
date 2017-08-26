using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Extensions;
using Assets.Scripts.Ants;
using System;

public class AntManager : MonoBehaviour
{
    // Unity Object and Script References
    public SimulationManager simulation;    // The simulation manager for the current simulation
    private AntMovement move;               // Movement controller for this ant.
    private Collider sensesCol;             // The sphere collider used to sense other ants.
    private Transform carryPosition;        // The child object of this ant that is used to carry another ant.

    // General Ant Variables
    public int AntId { get; set; }          // The unique ID number of this ant.
    public BehaviourState state;            // Which state this ant is currently in.
    private BehaviourState previousState;   // The state which this ant was in prior to assessing
    public bool passive;                    // If this ant is passive or not (passive ants do not perform any actions and can only be carried between nests).
    public NestManager myNest;              // The nest this ant currently has allegiance to.
    public NestManager oldNest;             // The nest this ant previously has allegiance to.
    public NestManager nestToAssess;        // Nest that this ant is currently assessing.
    public NestManager currentNest;         // Nest that this ant is currently inside.
    public AntManager leader, follower;     // The ants that are leading or following this ant
    public bool inNest;                     // True when this ant is inside a nest.
    public int droppedRecently;             // Timer to show if this ant has been recently dropped or not. -1 = never been carried, 0 = was dropped > 5sec ago.
    public int PerceivedTicks { get; set; } // Used for the Debug Results Output 

    // Assessment Variables
    public float nextAssessment;                // When the next assessment of the nest that this ant is will be carried out
    private float perceivedQuality;             // The quality that this ant perceives this.myNest to be
    private float nestThreshold;                // This individual's threshold for nest quality
    private int assessTime;                     // The time remaining for this ants current assessment of a nest.
    public NestAssessmentStage assessmentStage; // The stage of the assessment process this ant is currently in.
    public int nestAssessmentVisitNumber;       //? ideally add one more nestassessmentstage and do away with this // 1 or 2 depending on how many times the ant has visited the nest to assess it
    private bool comparisonAssess = true;       //? This should probably be a setting instead // When true this allows the ant to compare new nests it encounters to the nest it currently has allegiance to.

    // Recruitment and Tandem Run Variables
    public RecruitmentStage recruitmentStage;   // The current direction of movement/waiting location of the recruiting ant
    private bool finishedRecruiting;            // True when this ant has finished recruiting and is returning to its nest
    private float perceivedQuorum;              // The quorum that this ant perceives this.myNest to have
    public int quorumThreshold;                 // Quorum threshold where recruiting ant carries rather than tandem runs
    private int reverseTime;                    // The countdown for recruiters waiting in their new home nest attempting to start a reverse tandem run (after quorum is reached).
    public int waitOldNestTime;                 // The countdown for recruiters waiting in their old nest trying to start a forward tandem run/social carry.
    private int waitNewNestTime;                // The countdown for recruiters waiting in their new home nest (before quorum is reached).
    public bool followerWait = true;
    public bool leaderWaits = false;
    private float timeWhenTandemLostContact = 0f;
    private float leaderGiveUpTime;
    public Vector3 estimateNewLeaderPos = Vector3.zero;
    // Tandem Run Output Variables
    private float tandemStartTime;              // The start time (in ms) of the tandem run
    public float tandemDistance;                // The distance moved by the leader during the tandem run
    private bool forwardRun;                    // True if the current tandem run is forward, and false if reverse
    public Vector3 prevLeaderPosition;          // The location of the leader during the previous timestep. Used to calculate tandem run distance.

    // Other
    //?private bool _wasColourOn = false;
    //?private float _colourFlashTime = 0;
    private Color? _temporaryColour;
    private Color _primaryColour;
    public bool DEBUG_ANT = false; // Used for debugging

    // Use this for initialization
    void Start()
    {
        oldNest = GameObject.Find(Naming.World.InitialNest).NestManager();
        carryPosition = transform.Find(Naming.Ants.CarryPosition);
        sensesCol = transform.Find(Naming.Ants.SensesArea).GetComponent<Collider>();
        move = gameObject.AntMovement();
        nestThreshold = RandomGenerator.Instance.NormalRandom(Other.v[Other.QualityThreshMean], Other.v[Other.QualityThreshNoise]);
        perceivedQuality = float.MinValue;
        finishedRecruiting = false;
        //make sure the value is within contraints
        if (nestThreshold > 1)
            nestThreshold = 1;
        else if (nestThreshold < 0)
            nestThreshold = 0;
    }

    public void Tick()
    {
        PerceivedTicks++;

        // Decrement Counters only if 1 second of simulated time has elapsed
        if (Mathf.Approximately(simulation.TotalElapsedSimulatedTime("ms") % 1000, 0))
        {
            DecrementCounters();
        }

        /* //? This part has been added new. Some may be useful (recruitmentStage)
        if (state == BehaviourState.Recruiting && recruitmentStage == RecruitmentStage.WaitingInNewNest)
        {
            //CheckQuorum(myNest); //?

            // Check if ant is giving up recruiting
            if (simulation.TickManager.TotalElapsedSimulatedSeconds - _recruitmentWaitStartSeconds >= AntScales.Times.RecruiterWaitSeconds)
            {
                recruitmentStage = RecruitmentStage.GoingToOldNest;
            }
        }*/

        /*//? Test removing this, I changed the triggerExit code and nest wall thickness & movement code so hopefully it's fixed
        //BUGFIX: sometimes assessors leave nest without triggering OnExit in NestManager
        if (state == BehaviourState.Assessing && Vector3.Distance(nestToAssess.transform.position, transform.position) >
           Mathf.Sqrt(Mathf.Pow(nestToAssess.transform.localScale.x, 2) + Mathf.Pow(nestToAssess.transform.localScale.z, 2)))
            LeftNest();*/
        
        //BUGFIX: occasionally when followers enter a nest there EnteredNest function doesn't get called, this forces that
        if (state == BehaviourState.Following && Vector3.Distance(LeadersNest().transform.position, transform.position) < LeadersNest().transform.localScale.x / 2f)
        {
            EnteredNest(LeadersNest().NestManager());
        }

        //makes Inactive and !passive ants assess nest that they are in every so often
        if (!passive && state == BehaviourState.Inactive && nextAssessment > 0 && simulation.TotalElapsedSimulatedTime("s") >= nextAssessment)
        {
            AssessNest(myNest);
            nextAssessment = simulation.TotalElapsedSimulatedTime("s") + RandomGenerator.Instance.Range(0.5f, 1f) * Times.v[Times.MaxAssessmentWait];
        }

        //if an ant is carrying another and is within x distance of their nest's centre then drop the ant
        if (carryPosition.childCount > 0 && Vector3.Distance(myNest.transform.position, transform.position) < Length.v[Length.RecruitingNestMiddle])
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

            // If RTRs are enabled attempt to find an ant to lead, else just continue to recruit as normal
            if (simulation.Settings.AntsReverseTandemRun.Value)
            {
                Reverse(myNest);
            }
            else
            {
                RecruitToNest(myNest);
            }
                
        }

        //BUGFIX: Sometimes new to old is incorrectly set for recruiters - unclear why as of yet.
        if (state == BehaviourState.Recruiting && follower != null && currentNest == oldNest)
        {
            recruitmentStage = RecruitmentStage.GoingToNewNest;
        }

        move.Tick(); //? Moved this to the end
    }

    private void DecrementCounters()
    {
        if (state == BehaviourState.Recruiting)
        {
            if (currentNest == oldNest && recruitmentStage == RecruitmentStage.GoingToOldNest)
            {
                if (waitOldNestTime > 0)
                {
                    waitOldNestTime -= 1;
                }
                else
                {
                    recruitmentStage = RecruitmentStage.GoingToNewNest;
                }
            }
            else if (currentNest == myNest && recruitmentStage == RecruitmentStage.GoingToNewNest)
            {
                if (waitNewNestTime > 0)
                {
                    waitNewNestTime -= 1;
                }
                else
                {
                    RecruitToNest(myNest);
                }
            }
        }

        //Only try reverse tandem runs for a certain amount of time
        if (state == BehaviourState.Reversing && currentNest == myNest && follower == null)
        {
            if (reverseTime < 1)
            {
                RecruitToNest(myNest);
            }
            else
            {
                reverseTime -= 1;
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
        }
        if (colour.HasValue)
        {
            SetPrimaryColour(colour.Value);
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

    // Tell this ant to lead 'follower' to preferred nest
    public void Lead(AntManager follower)
    {
        // Reset the leader giving up time
        leaderGiveUpTime = 2;   // 2 is roughly the lowest LGUT possible (LGUT when tandem duration is 0s cannot be calculated since Log(0) = -inf

        // Set the tandem variables (for recording tandem run data) 
        tandemStartTime = simulation.TotalElapsedSimulatedTime("s");
        tandemDistance = 0f;
        forwardRun = true;
        prevLeaderPosition = transform.position;

        //let following ant know that you're leading it
        this.follower = follower;
        this.follower.Follow(this);
        recruitmentStage = RecruitmentStage.GoingToNewNest;
    }

    public void ReverseLead(AntManager follower)
    {
        // Reset the leader giving up time
        leaderGiveUpTime = 2;   // 2 is roughly the lowest LGUT possible (LGUT when tandem duration is 0s cannot be calculated since Log(0) = -inf

        // Set the tandem variables (for recording tandem run data) 
        tandemStartTime = simulation.TotalElapsedSimulatedTime("s");
        tandemDistance = 0f;
        forwardRun = false; // Reverse run
        prevLeaderPosition = transform.position;

        //let following ant know that you're leading it
        this.follower = follower;
        this.follower.Follow(this);
        recruitmentStage = RecruitmentStage.GoingToOldNest;
    }

    public void StopLeading()
    {
        followerWait = true;
        leaderWaits = false;
        follower = null;

        AddTandemRunRecord(success : true);
        
        // Reversing Leaders must return to the recruiting state after the reverse run is completed
        if (state == BehaviourState.Reversing) 
        {
            RecruitToNest(myNest);
        }
    }

    // Add the record of this tandem run to the emigration data
    private void AddTandemRunRecord(bool success)
    {
        float runDuration = simulation.TotalElapsedSimulatedTime("s") - tandemStartTime;
        EmigrationData.TandemRunData runData = new EmigrationData.TandemRunData(forwardRun, success, runDuration, tandemDistance);
        simulation.emigrationData.tandemRunData.Add(runData);
        if (DEBUG_ANT == true) Debug.Log("duration=" + runDuration + "  distance=" + tandemDistance); //?
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
        //start following leader towards nest
        ChangeState(BehaviourState.Following);
        this.leader = leader;

        followerWait = true;
    }

    public void StopFollowing()
    {
        followerWait = true;
        leaderWaits = false;
        estimateNewLeaderPos = Vector3.zero;
        leader = null;
    }

    //makes this ant pick up 'otherAnt' and carry them back to preffered nest
    public void PickUp(AntManager otherAnt)
    {
        otherAnt.PickedUp(transform);
        recruitmentStage = RecruitmentStage.GoingToNewNest;
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
        transform.position = new Vector3(transform.position.x, GetComponent<CapsuleCollider>().radius * 2, transform.position.z);
        move.isBeingCarried = false;

        if (transform.parent.tag == Naming.Ants.CarryPosition)
        {
            int id = simulation.GetNestID(nest);
            transform.parent = GameObject.Find("P" + id).transform;
        }

        //make ant inactive in this nest
        // oldNest = nest; //? This is commented out in Gregs version
        myNest = nest;
        droppedRecently = (int)Times.v[Times.DroppedWait];
        //? commented out in both versions?
        nextAssessment = simulation.TotalElapsedSimulatedTime("s") + RandomGenerator.Instance.Range(0.5f, 1f) * Times.v[Times.MaxAssessmentWait];
        ChangeState(BehaviourState.Inactive);

        //turns senses on if non passive ant
        if (passive == false)
        {
            sensesCol.enabled = true;
        }
    }

    //this is called whenever an ant enters a nest
    public void EnteredNest(NestManager nest)
    {
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

        // Behaviour for recruiters entering their new nest
        if (state == BehaviourState.Recruiting && nest == myNest)
        {
            if (finishedRecruiting == true)
            {
                ChangeState(BehaviourState.Inactive);
                finishedRecruiting = false;
                droppedRecently = 0;    // Allows this ant to be reverse lead if the emigration is still continuing (this ant was recruited from the other potential nest)
                return;
            }
            else
            {
                // If the quorum is not yet reached, there is a chance that a recruiter will reassess their new nest
                if (follower == null && RandomGenerator.Instance.Range(0f, 1f) < Other.v[Other.RecAssessNewProb] && !IsQuorumReached())
                {
                    nestToAssess = nest;
                    ChangeState(BehaviourState.Assessing);
                }
                else if (waitNewNestTime == 0) // If the recruiter needs to now wait in the new nest, set the wait counter
                {
                    waitNewNestTime = Mathf.RoundToInt(quorumThreshold * simulation.Settings.WaitNewNestFactor.Value);  // Recruiters wait in the new nest for time dependent on the quorum threshold
                }
            }
        }

        if (state == BehaviourState.Recruiting && nest == oldNest)
        {
            //if no passive ants left in old nest then turn around and return home
            if (finishedRecruiting == true || nest.GetPassive() == 0)
            {
                recruitmentStage = RecruitmentStage.GoingToNewNest;
                finishedRecruiting = true;
                return;
            }
            //if recruiting and this is old nest then assess with probability pRecAssessOld
            else if (follower == null && RandomGenerator.Instance.Range(0f, 1f) < Other.v[Other.RecAssessOldProb])
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
                recruitmentStage = RecruitmentStage.GoingToNewNest;
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
            /* //? Don't think this is needed
            //if recruiting and this is your nest then go back to looking around for ants to recruit 
            if (state == BehaviourState.Recruiting && nest == myNest && follower == null)
            {
                RecruitToNest(myNest);
            }*/
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
        //reset current nest area
        //?currentNestArea = 0f;

        // Nest quality measurement (not buffon's needle, random value from normal distribution, constrained between 0-1)
        float q = RandomGenerator.Instance.NormalRandom(nest.quality, Other.v[Other.AssessmentNoise]);
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
    }

    private int GetAssessTime()
    {
        // Eamonn B. Mallon and Nigel R. Franks - Ants estimate area using Buffon's needle
        // The median time that a scout spends within a nest cavity assessing a potential nest is 110 s per visit (interquartile range 140 s and n = 115)
        // range must be +/- 55. As 125+55=180 (110+70). And 95-55=40 (110-70)

        int averageAssessTime;
        if (nestAssessmentVisitNumber == 1)
        {
            averageAssessTime = (int)Times.v[Times.AverageAssessTimeFirstVisit];
        }
        else
        {
            averageAssessTime = (int)Times.v[Times.AverageAssessTimeSecondVisit];
        }

        float deviate = (float) RandomGenerator.Instance.UniformDeviate(-1, 1);
        int duration = (averageAssessTime + (int)(Times.v[Times.HalfIQRangeAssessTime] * deviate));

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
        recruitmentStage = RecruitmentStage.GoingToOldNest;
        myNest = nest;
        CheckQuorum(nest);

        waitOldNestTime = (int)Times.v[Times.RecruitTryTime];

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
            perceivedQuorum = RandomGenerator.Instance.NormalRandom(nest.GetQuorum(), Other.v[Other.QuorumAssessNoise]);
        }
    }

    private void Reverse(NestManager nest)
    {
        ChangeState(BehaviourState.Reversing);
        reverseTime = (int)Times.v[Times.ReverseTryTime];
    }

    // called once leader is 2*antennaReach away from follower
    public void TandemContactLost()
    {
        // log the time that the tandem run was lost
        timeWhenTandemLostContact = simulation.TotalElapsedSimulatedTime("s");
        // calculate the Leader Give-Up Time (LGUT)
        CalculateLGUT();
    }

    // calculate the LGUT that the leader and follower will wait for a re-connection
    private void CalculateLGUT()
    {
        float tandemDuration = simulation.TotalElapsedSimulatedTime("s") - tandemStartTime;
        double exponent = 0.9651 + 0.3895 * Mathf.Log10(tandemDuration);
        leaderGiveUpTime = Mathf.Pow(10, (float)exponent);

        /*leaderGiveUpTime += AntScales.Times.leaderGiveUpTimeIncrement;
        leaderGiveUpTime = Mathf.Min(leaderGiveUpTime, AntScales.Times.maxLeaderGiveUpTime);*/
    }

    // called once a follower has re-connected with the tandem leader (re-sets values)
    public void TandemContactRegained()
    {
        timeWhenTandemLostContact = 0;
    }

    // every time step this function is called from a searching follower
    public bool HasLGUTExpired()
    {
        if (timeWhenTandemLostContact == 0) return false;

        float durationLostContact = simulation.TotalElapsedSimulatedTime("s") - timeWhenTandemLostContact;

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
                leader.FailedTandemLeaderBehaviour();
                // failed tandem follower behaviour
                FailedTandemFollowerBehaviour();
            }
        }
    }

    // failed tandem leader behaviour
    private void FailedTandemLeaderBehaviour()
    {
        // reset tandem variables
        leaderWaits = false;
        followerWait = true;
        follower = null;

        // behaviour after failed tandem run
        ChangeState(BehaviourState.Recruiting);

        //? After a failed run the leader continues in the same direction
        if (previousState == BehaviourState.Reversing) // failed reverse run - head to old nest
        {
            AddTandemRunRecord(success: false);
            recruitmentStage = RecruitmentStage.GoingToOldNest;
        }
        else // failed forward run - head to new nest
        {
            AddTandemRunRecord(success: false);
            recruitmentStage = RecruitmentStage.GoingToNewNest;
        }
    }

    // failed tandem follower behaviour
    private void FailedTandemFollowerBehaviour()
    {
        StopFollowing();
        ChangeState(previousState);
    }

    public void SetPrimaryColour(Color primary)
    {
        _primaryColour = primary;
        if (!_temporaryColour.HasValue)
        {
            this.ChangeColour(_primaryColour);
        }
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
}