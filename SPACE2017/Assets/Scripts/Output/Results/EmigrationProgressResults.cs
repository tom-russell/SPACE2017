using System.IO;

namespace Assets.Scripts.Output
{
    public class EmigrationProgressResults : FixedTickResults
    {
        public EmigrationProgressResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "emigration"))
        {
            WriteLine("Tick,Property,Value");
        }

        protected override void OutputData(long step)
        {
            WriteLine(string.Format("{0},{1},{2}", step, "PassivesInOldNest", Simulation.simulationData.passivesInOldNest));
            foreach (var o in Simulation.simulationData.passivesInNewNests)
            {
                WriteLine(string.Format("{0},{1},{2}", step, "PassivesInNewNest_" + o.Key, o.Value));
            }
            WriteLine(string.Format("{0},{1},{2}", step, "Completion", Simulation.simulationData.emigrationCompletion));
            WriteLine(string.Format("{0},{1},{2}", step, "RelativeAccuracy", Simulation.simulationData.emigrationRelativeAccuracy));
            WriteLine(string.Format("{0},{1},{2}", step, "AbsoluteAccuracy", Simulation.simulationData.emigrationAbsoluteAccuracy));
        }
    }
}
