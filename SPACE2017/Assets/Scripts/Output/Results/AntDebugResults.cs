using Assets.Scripts.Ants;
using System.Collections.Generic;
using System.IO;

namespace Assets.Scripts.Output
{
    public class AntDebugResults : FixedTickResults
    {
        //?private Dictionary<int, BehaviourState> _stateHistory = new Dictionary<int, BehaviourState>();

        public AntDebugResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "ants_debug"))
        {
            WriteLine("Tick,AntId,PerceivedTicks");
        }

        protected override void OutputData(long step)
        {
            foreach (var ant in Simulation.Ants)
            {
                WriteLine(string.Format("{0},{1},{2}", step, ant.AntId, ant.PerceivedTicks));
            }
        }
    }
}
