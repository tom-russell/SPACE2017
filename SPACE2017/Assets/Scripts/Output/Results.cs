using System;
using System.IO;

namespace Assets.Scripts.Output
{
    public abstract class Results : IDisposable
    {
        public SimulationManager Simulation { get; private set; }

#if !UNITY_WEBGl
        private StreamWriter _writer;
#endif

        public Results(SimulationManager simulation, string fileNameWithoutExtension)
        {
            Simulation = simulation;
#if !UNITY_WEBGl
            _writer = new StreamWriter(fileNameWithoutExtension + ".txt", false);
#endif
        }

        public abstract void Step(long step);

        protected void Write(string message)
        {
#if !UNITY_WEBGl
            _writer.Write(message);
#endif
        }

        protected void WriteLine(string message = "")
        {
#if !UNITY_WEBGl
            _writer.WriteLine(message);
#endif
        }

        public void Dispose()
        {
            BeforeDispose();

#if !UNITY_WEBGl
            if (_writer != null)
                _writer.Close();
#endif
        }

        protected virtual void BeforeDispose()
        {
        }

        public virtual void SimulationStarted()
        {

        }

        public virtual void SimulationStopped()
        {

        }
    }
}
