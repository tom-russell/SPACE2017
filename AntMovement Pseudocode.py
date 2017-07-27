# # # # General Notes # # # #
# AntManager - some states seem aren't used any more and still remain in Andy's version (Carrying, Leading, reversingLeading).
# Turns seem to occur in too many different places, could be cleaned up (FaceObject, Turn, ChangeDirection, ObstructionCheck).
# assessment stages don't match Greg's thesis but this is likely due to Buffon's needle being current disabled for testing (by Greg & Martin). 
    # For now use Martin's assessing code, add Greg's later as an optional toggle (along with disabled inactive movement & pheromone usage)
# Andy moved speed values into the Scales file
    
    
function Update()
{
    if CharacterController is disabled : exit; # This is used for Social Carrying (disabling movement)
    if state == PASSIVE : exit;

    # ASSESSING ANTS
    # ensures assessor ants stays within nest cavity
    # if outside of nest change direction more frequently
    if state == ASSESSING && assessmentStage == 0 : 
        if "ant not in nest" : ChangeDirection();
        else "only every 3 frames" : ChangeDirection();
    
    
    # RECRUITING LEADER & FOLLOWER ANTS
    # also sets speeds to values
    if "ant has a leader (is a follower)" : 
        if "distance between ant and leader > 9.5" : ChangeDirection();
        if "ant has touched leader" : exit;
        
    if "ant has a follower (is a leader)" :
        if "leader should wait": exit;
    
    # Move the ant forwards
    Move();
    
    if "ant not in nest" && (state == SCOUT or INACTIVE): # scouts can sense doors within a certain range & inactive ants must return if they accidentally leave the nest
        if "door is in range" : FaceObject(door);
        else : Turn()
        
    # wait required time till next direction change
    if "not yet time for direction change" : exit;
    else ChangeDirection();
}

function Move()
{
    ObstructionCheck(); # if there is an obstruction turn to avoid
    
    MoveForward @ "speed for current state";
}

function ChangeDirection() 
{
    if state == FOLLOWER : maxVar = 20;     # maxVar is the standard deviation (of a normal distribution) when determining a new angle
    else                 : maxVar = 40;
    
    if state == SCOUT      : ScoutingDirectionChange();
    if state == FOLLOWER   : FollowingDirectionChange();
    if state == INACTIVE   : InactiveDirectionChange();
    if state == RECRUITING : RecruitingDirectionChange();
    if state == REVERSING  : ReversingDirectionChange();
    if state == ASSESSING  : AssessingDirectionChange();
    
    Turned();
}

function ScoutingDirectionChange() 
{
    if "Nest door is within sensing range" : WalkToGameObject(door);
    else RandomWalk();
}

function FollowingDirectionChange() 
{
    if "Follower ant has no leader" : "Face direction of own nest";     #? I assume this is when a tandem run fails
    #? Lots of commented code here
    else if "Follower can't see leader" :
        if "First movement of run (estimated location not set)" : estimateNewLeaderPos = leader.position;
        predictedLeaderAngle = "calculate angle towards predicted leader location";
        newDirection = normallyDistributedRandom(mean=predictedLeaderAngle, std=maxVar);
        Turn(newDirection);
    else : 
        "If follower can see leader, turn to face the leader";
}

# This function creates a swarming of inactive ants around the center of their nest
function InactiveDirectionChange() 
{
    WalkToGameObject(CenterOfNest);
}

function RecruitingDirectionChange() 
{
    if "recruiter returning to old nest" && "old nest empty" : RandomWalk();
    else : WalkToGameObject(NextWaypoint);
}

function ReversingDirectionChange() 
{
    # This function is exactly the same as RecruitingDirectionChange()
}

function AssessingDirectionChange()     #? why is some of the assessment code in update()?
{
    if assessmentStage == 1 :
        WalkToGameObject(oldNest);
        if "distance between ant and oldNest" < 20 : assessmentStage = 2;   #? This seems like it should belong in AntManager instead?
        return;
    else if assessmentStage == 2 :
        WalkToGameObject(nestToAssess);
        if "distance between ant and new nest" < 40 && "ant is in (new?) nest" :
            nestAssessmentSecondVisit();   # updates variables, such as aStage = 0;
        return;
        
        
    if assessmentVisitNumber == 1 || 2 :    # this section again had repeated code
        "update total assessing distance (add distance between current and last positions";
        lastPosition = currentPosition;
        
    if assessTime > 0 : # assessTime is a count down, initially set to the number of seconds required for this visit
        if "ant is in nest" : RandomWalk();
        else : WalkToGameObject(nestToAssess);  # I assume this is if an assessor wanders out of the nest
    else : # assessment is over
        WalkToGameObject(NextWaypoint());
}

fucntion NewDirectionCheck(newDirection)
{
    "set newDirection to be within range 0 <= x <= 360";
    "check forwards/backwards/left/right for a wall in close proximity";
    if "newDirection will cause a wall collision" :
        return previousDirection;
    else return newDirection;
}

# This function needs lots of change, >100 lines long and covers cases for almost every state
function NextWaypoint() 
{
    # unclear brackets .
    if "ant doesn't have an old nest" or "ant is nearer the currentNest" :
        nearerOld = false;
    else nearerOld = true;
    
    # Passive and inactive ants always move towards center of their current nest #
    if state == PASSIVE or INACTIVE :
        return currentNest;
        
    # Assessing ant either needs to move to the new next or back to the old nest (when assessment is complete)
    if state == ASSESSING :
        # remember - assessTime starts when ant assesses new nest and counts down to 0
        if assessTime > 0 : return nestToAssess; # this is called if the assessor wanders out of the nest
        else : return nestToAssess.door; #? return trip to new nest perhaps?
        
    # reversing ants #
    if state == REVERSING :
        if "ant is in new nest" : # requires two checks (inNest && !nearerOld)
            if "ant is not a leader or follower" : # comment here is a little confusing. "If not carrying" but the if return true if the ant is not in a tandem pair
                #again strange comment: "find an ant to carry" but returns to the newNest. surely must mean RTR rather than carry
                return ant.newNest # newNest = recruit to this nest.
            else : # ant is part of a RTR pair
                if newNest.door != null : return newNest.door; #? not sure about this one. NestManager.door never seems to be set to anything.....
                else : return newNest.center;
        else : # ant is not in new nest
            
            if "ant is not in new nest" : # this part seems redundant?
                # go to old nest
                if oldNest.door != null : return oldNest.door;
                else : return oldNest.center;;
            else : return oldNest.center;;  # if already in the required location go to nest center
            
    if "ant is in a nest" and "is heading towards new nest" :
        if nearerOld == true : # ant is in old nest
            if oldNest.door is not null : return oldNest.door;   #see above, door never seems to be set
            else : return newNest.center;
    
    else if "ant is in a nest" and "heading towards old nest" : 
        if "ant is in new nest" :
            if newNest.door is not null : return newNest.door; # head towards nest exit (ant wants to leave this nest)
            else return newNest.center;
        else : return oldNest.center;; # if already in the required location go to nest center
    
    else if "ant not in a nest" and "heading towards new nest" :
        if newNest.door is not null : return newNest.door;
        else return newNest.center;
    
    else : # not in a nest and heading towards old nest
        if oldNest.door is not null : return oldNest.door;
        else return oldNest.center;
}

# This function is called after a turn to reset turn time and distance counters
function Turned()
{
    if "ant has moved" : 
        "calculate next direction change time";
        if state == ASSESSING : "time till next direction change is doubled";
    else : # ant is stuck, so change direction faster to try and get unstuck
        "time till next direction change is 10x less";
}

# slightly more complex angle calculations
function WalkToGameObject(GameObject)
{
    # This is the direction angle towards the specified gameObject
    goalAngle = GetAngleBetweenPositions(currentPosition, GameObject.position);
    
    if "difference between goalAngle & currentAngle" > 180 :    #? not 100% sure of the purpose of this block
        currentAngle -= 360; # this gets angle between -180 & 180?
        
    newDir = normalRandom(mean = "midpoint between goalAngle & currentAngle", stdev = maxVar);
    Turn(newDir);
}

function RandomWalk() 
{
    maxVar; # (angle is selected as random value from normal distribution with stdev = maxVar)
    if state == ASSESSING : maxVar = 10;
    if "ant has not moved" : maxVar = 180; # I assume this is so ants don't take forever to turn when hitting walls
    
    newDirection = currentDirection + normalRandom(0, maxVar);
    Turn(newDirection);
}

function Turn(newDirection)
{
    "rotate ant to face direction" : NewDirectionCheck(newDirection);
}

function ObstructionCheck()
{
    if "raycast hits target" :
        if "target is an ant" && "this.ant is not following a tandem run" : RandomRotate();
        else : FollowWall();
        
        Turned();
        return true;
        
    else : return false;
}

function FollowWall()
{
    # simple maths but long algorithm. weighting system to favour directions along walls, detailed in greg's thesis.
}



# Helper functions #
function hasFollowerTouchedLeader();            # return true of follower caught up to leader. Also checks what movement the follower should take
function tandemRegainedContact();               # called when follower finds tandem leader. Resets the required variables etc.
function estimateNextLocationOfLeader();        # when tandemRegainedContact, follower estimates where the leader will have moved to
function shouldTandemLeaderWait();              # updates whether leader should wait during a tandem run.
function distanceBetweenLeaderAndFollower();    # seems redundant --- just 2 lines of code
function tandemLostContact();
function FaceObject();                          # seems redundant --- just 2 lines of code. 
function DoorCheck();                           # if any nest door is within doorSenseRange, return the Door GameObject (else return null)
function normalRandom();                        # returns a random (normally distributed) value around given mean with given stdev
function getAngleBetweenPositions();            # returns the angle between two positional vectors from v1 to v2
function square();                              # returns square of input
function RandomRotate();                        # Used to avoid obstructions. Turn ant randomly (normally distributed) with maxVar of 180
function ObstructionCheck();                    #

# These 3 functions are used for disabling movement during social carrying #
function Enable();      
function Disable();     
function isEnabled();   

# Pheromone Functions #
function PheromoneDirection();
function LayPheromoneScouting();
function LayPheromoneFTR();
function LayPheromoneRTR();
function LayPheromoneAssessing();
function PheromonesInRange();
function AssessmentPheromonesInRange();












#? is reversing state just when part of a tandem pair?