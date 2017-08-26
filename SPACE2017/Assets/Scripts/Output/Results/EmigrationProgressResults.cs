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
            WriteLine(string.Format("{0},{1},{2}", step, "PassivesInOldNest", Simulation.emigrationData.passivesInOldNest));
            foreach (var o in Simulation.emigrationData.passivesInNewNests)
            {
                WriteLine(string.Format("{0},{1},{2}", step, "PassivesInNewNest_" + o.Key, o.Value));
            }
            WriteLine(string.Format("{0},{1},{2}", step, "Completion", Simulation.emigrationData.emigrationCompletion));
            WriteLine(string.Format("{0},{1},{2}", step, "RelativeAccuracy", Simulation.emigrationData.emigrationRelativeAccuracy));
            WriteLine(string.Format("{0},{1},{2}", step, "AbsoluteAccuracy", Simulation.emigrationData.emigrationAbsoluteAccuracy));
        }
    }
}
