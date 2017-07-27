using Assets.Scripts.Ants;
using System.Collections.Generic;
using System.IO;

namespace Assets.Scripts.Output
{
    public class AntDeltaResults : DeltaResults
    {
        private Dictionary<int, BehaviourState> _stateHistory = new Dictionary<int, BehaviourState>();

        public AntDeltaResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "ants_delta"))
        {
            WriteLine("Tick,AntId,State,Position");
        }

        public override void Step(long step)
        {
            foreach (var ant in Simulation.Ants)
            {
                if (StateChanged(ant))
                    WriteLine(string.Format("{0},{1},{2},({3},{4},{5})", step, ant.AntId, ant.state, ant.transform.position.x, ant.transform.position.y, ant.transform.position.z));
            }
        }

        private bool StateChanged(AntManager ant)
        {
            if (!_stateHistory.ContainsKey(ant.AntId))
            {
                _stateHistory.Add(ant.AntId, ant.state);
                return true;
            }

            if (_stateHistory[ant.AntId] != ant.state)
            {
                _stateHistory[ant.AntId] = ant.state;
                return true;
            }

            return false;
        }
    }
}
