using Assets.Scripts.Extensions;
using System;
using System.IO;

namespace Assets.Scripts.Output
{
    public class EmigrationEndResults : DeltaResults
    {
        private bool _cleanedUp = false;

        public EmigrationEndResults(SimulationManager simulation, string basePath)
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
            WriteLine("Absolute Emigration Accuracy: " + Simulation.simulationData.emigrationAbsoluteAccuracy);
            WriteLine("Relative Emigration Accuracy: " + Simulation.simulationData.emigrationRelativeAccuracy);
            WriteLine("Emigration Completion: " + Simulation.simulationData.emigrationAbsoluteAccuracy);
            WriteLine("FTRs: " + (Simulation.simulationData.successFTR));
            WriteLine("RTRs: " + (Simulation.simulationData.successRTR));
            WriteLine("Discovery Time: " + Simulation.simulationData.discoveryTime);
            WriteLine("First Recruiter Time: " + Simulation.simulationData.firstRecruiter);
            WriteLine("First Tandem Time: " + Simulation.simulationData.firstTandem);
            WriteLine("First Carry Time: " + Simulation.simulationData. firstCarry);
            WriteLine("First Reverse Time: " + Simulation.simulationData.firstReverse);
            WriteLine("End of Emigration Time: " + Simulation.simulationData.endOfEmigration);

            _cleanedUp = true;
        }

        protected override void BeforeDispose()
        {
            base.BeforeDispose();
            CleanUp();
        }
    }
}
