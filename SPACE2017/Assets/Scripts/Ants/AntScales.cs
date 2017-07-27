namespace Assets.Scripts.Ants
{
    public static class AntScales
    {
        // All speed must be in millimeters per second (mm/s) and to 2s.f. >>>>>divided by 10 (10mm/s = 1unity unit per second
        public static class Speeds
        {
            public const float Scouting = 0.84f; //5f;                   // speed of an active ant while not tandem running or carrying
            public const float TandemRunLead = 0.75f; //4.5f;        // speed of an ant while tandem running
            public const float TandemRunFollow = 0.5f; //3f;        // speed of an ant while tandem running
            public const float Carrying = 0.54f; //3.75f;                // speed of an ant while carrying another ant
            public const float Inactive = 0.2f;                  // speed of an ant in the inactive state
            public const float AssessingFirstVisit = 0.34f;//2.6595f;   // speed of an ant in the assessing state (first visit)
            public const float AssessingSecondVisit = 0.41f;//3.2175f;  // speed of an ant during the assessing state (second visit)
            public const float ReverseWaiting = 0.84f;//4.5f;           // speed of an ant waiting in the new nest to find a reverse tandem run follower

            /*// non-intersection 4.06 mm/s, intersection 2.72mm/s 
            public const float AssessingFirstVisitNonIntersecting = 3.2175f; //speed of an ant in the assessing state (second visit) when not intersecting with pheromones
            public const float AssessingFirstVisitSecondVisitIntersecting = 1.605f; //speed of an ant in the assessing state (second visit) when intersecting with pheromones
            */

            public const float PheromoneFrequencyFTR = 0.2664f;   // 1.8mm/s => 12.5f ! FTR 3 lay per 10mm => 12.5mm/s (speed) lay pheromone every 0.2664 secs 
            public const float PheromoneFrequencyRTR = 0.8f;      // 1.8mm/s => 12.5f ! FTR 1 lay per 10mm => 12.5mm/s (speed) lay pheromone every 0.8 secs

            public const float PheromoneFrequencyBuffon = 0.0376f;        //the frequency that pheromones are laid for buffon needle
        }

        // All times must be in seconds (s)
        public static class Times
        {
            public const int averageAssessTimeFirstVisit = 142; //28
            public const int averageAssessTimeSecondVisit = 80; //16
            public const int halfIQRangeAssessTime = 40;       //7

            // Leader give up times
            public const int startingLeaderGiveUpTime = 10;
            public const int maxLeaderGiveUpTime = 20;
            public const float leaderGiveUpTimeIncrement = 0.1f;
        }

        // All distances must be in millimeters (mm), divided by 10? 1mm = 0.1 unity units
        public static class Distances
        {
            public const float DoorSenseRange = 1.5f;         // The distance at which an ant can 'sense' a nest door, and can walk into the nest towards the centre
            public const float PheromoneSensing = 1f;        //maximum distance that pheromones can be sensed from

            // buffon needle
            // Eamonn B. Mallon and Nigel R. Franks - Ants estimate area using Buffon’s needle
            // The speeds at intersections were noted when an ant was within one antenna’s length of its first visit path.
            // one antenna lenght = 1mm. As pheromone range a radius around ant assessmentPheromoneRange = 0.5
            public const float AssessmentPheromoneSensing = .5f;

            // Ant body dimensions
            public const float AntennaeLength = 0.1f;   // The maximum antenna reach distance .
            public const float BodyLength = 0.2f;       // The length of the capsule that represents the ant's body (not including antennae).
            public const float BodyWidth = 0.05f;       // The radius/width of the capsule that represents the ant's body.

            public const float LeaderStopping = 0.2f; // Separation distance between a tandem run pair at which the leader stops to wait.
            //public const float TandemFollowerLagging = 0.95f; //? I haven't reimplemented this, not sure if its important // The distance from the leader causing follower to change direction 
            //public const float AssessingDoor = 1.5f;
            public const float AssessingNestMiddle = 1f;    // The distance an assessor ant must be from the centre of the goal nest to trigger the switch to the next assessment stage
            public const float RecruitingNestMiddle = 1f;
            public const float Spawning = 0.2f;
            public const float SensesCollider = 0.5f; //? This was incorrectly set to 3
        }
    }
}
