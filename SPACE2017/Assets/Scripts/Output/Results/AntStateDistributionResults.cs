using System.IO;
using System.Linq;

namespace Assets.Scripts.Output
{
    public class AntStateDistributionResults : FixedTickResults
    {
        public AntStateDistributionResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "ant_state"))
        {
            WriteLine("Tick,State,Ants");
        }

        protected override void OutputData(long step)
        {
            var grouped = Simulation.Ants.GroupBy(a => a.state);

            foreach(var state in grouped)
            {
                WriteLine(string.Format("{0},{1},{2}", step, state.Key, state.Count()));
            }
        }
    }
}
