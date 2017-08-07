namespace Assets.Scripts.Ants
{
    public static class AntScales
    {
        // All speed must be in centimetres per second (cm/s) and to 2s.f. (10mm/s = 1 unityunit/s)
        public static class Speeds
        {
            public const float Scouting = 0.84f;                // speed of an active ant while not tandem running or carrying
            public const float TandemRunLead = 0.75f;           // speed of an ant while tandem running
            public const float TandemRunFollow = 0.5f;          // speed of an ant while tandem running
            public const float Carrying = 0.54f;                // speed of an ant while carrying another ant
            public const float Inactive = 0.2f;                 // speed of an ant in the inactive state
            public const float AssessingFirstVisit = 0.34f;     // speed of an ant in the assessing state (first visit)
            public const float AssessingSecondVisit = 0.41f;    // speed of an ant during the assessing state (second visit)
            public const float ReverseWaiting = 0.84f;          // speed of an ant waiting in the new nest to find a reverse tandem run follower

            /*// non-intersection 4.06 mm/s, intersection 2.72mm/s 
            public const float AssessingFirstVisitNonIntersecting = 3.2175f; //speed of an ant in the assessing state (second visit) when not intersecting with pheromones
            public const float AssessingFirstVisitSecondVisitIntersecting = 1.605f; //speed of an ant in the assessing state (second visit) when intersecting with pheromones
            */

            public const float PheromoneFrequencyFTR = 0.2664f;     // 1.8mm/s => 12.5f ! FTR 3 lay per 10mm => 12.5mm/s (speed) lay pheromone every 0.2664 secs 
            public const float PheromoneFrequencyRTR = 0.8f;        // 1.8mm/s => 12.5f ! FTR 1 lay per 10mm => 12.5mm/s (speed) lay pheromone every 0.8 secs

            public const float PheromoneFrequencyBuffon = 0.0376f;  //the frequency that pheromones are laid for buffon needle
        }

        // All times must be in seconds (s)
        public static class Times
        {
            // Assessment duration times
            public const int averageAssessTimeFirstVisit = 142; //28
            public const int averageAssessTimeSecondVisit = 80; //16
            public const int halfIQRangeAssessTime = 40;       //7

            // Leader give up times
            public const int startingLeaderGiveUpTime = 10;
            public const int maxLeaderGiveUpTime = 20;
            public const float leaderGiveUpTimeIncrement = 0.1f;

            // Other times
            public const float maxAssessmentWait = 45f;     // maximum wait between reassessments of the ants current nest when a !passive ant is in the Inactive state in a nest
            public const int reverseTryTime = 5;            // No. of seconds a recruiter spends in their new nest trying to start a reverse tandem run.
            public const int recruitTryTime = 10;           // No. of seconds a recruiter spends in their old nest trying to start a forward tandem run.
            public const int droppedWait = 5;               // The time after which a dropped social carry can follow a reverse tandem run.
            public const int recTryTime = 15;               // The maximum time a recruiter will spend waiting in their old nest trying to start a forward tandem run.
        }

        // All distances must be in centimetres (cm),  10mm = 1cm = 1 unityunit
        public static class Distances
        {
            // Sensing range distances
            public const float DoorSenseRange = 1.5f;       // The distance at which an ant can 'sense' a nest door, and can walk into the nest towards the centre
            public const float PheromoneSensing = 1f;       // Maximum distance that pheromones can be sensed from

            // buffon needle
            // Eamonn B. Mallon and Nigel R. Franks - Ants estimate area using Buffon’s needle
            // The speeds at intersections were noted when an ant was within one antenna’s length of its first visit path.
            // one antenna lenght = 1mm. As pheromone range a radius around ant assessmentPheromoneRange = 0.5
            public const float AssessmentPheromoneSensing = .5f;

            // Ant body dimensions
            public const float AntennaeLength = 0.1f;       // The maximum antenna reach distance .
            public const float BodyLength = 0.2f;           // The length of the capsule that represents the ant's body (not including antennae).
            public const float BodyWidth = 0.05f;           // The radius/width of the capsule that represents the ant's body.

            public const float LeaderStopping = 0.2f;       // Separation distance between a tandem run pair at which the leader stops to wait.
            public const float AssessingNestMiddle = 1f;    // The distance an assessor ant must be from the centre of the goal nest to trigger the switch to the next assessment stage
            public const float RecruitingNestMiddle = 1f;
            public const float Spawning = 0.2f;
            public const float SensesCollider = 0.5f;       // The radius of the ants sense sphere collider (minimum distance at which other ants can be sensed)
        }

        // Values representing probabilities, standard deviations and means.
        public static class Other
        {
            // Standard deviations
            public const float quorumAssessNoise = 1f;      // The standard deviation of normal distribution with mean equal to the nests actual quorum from which perceived quorum is drawn
            public const float assessmentNoise = 0.1f;      // The standard deviation of normal distribution with mean equal to the nests actual quality from which perceived quality is drawn
            public const int tryTimeNoise = 2;              // The standard deviation of waiting times for recruiting ants waiting in nests for forward or reverse tandem runs

            // Means
            public const float qualityThreshMean = 0.5f;    // The mean of the normal distibution from which this ants quality threshold for nests is picked

            // Probablilities
            public const float tandRecSwitchProb = 0.3f;    // The probability that an ant that is recruiting via tandem (though not leading at this time) can be recruited by another ant
            public const float carryRecSwitchProb = 0.1f;   // The probability that an ant that is recruiting via transports (though not at this time) can be recruited by another ant
            public const float pRecAssessOld = 0.05f;        // The probability that a recruiter assesses its old nest when it enters it
            public const float pRecAssessNew = 0.2f;        // The probability that a recruiter assesses its new nest when it enters it

            public const float qualityThreshNoise = 0.2f;   // The stdev of the normal distibution from which this ants quality threshold for nests is picked
        }
    }
}
