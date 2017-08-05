namespace Assets.Scripts.Ants
{
    public enum BehaviourState
    {
        Inactive,
        Scouting,
        Assessing,
        Recruiting,
        Following,
        Reversing,
    };

    public enum NestAssessmentStage
    {
        Assessing,
        ReturningToHomeNest,
        ReturningToPotentialNest
    }

    public enum RecruitmentStage //? not currently used
    {
        GoingToOldNest,
        GoingToNewNest,
        WaitingInNewNest
    }
}
