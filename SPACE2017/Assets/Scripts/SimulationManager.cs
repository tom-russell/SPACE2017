using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Extensions;
using Assets.Scripts.Config;
using System.Linq;
using Assets;
using Assets.Scripts.Nests;
using Assets.Scripts.Ants;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }
    public bool SimulationRunning { get; private set; }
    public List<Transform> nests = new List<Transform>();
    public List<NestInfo> NestInfo { get; private set; }
    public GameObject[] doors;
    public SimulationData simData { get; private set; }
    public ResultsManager ResultsManager { get; private set; }
    public List<AntManager> Ants { get; private set; }
    public SimulationSettings Settings { get; private set; }
    public EmigrationInformation EmigrationInformation { get; private set; }
    int _sinceEmigrationCheck = 5;
    public int currentTick { get; private set; }

    //Parameters
    private GameObject initialNest;
    public float InitialScouts { get { return (Settings.ProportionActive.Value * Settings.ColonySize.Value) - 1 * Settings.QuorumThreshold.Value; } }
    private bool _spawnOnlyScouts = false;

    public void Begin(SimulationSettings settings)
    {
        Instance = this;

        Ants = new List<AntManager>();
        NestInfo = new List<NestInfo>();
        simData = new SimulationData();

        Settings = settings;
        if (Settings == null)
            Settings = new SimulationSettings();

        Debug.Log(Naming.SimulationEndPoints.points[Settings.SimulationEndPoint.Value]);

        RandomGenerator.Init(Settings.RandomSeed.Value);

        Time.timeScale = settings.StartingTimeScale.Value;
        

        doors = GameObject.FindGameObjectsWithTag(Naming.World.Doors);

        initialNest = GameObject.Find(Naming.World.InitialNest);
        initialNest.NestManager().simulation = this;
        initialNest.NestManager().quality = Settings.StartingNestQuality.Value;
        nests.Add(initialNest.transform);

        GameObject[] newNests = GameObject.FindGameObjectsWithTag(Naming.World.NewNests);
        //?GameObject arena = GameObject.FindGameObjectWithTag(Naming.World.Arena);

        MakeObject(Naming.ObjectGroups.Pheromones, null);
        Transform antHolder = MakeObject(Naming.ObjectGroups.Ants, null).transform;

        // For some reason scouting isnt suffixed with nest number - perhaps because scouting doesnt need a nest
        MakeObject(Naming.Ants.BehavourState.Scouting, antHolder);
        MakeObject("F", antHolder);

        SpawnColony(antHolder);

        //set up various classes of ants
        for (int i = 0; i < newNests.Length; i++)
        {
            Transform t = newNests[i].transform;

            this.nests.Add(t.transform);
            newNests[i].NestManager().simulation = this;

            if (i == 0)
                newNests[i].NestManager().quality = Settings.FirstNewNestQuality.Value;
            else if (i == 1)
                newNests[i].NestManager().quality = Settings.SecondNewNestQuality.Value;

            int id = i + 1;

            NestInfo.Add(new NestInfo(newNests[i].NestManager(), id, false,
                MakeObject(Naming.Ants.BehavourState.Assessing + id, antHolder),
                MakeObject(Naming.Ants.BehavourState.Recruiting + id, antHolder),
                MakeObject(Naming.Ants.BehavourState.Inactive + id, antHolder),
                MakeObject(Naming.Ants.BehavourState.Reversing + id, antHolder)
                ));
        }

        ResultsManager = new ResultsManager(this);
        EmigrationInformation = new EmigrationInformation(this);
        ResultsManager.SimulationStarted();

        SimulationRunning = true;
    }

    private void SpawnColony(Transform ants)
    {
        var antPrefab = Resources.Load(Naming.Resources.AntPrefab) as GameObject;

        Transform passive = MakeObject("P0", ants).transform;

        NestInfo.Add(new NestInfo(initialNest.NestManager(), 0, true,
               MakeObject(Naming.Ants.BehavourState.Assessing + "0", ants),
               MakeObject(Naming.Ants.BehavourState.Recruiting + "0", ants),
                passive.gameObject,
               MakeObject(Naming.Ants.BehavourState.Reversing + "0", ants)
           ));

        // Local variables for ant setup
        //find size of square to spawn ants into 
        float sqrt = Mathf.Ceil(Mathf.Sqrt(Settings.ColonySize.Value)); //?
        int spawnedAnts = 0;
        int spawnedAntScouts = 0;

        //just spawns ants in square around wherever this is placed
        while (spawnedAnts < Settings.ColonySize.Value)
        {
            int column = 0;
            while ((column == 0 || spawnedAnts % sqrt != 0) && spawnedAnts < Settings.ColonySize.Value)
            {
                float row = Mathf.Floor(spawnedAnts / sqrt);
                Vector3 pos = initialNest.transform.position;
                //?
                pos.x -= 1;
                pos.z -= 1;

                GameObject newAnt = Instantiate(antPrefab, pos + (new Vector3(row, 0, column) * AntScales.Distances.Spawning), Quaternion.identity);
                newAnt.transform.position += new Vector3(0, newAnt.GetComponent<CapsuleCollider>().radius * 2, 0);
                newAnt.name = CreateAntId(Settings.ColonySize.Value, spawnedAnts);
                newAnt.AntMovement().simulation = this;

                AntManager newAM = newAnt.AntManager();

                Ants.Add(newAM);

                newAM.AntId = spawnedAnts;
                newAM.myNest = initialNest.NestManager();
                // why is there 2 of this here? is it meant to be old nest or something
                // newAM.myNest = initialNest;
                newAM.simulation = this;
                newAM.inNest = true;
                newAM.quorumThreshold = Settings.QuorumThreshold.Value;
                newAnt.transform.parent = passive;

                if (spawnedAnts < Settings.ColonySize.Value * Settings.ProportionActive.Value || Settings.ColonySize.Value <= 1 || _spawnOnlyScouts)
                {
                    newAM.state = BehaviourState.Inactive;
                    newAM.passive = false;
                    newAnt.GetComponentInChildren<Renderer>().material.color = AntColours.States.Inactive;

                    Transform senses = newAnt.transform.Find(Naming.Ants.SensesArea);
                    (senses.GetComponent<SphereCollider>()).enabled = true;
                    (senses.GetComponent<SphereCollider>()).radius = AntScales.Distances.SensesCollider;
                    (senses.GetComponent<AntSenses>()).enabled = true;

                    if (spawnedAntScouts < InitialScouts || Settings.ColonySize.Value <= 1 || _spawnOnlyScouts)
                    {
                        newAM.nextAssessment = 1;
                        spawnedAntScouts++;
                    }
                    else
                    {
                        newAM.nextAssessment = 0;
                    }
                }
                else
                {
                    // Passive ant
                    newAM.passive = true;
                    newAnt.GetComponentInChildren<Renderer>().material.color = AntColours.States.InactivePassive;
                }

                column++;
                spawnedAnts++;
            }
        }
    }

    private string CreateAntId(int colonySize, int antNumber)
    {
        return Naming.Ants.Tag;
        //return string.Format("{0}{1}", Naming.Entities.AntPrefix, antNumber);
    }

    void FixedUpdate()
    {
        if (SimulationRunning)
        {
            currentTick++;

            // (this is used for testing determinism)
            if (Settings.RandomiseTimeScale.Value == true && currentTick % 500 == 0)
            {
                Time.timeScale = UnityEngine.Random.Range(1, 10);
            }

            foreach (AntManager ant in Ants)
            {
                ant.Tick();
            }
            ResultsManager.Tick();
            EmigrationInformation.Tick();
        }
        else
            return;

        // Check if time is expired
        if (Settings.MaximumSimulationRunTime.Value > 0 && TotalElapsedSimulatedTime("m") >= Settings.MaximumSimulationRunTime.Value)
        {
            SimulationRunning = false;
        }
        // Check if emigration complete
        _sinceEmigrationCheck--;
        if (_sinceEmigrationCheck <= 0)
        {
            _sinceEmigrationCheck = 5;
            if (EmigrationInformation.Data.PassivesInOldNest == 0)
            {
                SimulationRunning = false;
            }
        }

        // If we are no longer running this update then notify the simulation has stopped
        if (SimulationRunning == false)
        {
            ResultsManager.SimulationStopped();
        }
    }

    private GameObject MakeObject(string name, Transform parent)
    {
        GameObject g = new GameObject();
        g.name = name;
        g.transform.parent = parent;
        return g;
    }

    //returns the ID of the nest that is passed in
    public int GetNestID(NestManager nest)
    {
        return nests.IndexOf(nest.transform);
    }

    public float TotalElapsedSimulatedTime(string unit)
    {
        if (unit == "s")
        {
            return currentTick * Time.fixedDeltaTime;
        }
        else if (unit == "m")
        {
            return (currentTick * Time.fixedDeltaTime) / 60;
        }
        else if (unit == "ms")
        {
            return (currentTick * Time.fixedDeltaTime) * 1000;
        }
        else throw new System.Exception("Invalid function input: " + unit + ", s/m/ms expected.");
    }

    void OnDestroy()
    {
        ResultsManager.Dispose();
    }
}
