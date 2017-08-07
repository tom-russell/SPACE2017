using System.Collections.Generic;
using Assets.Scripts.Ants;
using Assets.Scripts.Nests;
using Assets.Scripts.Extensions;
using System.Linq;

public class SimulationData {

    private SimulationManager sim;
    NestInfo bestNest;

    // Emigration progress counters
    public int passivesInOldNest { get; private set; }
    public Dictionary<int, int> passivesInNewNests { get; set; }
    public float emigrationRelativeAccuracy { get; private set; }
    public float emigrationAbsoluteAccuracy { get; private set; }
    public float emigrationCompletion { get; private set; }

    // Tandem running counters (these are updated from AntManager when a tandem run terminates)
    public int successFTR { get; set; }
    public int successRTR { get; set; }
    public int failFTR { get; set; }
    public int failRTR { get; set; }

    // Emigration Times points (s)
    public int discoveryTime { get; private set; } 
    public int firstRecruiter { get; private set; }
    public int firstTandem { get; private set; }
    public int firstCarry { get; private set; }
    public int firstReverse { get; private set; }
    public int endOfEmigration { get; private set; }

    public void SimulationStart(SimulationManager simulationManager)
    {
        sim = simulationManager;
        passivesInNewNests = new Dictionary<int, int>();

        // Find and keep a reference to the best nest option 
        foreach (NestInfo nest in sim.NestInfo)
        {
            if (bestNest == null || nest.Nest.quality > bestNest.Nest.quality)
            {
                bestNest = nest;
            }
        }

        updateNestNumbers();
    }

    private void updateNestNumbers()
    {
        foreach (NestInfo nest in sim.NestInfo)
        {
            if (nest.IsStartingNest)
            {
                passivesInOldNest = nest.AntsPassive.transform.childCount;
            }
            else
            {
                passivesInNewNests.Add(nest.NestId, nest.AntsPassive.transform.childCount);
            }
        }
    }

    public void Tick()
    {
        updateEmigrationTimes();
        updateNestNumbers();
        int totalAnts = sim.Ants.Count;
        int emigratedAnts = passivesInNewNests.Sum(p => p.Value);
        int antsInBestNest = passivesInNewNests[sim.GetNestID(bestNest.Nest)];

        emigrationCompletion = (float)((totalAnts - passivesInOldNest) / totalAnts);
        emigrationRelativeAccuracy = antsInBestNest / emigratedAnts;
        emigrationAbsoluteAccuracy = antsInBestNest / totalAnts;
    }

    // Check if any new emigration time checkpoints have been reached
    private void updateEmigrationTimes()
    {
        foreach (AntManager ant in sim.Ants)
        {
            // Times are checked in the order they must occur in (e.g. recruiting cannot start until a nest is discovered
            if (discoveryTime == 0)
            {
                if (ant.state == BehaviourState.Assessing)
                {
                    discoveryTime = (int)sim.TotalElapsedSimulatedTime("s");
                }
            }
            else if (firstRecruiter == 0)
            {
                if (ant.state == BehaviourState.Recruiting)
                {
                    firstRecruiter = (int)sim.TotalElapsedSimulatedTime("s");
                }
            }
            else if (firstTandem == 0)
            {
                if (ant.follower != null)
                {
                    firstTandem = (int)sim.TotalElapsedSimulatedTime("s");
                }
            }
            else if (firstCarry == 0)
            {
                if (ant.isBeingCarried == true)
                {
                    firstCarry = (int)sim.TotalElapsedSimulatedTime("s");
                }
            }
            else if (firstReverse == 0)
            {
                if (ant.state == BehaviourState.Reversing && ant.follower != null)
                {
                    firstReverse = (int)sim.TotalElapsedSimulatedTime("s");
                }
            }
        }
    }

    public void SimulationFinish()
    {
        endOfEmigration = (int)sim.TotalElapsedSimulatedTime("s");
    }
}