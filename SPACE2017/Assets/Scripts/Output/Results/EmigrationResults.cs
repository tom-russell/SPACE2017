using System.IO;

namespace Assets.Scripts.Output
{
    public class EmigrationResults : FixedTickResults
    {
        public EmigrationResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "emigration"))
        {
            WriteLine("Tick,Property,Value");
        }

        protected override void OutputData(long step)
        {
            WriteLine(string.Format("{0},{1},{2}", step, "PassivesInOldNest", Simulation.EmigrationInformation.Data.PassivesInOldNest));
            foreach(var o in Simulation.EmigrationInformation.Data.PassivesInNewNests)
                WriteLine(string.Format("{0},{1},{2}", step, "PassivesInNewNest_" + o.Key, o.Value));
            WriteLine(string.Format("{0},{1},{2}", step, "Completion", Simulation.EmigrationInformation.Data.EmigrationCompletion));
            WriteLine(string.Format("{0},{1},{2}", step, "RelativeAccuracy", Simulation.EmigrationInformation.Data.EmigrationRelativeAccuracy));
            WriteLine(string.Format("{0},{1},{2}", step, "AbsoluteAccuracy", Simulation.EmigrationInformation.Data.EmigrationAbsoluteAccuracy));
        }
    }
}
