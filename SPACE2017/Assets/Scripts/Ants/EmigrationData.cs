using System.Collections.Generic;
using Assets.Scripts.Ants;
using Assets.Scripts.Nests;
using Assets.Scripts.Extensions;
using System.Linq;

public class EmigrationData {

    private SimulationManager simulation;
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

    public EmigrationData(SimulationManager simulationManager)
    {
        simulation = simulationManager;
    }

    public void SimulationStarted()
    {
        passivesInNewNests = new Dictionary<int, int>();

        // Find and keep a reference to the best nest option 
        foreach (NestInfo nest in simulation.NestInfo)
        {
            if (bestNest == null || nest.Nest.quality > bestNest.Nest.quality)
            {
                bestNest = nest;
            }
            if (nest.IsStartingNest != true)
            {
                passivesInNewNests.Add(nest.NestId, 0);
            }
        }

        updateNestNumbers();
    }

    private void updateNestNumbers()
    {
        foreach (NestInfo nest in simulation.NestInfo)
        {
            if (nest.IsStartingNest)
            {
                passivesInOldNest = nest.AntsPassive.transform.childCount;
            }
            else
            {
                passivesInNewNests[nest.NestId] = nest.AntsPassive.transform.childCount;
            }
        }
    }

    public void Tick()
    {
        updateEmigrationTimes();
        updateNestNumbers();
        int totalAnts = simulation.Ants.Count;
        int emigratedAnts = passivesInNewNests.Sum(p => p.Value);
        int antsInBestNest = passivesInNewNests[simulation.GetNestID(bestNest.Nest)];

        emigrationCompletion = (totalAnts - passivesInOldNest) / (float)totalAnts;
        emigrationRelativeAccuracy = antsInBestNest / (float)emigratedAnts;
        emigrationAbsoluteAccuracy = antsInBestNest / (float)totalAnts;
    }

    // Check if any new emigration time checkpoints have been reached
    private void updateEmigrationTimes()
    {
        foreach (AntManager ant in simulation.Ants)
        {
            // Times are checked in the order they must occur in (e.g. recruiting cannot start until a nest is discovered
            if (discoveryTime == 0)
            {
                if (ant.state == BehaviourState.Assessing)
                {
                    discoveryTime = (int)simulation.TotalElapsedSimulatedTime("s");
                    return;
                }
            }
            else if (firstRecruiter == 0)
            {
                if (ant.state == BehaviourState.Recruiting)
                {
                    firstRecruiter = (int)simulation.TotalElapsedSimulatedTime("s");
                    return;
                }
            }
            else if (firstTandem == 0)
            {
                if (ant.IsTandemRunning())
                {
                    firstTandem = (int)simulation.TotalElapsedSimulatedTime("s");
                    return;
                }
            }
            else if (firstCarry == 0)
            {
                if (ant.IsTransporting())
                {
                    firstCarry = (int)simulation.TotalElapsedSimulatedTime("s");
                    return;
                }
            }
            else if (firstReverse == 0)
            {
                if (ant.state == BehaviourState.Reversing && ant.follower != null)
                {
                    firstReverse = (int)simulation.TotalElapsedSimulatedTime("s");
                    return;
                }
            }
        }
    }

    public void SimulationStopped()
    {
        endOfEmigration = (int)simulation.TotalElapsedSimulatedTime("s");
    }
}