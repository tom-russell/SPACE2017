using Assets.Scripts.Extensions;
using System;
using System.Collections.Generic;
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

            Dictionary<string, float> runOutput = CalculateTandemValues();

            WriteLine(nestNumbersOutput.Substring(1));
            WriteLine("Absolute Emigration Accuracy: " + Simulation.emigrationData.emigrationAbsoluteAccuracy);
            WriteLine("Relative Emigration Accuracy: " + Simulation.emigrationData.emigrationRelativeAccuracy);
            WriteLine("Emigration Completion: " + Simulation.emigrationData.emigrationAbsoluteAccuracy);
            WriteLine("FTRs Completed: " + (int) runOutput["forward success"]);
            WriteLine("FTRs Failed: " + (int) runOutput["forward failed"]);
            WriteLine("FTR Mean Duration: " + runOutput["forward mean duration"]);
            WriteLine("FTR Mean Distance: " + runOutput["forward mean distance"]);
            WriteLine("FTR Mean Speed: " + runOutput["forward mean speed"]);
            WriteLine("FTR Failure Rate/min: " + runOutput["forward failures/min"]);
            WriteLine("RTRs Completed: " + (int) runOutput["reverse success"]);
            WriteLine("RTRs Failed: " + (int) runOutput["reverse failed"]);
            WriteLine("RTR Mean Duration: " + runOutput["reverse mean duration"]);
            WriteLine("RTR Mean Distance: " + runOutput["reverse mean distance"]);
            WriteLine("RTR Mean Speed: " + runOutput["reverse mean speed"]);
            WriteLine("RTR Failure Rate/min: " + runOutput["reverse failures/min"]);
            WriteLine("Discovery Time: " + Simulation.emigrationData.discoveryTime);
            WriteLine("First Recruiter Time: " + Simulation.emigrationData.firstRecruiter);
            WriteLine("First Tandem Time: " + Simulation.emigrationData.firstTandem);
            WriteLine("First Carry Time: " + Simulation.emigrationData. firstCarry);
            WriteLine("First Reverse Time: " + Simulation.emigrationData.firstReverse);
            WriteLine("End of Emigration Time: " + Simulation.emigrationData.endOfEmigration);

            _cleanedUp = true;
        }

        private Dictionary<string, float> CalculateTandemValues()
        {
            // Calculating tandem run output numbers from the emigration data

            Dictionary<string, float> runOutput = new Dictionary<string, float>() {
                {"forward success", 0}, {"reverse success", 0},
                {"forward failed", 0}, {"reverse failed", 0},
                {"forward total duration", 0}, {"reverse total duration", 0},
                {"forward total distance", 0}, {"reverse total distance", 0}
            };

            foreach (EmigrationData.TandemRunData run in Simulation.emigrationData.tandemRunData)
            {
                string direction = "forward";
                if (run.forwardRun != true)
                {
                    direction = "reverse";
                }

                if (run.success == true) runOutput[direction + " success"]++;
                else runOutput[direction + " failed"]++;
                runOutput[direction + " total duration"] += run.duration;
                runOutput[direction + " total distance"] += run.distance;
            }

            float forwardRuns = runOutput["forward success"] + runOutput["forward failed"];
            float reverseRuns = runOutput["reverse success"] + runOutput["reverse failed"];
            // Adding mean values to the output
            runOutput.Add("forward mean duration", runOutput["forward total duration"] / forwardRuns);
            runOutput.Add("reverse mean duration", runOutput["reverse total duration"] / reverseRuns);
            runOutput.Add("forward mean distance", runOutput["forward total distance"] / forwardRuns);
            runOutput.Add("reverse mean distance", runOutput["reverse total distance"] / forwardRuns);
            runOutput.Add("forward mean speed", runOutput["forward mean distance"] / runOutput["forward mean duration"]);
            runOutput.Add("reverse mean speed", runOutput["reverse mean distance"] / runOutput["reverse mean duration"]);

            // Adding failure rates / min to the output (to compare to the scientific literature values, which are in these units)
            runOutput.Add("forward failures/min", runOutput["forward failed"] / (runOutput["forward total duration"] / 60));
            runOutput.Add("reverse failures/min", runOutput["reverse failed"] / (runOutput["reverse total duration"] / 60));
            return runOutput;
        }

        protected override void BeforeDispose()
        {
            base.BeforeDispose();
            CleanUp();
        }
    }
}
