using Assets.Scripts.Ants;
using System;

namespace Assets.Scripts.Extensions
{
    public static class StateExtensions
    {
        public static string StateName(this BehaviourState state)
        {
            switch (state)
            {
                case BehaviourState.Assessing:
                    return Naming.Ants.BehavourState.Assessing;
                case BehaviourState.Inactive:
                    return Naming.Ants.BehavourState.Inactive;
                case BehaviourState.Recruiting:
                    return Naming.Ants.BehavourState.Recruiting;
                default:
                    throw new ArgumentOutOfRangeException("state");
            }
        }
    }
}
