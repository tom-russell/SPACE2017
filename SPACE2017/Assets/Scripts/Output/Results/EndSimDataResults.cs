using Assets.Scripts.Extensions;
using System;
using System.IO;

namespace Assets.Scripts.Output
{
    public class EndSimDataResults : DeltaResults
    {
        private bool _cleanedUp = false;

        public EndSimDataResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "endSimData"))
        {
            //WriteLine("Start: " + _start.ToString("dd/MM/yyyy HH:mm:ss"));
        }

        public override void Step(long step)
        {
        }

        public override void SimulationStopped()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            if (_cleanedUp)
                return;

            int[] nestAllegiance = new int[Simulation.NestInfo.Count];

            foreach (AntManager ant in Simulation.Ants)
            {
                NestManager nest = ant.myNest;
                // If the ant is being carried, it is assigned to the carrier's nest
                if (ant.transform.parent.tag == "CarryPosition")
                {
                    nest = ant.transform.parent.parent.GetComponent<AntManager>().myNest;
                }

                nestAllegiance[Simulation.GetNestID(nest)]++;
            }

            string nestNumbersOutput = "";
            foreach (int nestNumbers in nestAllegiance)
            {
                nestNumbersOutput += "," + nestNumbers;
            }

            WriteLine(nestNumbersOutput.Substring(1));
            WriteLine("FTRs: " + (Simulation.simulationData.successFTR));
            WriteLine("RTRs: " + (Simulation.simulationData.successRTR));

            _cleanedUp = true;
        }

        protected override void BeforeDispose()
        {
            base.BeforeDispose();
            CleanUp();
        }
    }
}
