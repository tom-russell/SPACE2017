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
        ReversingLeading,
        Leading,
        Carrying,
    };

    public enum NestAssessmentStage
    {
        Assessing,
        ReturningToHomeNest,
        ReturningToPotentialNest
    }

    public enum RecruitmentStage
    {
        GoingToOldNest,
        GoingToNewNest,
        WaitingInNewNest
    }
}
