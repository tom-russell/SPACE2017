using System.Collections.Generic;

namespace Assets.Scripts.Ants
{
    // All speeds must be in centimetres per second (cm/s) and to 2s.f. (10mm/s = 1 unityunit/s)
    public static class Speed
    {
        public static Dictionary<string, float> v;

        static Speed()
        {
            v = new Dictionary<string, float>()
            {
                // Ant Speeds
                { Scouting, 0.84f },                                // speed of an active ant while not tandem running or carrying
                { TandemRunLead, 0.75f },                           // speed of an ant while leading a tandem run
                { TandemRunFollow, 0.5f },                          // speed of an ant while following a tandem run
                { Carrying, 0.54f },                                // speed of an ant while carrying another ant
                { Inactive, 0.2f },                                 // speed of an ant in the inactive state
                { AssessingFirstVisit, 0.34f },                     // speed of an ant in the assessing state (first visit)
                { AssessingSecondVisit, 0.41f },                    // speed of an ant during the assessing state (second visit)
                { ReverseWaiting, 0.84f },                          // speed of an ant waiting in the new nest to find a reverse tandem run follower
                //{ AssessingFirstVisitNonIntersecting, 3.2175f},   // speed of an ant in the assessing state (second visit) when not intersecting with pheromones
                //{ AssessingSecondVistIntersecting, 1.605f},       // speed of an ant in the assessing state (second visit) when intersecting with pheromones
                
                //Ant Pheromone Dropping Frequencies
                //{ PheromoneFrequencyFTR, 0.2664f},                // 1.8mm/s => 12.5f ! FTR 3 lay per 10mm => 12.5mm/s (speed) lay pheromone every 0.2664 secs
                //{ PheromoneFrequencyRTR, 0.8f},                   // 1.8mm/s => 12.5f ! FTR 1 lay per 10mm => 12.5mm/s (speed) lay pheromone every 0.8 secs
                //{ PheromoneFrequencyBuffon, 0.0376f}              // The frequency that pheromones are laid for buffon needle
            };
        }

        // Ant Speeds
        public const string Scouting = "Scouting";
        public const string TandemRunLead = "TandemRunLead";
        public const string TandemRunFollow = "TandemRunFollow";
        public const string Carrying = "Carrying";
        public const string Inactive = "Inactive";
        public const string AssessingFirstVisit = "AssessingFirstVisit";
        public const string AssessingSecondVisit = "AssessingSecondVisit";
        public const string ReverseWaiting = "ReverseWaiting";
        public const string AssessingFirstVisitNonIntersecting = "AssessingFirstVisitNonIntersecting";
        public const string AssessingSecondVistIntersecting = "AssessingSecondVistIntersecting";

        //Ant Pheromone Dropping Frequencies
        public const string PheromoneFrequencyFTR = "PheromoneFrequencyFTR";
        public const string PheromoneFrequencyRTR = "PheromoneFrequencyRTR";
        public const string PheromoneFrequencyBuffon = "PheromoneFrequencyBuffon";
    }

    // All times must be in seconds (s)
    public static class Times
    {
        public static Dictionary<string, float> v;

        static Times()
        {
            v = new Dictionary<string, float>()
            {
                { AverageAssessTimeFirstVisit, 142f },  // Average assessment time for an assessing ant on the first visit
                { AverageAssessTimeSecondVisit, 80f },  // Average assessment time for an assessing ant on the first visit
                { HalfIQRangeAssessTime, 40f },         // Half the interquartile range for the assessment times (used to normally distribute the assessment times)
                { MaxAssessmentWait, 45f },             // maximum wait between reassessments of the ants current nest when a !passive ant is in the Inactive state in a nest
                { ReverseTryTime, 5f },                 // No. of seconds a recruiter spends in their new nest trying to start a reverse tandem run.
                { RecruitTryTime, 15f },                // No. of seconds a recruiter spends in their old nest trying to start a forward tandem run or social carry
                { DroppedWait, 5f },                    // The time after which a dropped social carry can follow a reverse tandem run.
            };
    }

        public const string AverageAssessTimeFirstVisit = "AverageAssessTimeFirstVisit";    
        public const string AverageAssessTimeSecondVisit = "AverageAssessTimeSecondVisit";
        public const string HalfIQRangeAssessTime = "halfIQRangeAssessTime";
        public const string MaxAssessmentWait = "MaxAssessmentWait";
        public const string ReverseTryTime = "ReverseTryTime";
        public const string RecruitTryTime = "RecruitTryTime";
        public const string DroppedWait = "DroppedWait";
    }

    // All distances must be in centimetres (cm),  10mm = 1cm = 1 unityunit
    public static class Length
    {
        public static Dictionary<string, float> v;

        static Length()
        {
            v = new Dictionary<string, float>()
            {
                // Sensing range distances
                { DoorSenseRange, 1.5f },               // The distance at which an ant can 'sense' a nest door, and can walk into the nest towards the centre
                { SensesCollider, 0.5f },               // The radius of the ants sense sphere collider (minimum distance at which other ants can be sensed)
                //{ PheromoneSensing, 1f },             // Maximum distance that pheromones can be sensed from
                //{ AssessmentPheromoneSensing, 0.5f }, // one antenna length = 1mm. As pheromone range a radius around ant assessmentPheromoneRange = 0.5
                
                // Distances
                { LeaderStopping, 0.2f },               // Separation distance between a tandem run pair at which the leader stops to wait.
                { AssessingNestMiddle, 1f },            // The distance an assessor ant must be from the centre of a nest to trigger the switch to the next assessment stage
                { RecruitingNestMiddle, 1f },           // The distance a recruiter ant must be from the centre of the goal nest to trigger the switch to the next recruitment stage
                { Spawning, 0.2f },                     // The distance between the 'grid' of ants when they are instantiated at the start of a simulation

                // Ant body dimensions
                { AntennaeLength, 0.1f },               // The maximum antenna reach distance
                { BodyLength, 0.2f },                   // The length of the capsule that represents the ant's body (not including antennae).
                { BodyWidth, 0.05f }                    // The radius/width of the capsule that represents the ant's body.
            };
        }
        
        // Distances
        public const string DoorSenseRange = "DoorSenseRange";
        public const string PheromoneSensing = "PheromoneSensing";
        public const string AssessmentPheromoneSensing = "AssessmentPheromoneSensing";
        public const string LeaderStopping = "LeaderStopping";
        public const string AssessingNestMiddle = "AssessingNestMiddle";
        public const string RecruitingNestMiddle = "RecruitingNestMiddle";
        public const string Spawning = "Spawning";
        public const string SensesCollider = "SensesCollider";

        // Ant Dimensions
        public const string AntennaeLength = "AntennaeLength";
        public const string BodyLength = "BodyLength";
        public const string BodyWidth = "BodyWidth";
    }

    public static class Other
    {
        public static Dictionary<string, float> v;

        static Other()
        {
            v = new Dictionary<string, float>()
            {
                // Standard deviations/noise values
                { QuorumAssessNoise, 1f },      // The standard deviation of the normal distribution from which perceived nest quorum is drawn
                { AssessmentNoise, 0.1f },      // The standard deviation of the normal distribution from which perceived nest quality is drawn
                { TryTimeNoise, 2f },           // The standard deviation of the normal distribution from which recruiter waiting times are drawn
                { QualityThreshNoise, 0.2f },   // The standard deviation of the normal distribution from which each ants personal nest quality threshold is picked

                // Mean values
                { QualityThreshMean, 0.5f },    // The mean of the normal distibution from which this ants quality threshold for nests is picked

                // Probabilities    
                { TandRecSwitchProb, 0.3f },    // The probability that an ant that is recruiting via tandem (though not leading at this time) can be recruited by another ant
                { CarryRecSwitchProb, 0.1f },   // The probability that an ant that is recruiting via transports (though not at this time) can be recruited by another ant
                { RecAssessOldProb, 0.05f },    // The probability that a recruiter assesses its old nest when it enters it
                { RecAssessNewProb, 0.2f }      // The probability that a recruiter assesses its new/current nest when it enters it
            };
        }
        
        public const string QuorumAssessNoise = "QuorumAssessNoise";
        public const string AssessmentNoise = "AssessmentNoise";
        public const string TryTimeNoise = "TryTimeNoise";
        public const string QualityThreshNoise = "QualityThreshNoise";
        public const string QualityThreshMean = "QualityThreshMean";
        public const string TandRecSwitchProb = "TandRecSwitchProb";
        public const string CarryRecSwitchProb = "CarryRecSwitchProb";
        public const string RecAssessOldProb = "RecAssessOldProb";
        public const string RecAssessNewProb = "RecAssessNewProb";
    }
}
