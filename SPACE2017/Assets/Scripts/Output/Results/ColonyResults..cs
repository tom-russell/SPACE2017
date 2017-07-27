using System.IO;

namespace Assets.Scripts.Output
{
    public class ColonyResults : FixedTickResults
    {
        public ColonyResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "colony"))
        {
            WriteLine("Tick,NestId,Inactive,Assessing,Recruiting,Reversing");
        }

        protected override void OutputData(long step)
        {
            foreach(var nest in Simulation.NestInfo)
            {
                WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", step, nest.NestId,
                    nest.AntsPassive.transform.childCount,
                    nest.AntsAssessing.transform.childCount,
                    nest.AntsRecruiting.transform.childCount,
                    nest.AntsReversing.transform.childCount));
            }
        }
    }
}
