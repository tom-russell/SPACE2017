using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Assets.Scripts.Ants;

namespace Assets.Scripts.Output
{
    public class LegacyResults : FixedTickResults
    {
        Transform s;
        List<Transform> a, r, p;
        public UnityEngine.GameObject[] ants;
        int c;
        //?float timeStep = 1f;
        public SimData History;

        public LegacyResults(SimulationManager simulation, string basePath)
            : base(simulation, Path.Combine(basePath, "legacy"))
        {
        }

        public override void SimulationStarted()
        {
            //get all ant state container objects
            a = new List<Transform>();
            r = new List<Transform>();
            p = new List<Transform>();
            s = GameObject.Find(Naming.Ants.BehavourState.Scouting).transform;
            p.Add(GameObject.Find(Naming.Ants.BehavourState.Inactive + "0").transform);
            // Get only the new nests (so i = 1)
            for (int i = 1; i < Simulation.nests.Count; i++)
            {
                p.Add(GameObject.Find(Naming.Ants.BehavourState.Inactive + i).transform);
                a.Add(GameObject.Find(Naming.Ants.BehavourState.Assessing + i).transform);
                r.Add(GameObject.Find(Naming.Ants.BehavourState.Recruiting + i).transform);
            }

            int quorumThresh = Simulation.Settings.QuorumThreshold.Value;

            Write("Space Data");
            WriteLine();
            Write("QuorumThresh = " + quorumThresh);
            WriteLine();
            Write("Colony size = " + Simulation.Settings.ColonySize.Value);
            WriteLine();

            //write column titles
            Write("T, ");
            Write("S, ");
            for (int i = 0; i < p.Count; i++)
            {
                Write("P" + i + ", ");
            }
            for (int i = 1; i <= a.Count; i++)
            {
                Write("A" + i + ", "); ;
            }
            for (int i = 1; i <= r.Count; i++)
            {
                if (i == r.Count)
                    Write("R" + i);
                else
                    Write("R" + i + ", ");
            }
            WriteLine();
            c = 0;
        }

        protected override void OutputData(long step)
        {
            //check if setup
            if (s == null || c < 0) return;

            Write(step + ", ");
            Write(s.childCount + ", ");
            for (int i = 0; i < p.Count; i++)
            {
                Write(p[i].childCount + ", ");
            }
            for (int i = 0; i < a.Count; i++)
            {
                Write(a[i].childCount + ", ");
            }
            for (int i = 0; i < r.Count; i++)
            {
                if (i == r.Count - 1)
                    Write(r[i].childCount.ToString());
                else
                    Write(r[i].childCount + ", ");
            }
            WriteLine();
            c++;
        }

        public override void SimulationStopped()
        {
            ants = GameObject.FindGameObjectsWithTag("Ant");
            Write("State History Begins");
            WriteLine();

            Write("Left, FT, FC, NT, NC, NS, FR, NR,");
            for (int i = 1; i < p.Count; i++)
            {
                Write("Found " + i + ", ");
            }
            for (int i = 1; i < p.Count; i++)
            {
                Write("Recruited to " + i + ", ");
            }
            for (int i = 1; i < p.Count; i++)
            {
                Write("Assessed " + i + ", ");
            }
            for (int i = 1; i < p.Count; i++)
            {
                Write("Accepted" + i + ", ");
            }
            Write("State History");
            WriteLine();

            foreach (UnityEngine.GameObject ant in ants)
            {
                SimData Data = ant.GetComponent<SimData>();
                if (Data == null)
                    continue;
                Write(Data.LeftOld + ", ");
                Write(Data.firstTandem + ", ");
                Write(Data.firstCarry + ", ");
                Write(Data.numTandem + ", ");
                Write(Data.numCarry + ", ");
                Write(Data.numSwitch + ", ");
                Write(Data.firstRev + ", ");
                Write(Data.numRev + ", ");

                StringBuilder builder = new StringBuilder();
                foreach (int nestTime in Data.NestDiscoveryTime)
                {
                    // Append each int to the StringBuilder overload.
                    builder.Append(nestTime).Append(" ");
                }
                string result = builder.ToString();
                Write(result + ", ");

                builder = new StringBuilder();
                foreach (int nestTime in Data.NestRecruitTime)
                {
                    // Append each int to the StringBuilder overload.
                    builder.Append(nestTime).Append(" ");
                }
                result = builder.ToString();
                Write(result + ", ");

                builder = new StringBuilder();
                foreach (int assessments in Data.numAssessments)
                {
                    // Append each int to the StringBuilder overload.
                    builder.Append(assessments).Append(" ");
                }
                result = builder.ToString();
                Write(result + ", ");

                builder = new StringBuilder();
                foreach (int acceptances in Data.numAcceptance)
                {
                    // Append each int to the StringBuilder overload.
                    builder.Append(acceptances).Append(" ");
                }
                result = builder.ToString();
                Write(result + ", ");

                builder = new StringBuilder();
                foreach (BehaviourState state in Data.StateHistory)
                {
                    // Append each int to the StringBuilder overload.
                    builder.Append(state).Append(" ");
                }
                result = builder.ToString();
                Write(result);

                //Write((string)Data.StateHistory+ ", ");
                WriteLine();
            }
            //Debug.Log(ants.Count);
            return;
        }
    }
}