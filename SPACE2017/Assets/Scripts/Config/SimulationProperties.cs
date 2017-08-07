using System;
using System.Collections.Generic;
using Assets.Scripts;

namespace Assets.Scripts.Config
{
    public class ExperimentName : SimulationStringProperty
    {
        public override string Name { get { return "Experiment Name"; } }

        public override string Description { get { return "The name of the experiment. Leave blank for an auto-generated one."; } }
    }

    public class RandomSeed : SimulationIntProperty
    {
        public override int? MaxValue { get { return null; } }

        public override int? MinValue { get { return null; } }

        public override string Name { get { return "Random Seed"; } }

        public override string Description { get { return "The seed number used to initialize the random number generator used by the simulation."; } }

        public RandomSeed()
        {
            Value = 0;
        }
    }

    public class SimulationEndPoint : SimulationListProperty
    {
        public override string Name { get { return "Simulation End Point"; } }

        public override string Description { get { return "The point at which the simulation will finish and no longer continue."; } }

        public SimulationEndPoint()
        {
            Options = new List<string>();
            Options.Clear();
            foreach (string point in Naming.SimulationEndPoints.points)
            {
                Options.Add(point);
            }

            Value = 0;
        }
    }

    public class ColonySize : SimulationIntProperty
    {
        public override int? MaxValue { get { return null; } }

        public override int? MinValue { get { return 0; } }

        public override string Name { get { return "Colony Size"; } }

        public override string Description { get { return "The starting number of ants in the first nest."; } }

        public ColonySize()
        {
            Value = 200;
        }
    }

    public class QuorumThreshold : SimulationIntProperty
    {
        public override int? MaxValue { get { return null; } }

        public override int? MinValue { get { return 0; } }

        public override string Name { get { return "Quorum Threshold"; } }

        public override string Description { get { return "The perceived number of ants required to reach a quorum."; } }

        public QuorumThreshold()
        {
            Value = 10;
        }
    }

    public class StartingTimeScale : SimulationIntProperty
    {
        public override int? MaxValue { get { return null; } }

        public override int? MinValue { get { return 0; } }

        public override string Name { get { return "Starting Time Scale"; } }

        public override string Description { get { return "The time scale (simulation speed) that the simulation will start in."; } }

        public StartingTimeScale()
        {
            Value = 1;
        }
    }

    public class RandomiseTimeScale : SimulationBoolProperty
    {
        public override string Name { get { return "Randomly fluctuating TimeScale"; } }

        public override string Description { get { return "If true, the TimeScale will be changed randomly throughout the simulation. (used for testing determinism)"; } }

        public RandomiseTimeScale()
        {
            Value = false;
        }
    }

    public class MaximumSimulationRunTime : SimulationIntProperty
    {
        public override int? MaxValue { get { return null; } }

        public override int? MinValue { get { return 0; } }

        public override string Name { get { return "Maximum Simulation Run Time (minutes)"; } }

        public override string Description { get { return "The maximum simulated minutes the simulation should be allowed to run for. Zero for no limit."; } }

        public MaximumSimulationRunTime()
        {
            Value = 0;
        }
    }


    public class ProportionActive : SimulationFloatProperty
    {
        public override float? MaxValue { get { return 1; } }

        public override float? MinValue { get { return 0; } }

        public override string Name { get { return "Proportion Active"; } }

        public override string Description { get { return "The proportion of ants that are scouting and active."; } }

        public ProportionActive()
        {
            // Approximately what is seen in the literature
            Value = 0.25f;
        }
    }

    public class StartingNestQuality : SimulationFloatProperty
    {
        public override float? MaxValue { get { return 1; } }

        public override float? MinValue { get { return 0; } }

        public override string Name { get { return "Starting Nest Quality"; } }

        public override string Description { get { return "The quality of the starting nest."; } }

        public StartingNestQuality()
        {
            Value = 0.0f;
        }
    }

    public class FirstNewNestQuality : SimulationFloatProperty
    {
        public override float? MaxValue { get { return 1; } }

        public override float? MinValue { get { return 0; } }

        public override string Name { get { return "First New Nest Quality"; } }

        public override string Description { get { return "The quality of the first new nest."; } }

        public FirstNewNestQuality()
        {
            Value = 0.3f;
        }
    }

    public class SecondNewNestQuality : SimulationFloatProperty
    {
        public override float? MaxValue { get { return 1; } }

        public override float? MinValue { get { return 0; } }

        public override string Name { get { return "Second New Nest Quality"; } }

        public override string Description { get { return "The quality of the second new nest."; } }

        public SecondNewNestQuality()
        {
            Value = 0.7f;
        }
    }

    public class AntsLayPheromones : SimulationBoolProperty
    {
        public override string Name { get { return "Ants Lay Pheromones"; } }

        public override string Description { get { return "Whether ants will lay pheromones."; } }

        public AntsLayPheromones()
        {
            Value = false;
        }
    }

    public class AntsReverseTandemRun : SimulationBoolProperty
    {
        public override string Name { get { return "Ants Reverse Tandem Run"; } }

        public override string Description { get { return "Whether ants will reverse tandem run after a social carry."; } }

        public AntsReverseTandemRun()
        {
            Value = true;
        }
    }

    public class OutputTickRate : SimulationIntProperty
    {
        public override int? MaxValue { get { return null; } }

        public override int? MinValue { get { return 1; } }

        public override string Name { get { return "Output Tick Rate"; } }

        public override string Description { get { return "How often (in ticks) to output simulation data. 50 ticks occur each second."; } }

        public OutputTickRate()
        {
            // Approximately every 10 seconds
            Value = 500;
        }
    }

    public class OutputAntDelta : SimulationBoolProperty
    {
        public override string Name { get { return "Output Ant Deltas"; } }

        public override string Description { get { return "Whether to output ant state and position information whenever their state changes."; } }

        public OutputAntDelta()
        {
            Value = false;
        }
    }

    public class OutputAntDetail : SimulationBoolProperty
    {
        public override string Name { get { return "Output Ant Detail"; } }

        public override string Description { get { return "Whether to output ant state and position information at set intervals."; } }

        public OutputAntDetail()
        {
            Value = false;
        }
    }

    public class OutputAntStateDistribution : SimulationBoolProperty
    {
        public override string Name { get { return "Output Ant State Distribution"; } }

        public override string Description { get { return "Whether to output the distributions of ants among states at set intervals."; } }

        public OutputAntStateDistribution()
        {
            Value = false;
        }
    }

    public class OutputColonyData : SimulationBoolProperty
    {
        public override string Name { get { return "Output Colony Data"; } }

        public override string Description { get { return "Whether to output the distributions of ants among nests at set intervals."; } }

        public OutputColonyData()
        {
            Value = false;
        }
    }

    public class OutputEmigrationData : SimulationBoolProperty
    {
        public override string Name { get { return "Output Emigration Data"; } }

        public override string Description { get { return "Whether to output information about the emigration at set intervals."; } }

        public OutputEmigrationData()
        {
            Value = false;
        }
    }

    public class OutputLegacyData : SimulationBoolProperty
    {
        public override string Name { get { return "Output Legacy Data"; } }

        public override string Description { get { return "Whether to output emigration data in the legacy format."; } }

        public OutputLegacyData()
        {
            Value = false;
        }
    }


    public class OutputAntDebug : SimulationBoolProperty
    {
        public override string Name { get { return "Output Ant Debug"; } }

        public override string Description { get { return "Whether to output ant debug information."; } }

        public OutputAntDebug()
        {
            Value = false;
        }
    }

    public class OutputEndSimData : SimulationBoolProperty
    {
        public override string Name { get { return "Output End of Simulation Data"; } }

        public override string Description { get { return "Whether to output the end of simulation data (nest allegiance and FTR/RTR totals)."; } }

        public OutputEndSimData()
        {
            Value = false;
        }
    }
}