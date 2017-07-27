using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.Ants;

public class NestManager : MonoBehaviour 
{
    public SimulationManager simulation;
	public float quality = 0.5f; //between zero and one
	public GameObject door = null;
	
    void Start()
    {
        door = transform.Find("Door").gameObject;
        simulation = FindObjectOfType<SimulationManager>();
    }

	void OnTriggerEnter(Collider other) 
	{
		//if other isn't an ant or an ants collider has intersected with nest collider in an area that isn't the entrance then ignore
		if(other.tag != Naming.Ants.Tag /*|| (door != null && Vector3.Distance(other.transform.position, door.transform.position) > AntScales.Distances.DoorEntry)*/) 
			return;
		
		//let the ant know it has entered the nest
		AntManager ant = other.transform.GetComponent<AntManager>();
		ant.EnteredNest(this);
	}
	
	void OnTriggerExit(Collider other) 
	{
		//if other isn't an ant or an ants collider has intersected with nest collider in an area that isn't the entrance then ignore
		if(other.tag != Naming.Ants.Tag /*//?|| (door != null && Vector3.Distance(other.transform.position, door.transform.position) > AntScales.Distances.DoorEntry)*/) 
			return;
		AntManager ant = other.transform.GetComponent<AntManager>();
		
		//if ant is passive and somehow reaches edge of nest then turn around, otherwise let the ant know it has left the nest
		ant.LeftNest();
	}
	
    // Calculate the current quorum total within this nest (the total number of ants within the nest)
	public int GetQuorum()
	{
		int id = simulation.GetNestID(this);
		int total = GameObject.Find("P" + id).transform.childCount;

        Transform a = GameObject.Find("A" + id).transform;
		for (int i = 0; i < a.childCount; i++)
		{
			AntManager assessor = a.GetChild(i).GetComponent<AntManager>();
            // If the assessor is currently in this nest count them towards the total 
            if (assessor.currentNest == this)
            {
                total += 1;
            }
		}
		
		Transform r = GameObject.Find("R" + id).transform;
		for(int i = 0; i < r.childCount; i++)
		{
			AntManager recruiter = r.GetChild(i).GetComponent<AntManager>();
            // If the recruiter is currently waiting in this nest count them towards the total
            if (recruiter.currentNest == this)
            {
                total += 1;
            }
		}

        // The ant doesn't count itself towards the total
		return total - 1;
	}
	
	public int GetPassive()
	{
		return GameObject.Find("P" + simulation.GetNestID(this)).transform.childCount;
	}
}