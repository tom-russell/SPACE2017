using System.IO;
using UnityEngine;

namespace Assets.Scripts.Output
{
    public class AntDetailedResults : FixedTickResults
    {
        public AntDetailedResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "ants_detail"))
        {
            WriteLine("Tick,AntId,State,PosX,PosY,PosZ");
        }

        protected override void OutputData(long step)
        {
            foreach (var ant in Simulation.Ants)
            {
                Vector3 pos = ant.transform.position;
                WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", step, ant.AntId, ant.state, pos.x, pos.y, pos.z));
            }
        }
    }
}
