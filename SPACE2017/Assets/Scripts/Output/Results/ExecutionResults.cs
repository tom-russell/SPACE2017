using Assets.Scripts.Extensions;
using System;
using System.IO;

namespace Assets.Scripts.Output
{
    public class ExecutionResults : DeltaResults
    {
        private DateTime _start = DateTime.Now;

        private bool _cleanedUp = false;

        public ExecutionResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "execution"))
        {
            WriteLine("Start: " + _start.ToString("dd/MM/yyyy HH:mm:ss"));
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

            var end = DateTime.Now;
            var duration = end - _start;
            
            TimeSpan simDuration = System.TimeSpan.FromSeconds(Simulation.totalElapsedSimulatedTime("s"));
            WriteLine("End: " + end.ToString("dd/MM/yyyy HH:mm:ss"));
            WriteLine("Execution Duration: " + duration.ToOutputString());
            WriteLine("Simulation Duration: " + simDuration.ToOutputString());

            _cleanedUp = true;
        }

        protected override void BeforeDispose()
        {
            base.BeforeDispose();
            CleanUp();
        }
    }
}
