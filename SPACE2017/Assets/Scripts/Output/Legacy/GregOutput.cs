using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.Ants;

public class GregOutput : MonoBehaviour
{
	
	Transform s;
	List<Transform> a, r, p;
	public UnityEngine.GameObject[] ants;
	StreamWriter greg_sw;
	int c;
	int timeStampCount = 0;
	float timeStep = 1f;
	public SimData History;
	public int antNumber = 0;
	
	public void SetUp()
	{
		//get all ant state container objects
		a = new List<Transform>();
		r = new List<Transform>();
		p = new List<Transform>();
		s = GameObject.Find("S").transform;
		p.Add(GameObject.Find("P0").transform);
		for(int i = 1; i <= GameObject.Find(Naming.World.NewNests).transform.childCount; i++)
		{
			p.Add(GameObject.Find("P" + i).transform);
			a.Add(GameObject.Find("A" + i).transform);
			r.Add(GameObject.Find("R" + i).transform);
		}
		
		GameObject batchGO = GameObject.Find(Naming.Simulation.BatchRunner);
		BatchRunner batch = batchGO.transform.GetComponent<BatchRunner>();
        int quorumThresh = 0;// batch.quorumThreshold;// TODO: quorumThresh
		
		string gregOutputFile = batch.GREGGetNextOutputFile ();
		
		try
		{
			greg_sw = new StreamWriter(gregOutputFile);
		} 
		catch
		{
			print("Couldn't find output file");
			return;
		}
		greg_sw.Write("SPACE DATA\r\n\r\n");	
		greg_sw.Write("{\r\n\r\n");	
		greg_sw.Write("\t\"metadata\" : {\r\n");
		greg_sw.Write("\t\t\"QuorumThresh\":'" + quorumThresh + "',\r\n");
		greg_sw.Write("\t\t\"Colony_Size\":'200'\r\n");
		greg_sw.Write("\t},\r\n\r\n");
		greg_sw.Write("\t\"timeline\" : {\r\n");
		c = 0;
		
		//make writestatestofile be called every timeStep
		InvokeRepeating("WriteStatesToFile", 0f, timeStep);
	}
	
	void WriteStatesToFile()
	{
		//check if setup
		if(s == null || c < 0) return;
		
		greg_sw.Write("\t\t\"T_" + Mathf.Round(c * timeStep) + "\" : {\r\n");
		greg_sw.Write("\t\t\t\"S\":'" + s.childCount + "', ");
		for(int i = 0; i < p.Count; i++)
		{
			greg_sw.Write("\"P" + i + "\":'" + p[i].childCount + "', ");
		}
		for(int i = 0; i < a.Count; i++)
		{
			greg_sw.Write("\"A" + (i+1) + "\":'" + a[i].childCount + "', ");
		}
		for(int i = 0; i < r.Count; i++)
		{
			if(i == r.Count - 1)
				greg_sw.Write("\"R" + (i+1) + "\":'" + r[i].childCount + "'\r\n");
			else 
				greg_sw.Write("\"R" + (i+1) + "\":'" + r[i].childCount + "', ");
		}
		c++;
		timeStampCount++;
		
		//if there are no passive ants left in the original nest then restart the simulation
		//if the simulations last 540 timestamps, terminate the emigration early 
//		if(timeStampCount == 600 || p[0].childCount <= 70) // set to 1 as sometimes a passive ant gets stuck as a RTR follower
//greg edit		if(timeStampCount == 600 || p[0].childCount <= 1) // set to 1 as sometimes a passive ant gets stuck as a RTR follower
		if(timeStampCount == 6000 || p[0].childCount <= 1) // set to 1 as sometimes a passive ant gets stuck as a RTR follower
			//		if(timeStampCount == 600 || p[0].childCount <= 70) // set to 1 as sometimes a passive ant gets stuck as a RTR follower
		{
			greg_sw.Write("\t\t}\r\n");
			greg_sw.Write("\t}\r\n");
			greg_sw.Write("}\r\n\r\n");
			
			c = -1;
			WriteFinalState();
			greg_sw.Close();
			(GameObject.Find(Naming.Simulation.BatchRunner).GetComponent<BatchRunner>()).StartExperiment();
		} else {
			greg_sw.Write("\t\t},\r\n");
		}
	}
	
	void WriteFinalState() {
		ants = GameObject.FindGameObjectsWithTag("Ant");
		
		greg_sw.Write("STATE HISTORY\r\n\r\n");
		greg_sw.Write("{\r\n\r\n");
		
		int lastAnt = ants.Length;
		int countAnt = 0;
		foreach (UnityEngine.GameObject ant in ants)
		{
			antNumber = antNumber + 1;
			
			SimData Data = ant.GetComponent<SimData>();
			
			greg_sw.Write("\t\"ant_" + antNumber + "\" : {\r\n");
			greg_sw.Write("\t\t\"overview\" : {\r\n");
			greg_sw.Write("\t\t\t\"leftOld\":'" + Data.LeftOld + "', ");
			greg_sw.Write("\"firstTandem\":'" + Data.firstTandem + "', ");
			greg_sw.Write("\"firstCarry\":'" + Data.firstCarry + "', ");
			greg_sw.Write("\"numSwitch\":'" + Data.numSwitch + "', ");
			greg_sw.Write("\"firstRev\":'" + Data.firstRev + "', ");
			greg_sw.Write("\"numCarry\":'" + Data.numCarry + "', ");
			greg_sw.Write("\"numTandem\":'" + Data.numTandem + "', ");
			greg_sw.Write("\"numReverse\":'" + Data.numRev + "', ");
			
			greg_sw.Write("\"Failed_Leader_NewNest\":'" + Data.failedLeaderNewNest + "', ");
			greg_sw.Write("\"Failed_Leader_NewFollower\":'" + Data.failedLeaderFoundFollower + "', ");
			
			greg_sw.Write("\"completeTandem\":'" + Data.successfulForwardTandemRuns + "', ");
			greg_sw.Write("\"failedTandem\":'" + Data.failedForwardTandemRuns + "', ");
			greg_sw.Write("\"completeReverse\":'" + Data.successfulReverseTandemRuns + "', ");
			greg_sw.Write("\"failedReverse\":'" + Data.failedReverseTandemRuns + "', ");
			
			StringBuilder gregBuilder = new StringBuilder();
			int nestNumber = 0;
			foreach (int nestTime in Data.NestDiscoveryTime)
			{
				nestNumber = nestNumber + 1;
				gregBuilder.Append("\"Found_" + nestNumber + "\":'").Append(nestTime).Append("', ");
			}
			string gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult);
			
			gregBuilder = new StringBuilder();
			nestNumber = 0;
			foreach (int nestTime in Data.NestRecruitTime)
			{
				nestNumber = nestNumber + 1;
				gregBuilder.Append("\"Recruited_to_" + nestNumber + "\":'").Append(nestTime).Append("', ");	
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult);
			
			gregBuilder = new StringBuilder();
			nestNumber = 0;
			foreach (int assessments in Data.numAssessments)
			{
				nestNumber = nestNumber + 1;
				gregBuilder.Append("\"Assessed_" + nestNumber + "\":'").Append(assessments).Append("', ");
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult);
			
			gregBuilder = new StringBuilder();
			nestNumber = 0;
			int lastAcc = Data.numAcceptance.Count;
			int countAcc = 0;
			foreach (int acceptances in Data.numAcceptance)
			{
				nestNumber = nestNumber + 1;
				if (++countAcc == (lastAcc)) {
					gregBuilder.Append("\"Accepted_" + nestNumber + "\":'").Append(acceptances).Append("'");
				} else {
					gregBuilder.Append("\"Accepted_" + nestNumber + "\":'").Append(acceptances).Append("', ");
				}
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult);
			greg_sw.Write("\r\n");
			greg_sw.Write("\t\t},\r\n");
			
			
			
			greg_sw.Write("\t\t\"data\" : {\r\n");
			
			// 
			greg_sw.Write("\t\t\t\"Assessment_Time\" : {\r\n");
			int assessmentFirstNum = 0;
			int assessmentSecondNum = 0;
			gregBuilder = new StringBuilder();
			greg_sw.Write("\t\t\t\t");
			int lastAssessmentTime = Data.assessmentSecondTime.Count;
			int countAssessmentTime = 0;
			foreach (float assessmentFirstTime in Data.assessmentFirstTime)
			{
				assessmentFirstNum = assessmentFirstNum + 1;
				gregBuilder.Append("\"assessingFirstTime_" + assessmentFirstNum + "\":'").Append(assessmentFirstTime).Append("', ");
			}
			foreach (float assessmentSecondTime in Data.assessmentSecondTime)
			{
				assessmentSecondNum = assessmentSecondNum + 1;
				if (++countAssessmentTime == (lastAssessmentTime)) {
					gregBuilder.Append("\"assessmentSecondTime_" + assessmentSecondNum + "\":'").Append(assessmentSecondTime).Append("'");
				} else {
					gregBuilder.Append("\"assessmentSecondTime_" + assessmentSecondNum + "\":'").Append(assessmentSecondTime).Append("', ");
				}
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult + "\r\n");
			greg_sw.Write("\t\t\t},\r\n");
			
			// 
			greg_sw.Write("\t\t\t\"Assessment_Length\" : {\r\n");
			assessmentFirstNum = 0;
			assessmentSecondNum = 0;
			gregBuilder = new StringBuilder();
			greg_sw.Write("\t\t\t\t");
			int lastAssessmentLength = Data.assessmentSecondLength.Count;
			int countAssessmentLength = 0;
			foreach (float assessmentFirstLength in Data.assessmentFirstLength)
			{
				assessmentFirstNum = assessmentFirstNum + 1;
				gregBuilder.Append("\"assessingFirstLength_" + assessmentFirstNum + "\":'").Append(assessmentFirstLength).Append("', ");
			}
			foreach (float assessmentSecondLength in Data.assessmentSecondLength)
			{
				assessmentSecondNum = assessmentSecondNum + 1;
				if (++countAssessmentLength == (lastAssessmentLength)) {
					gregBuilder.Append("\"assessmentSecondLength_" + assessmentSecondNum + "\":'").Append(assessmentSecondLength).Append("'");
				} else {
					gregBuilder.Append("\"assessmentSecondLength_" + assessmentSecondNum + "\":'").Append(assessmentSecondLength).Append("', ");
				}
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult + "\r\n");
			greg_sw.Write("\t\t\t},\r\n");
			
			
			greg_sw.Write("\t\t\t\"Assessment_Area\" : {\r\n");
			gregBuilder = new StringBuilder();
			greg_sw.Write("\t\t\t\t");
			int lastAssessmentArea = Data.assessmentAreaResult.Count;
			int countAssessmentArea = 0;
			foreach (string assessmentAreaResult in Data.assessmentAreaResult)
			{
				if (++countAssessmentArea == (lastAssessmentArea)) {
					gregBuilder.Append("\"" + assessmentAreaResult + "'");
				} else {
					gregBuilder.Append("\"" + assessmentAreaResult + "', ");
				}
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult + "\r\n");
			greg_sw.Write("\t\t\t},\r\n");
			
			
			// amount of time steps each forward tandem run took for an ant
			greg_sw.Write("\t\t\t\"FTR_speed\" : {\r\n");
			int tandemNum = 0;
			gregBuilder = new StringBuilder();
			greg_sw.Write("\t\t\t\t");
			int lastFTRTime = Data.forwardTandemTimeSteps.Count;
			int countFTRTime = 0;
			foreach (float tandemTime in Data.forwardTandemTimeSteps)
			{
				tandemNum = tandemNum + 1;
				if (++countFTRTime == (lastFTRTime)) {
					gregBuilder.Append("\"ftr_" + tandemNum + "\":'").Append(tandemTime).Append("'");
				} else {
					gregBuilder.Append("\"ftr_" + tandemNum + "\":'").Append(tandemTime).Append("', ");
				}
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult + "\r\n");
			greg_sw.Write("\t\t\t},\r\n");
			
			// amount of time steps each reverse tandem run took for an ant
			greg_sw.Write("\t\t\t\"RTR_speed\" : {\r\n");
			tandemNum = 0;
			gregBuilder = new StringBuilder();
			greg_sw.Write("\t\t\t\t");
			int lastRTRTime = Data.reverseTandemTimeSteps.Count;
			int countRTRTime = 0;
			foreach (float tandemTime in Data.reverseTandemTimeSteps)
			{
				tandemNum = tandemNum + 1;
				if (++countRTRTime == (lastRTRTime)) {
					gregBuilder.Append("\"rtr_" + tandemNum + "\":'").Append(tandemTime).Append("'");
				} else {
					gregBuilder.Append("\"rtr_" + tandemNum + "\":'").Append(tandemTime).Append("', ");
				}
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult + "\r\n");
			greg_sw.Write("\t\t\t},\r\n");
			
			// amount of time steps each social carry took for an ant
			greg_sw.Write("\t\t\t\"socialCarrying_speed\" : {\r\n");
			tandemNum = 0;
			gregBuilder = new StringBuilder();
			greg_sw.Write("\t\t\t\t");
			int lastCarryTime = Data.carryingTimeSteps.Count;
			int countCarryTime = 0;
			foreach (float tandemTime in Data.carryingTimeSteps)
			{
				tandemNum = tandemNum + 1;
				if (++countCarryTime == (lastCarryTime)) {
					gregBuilder.Append("\"carry_" + tandemNum + "\":'").Append(tandemTime).Append("'");
				} else {
					gregBuilder.Append("\"carry_" + tandemNum + "\":'").Append(tandemTime).Append("', ");
				}
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult + "\r\n");
			greg_sw.Write("\t\t\t}\r\n");
			
			greg_sw.Write("\t\t},\r\n");
			
			
			
			greg_sw.Write("\t\t\"state_history\" : {\r\n");
			int timeStamp = 0;
			gregBuilder = new StringBuilder();
			greg_sw.Write("\t\t\t");
			foreach (BehaviourState state in Data.StateHistory)
			{
				timeStamp = timeStamp + 1;
				gregBuilder.Append("\"T_" + timeStamp + "\":'").Append(state).Append("', ");
			}
			gregResult = gregBuilder.ToString();
			greg_sw.Write(gregResult + "\r\n");
			greg_sw.Write("\t\t}\r\n");
			if (++countAnt == (lastAnt)) {
				greg_sw.Write("\t}\r\n");
			} else {
				greg_sw.Write("\t},\r\n\r\n");
			}
			
		}
		
		greg_sw.Write("}\r\n");
		greg_sw.Write("{\r\n");
		greg_sw.Write("\t\"gasterHeadDistance\":'" + (AntMovement.gasterHeadDistance / AntMovement.gasterHeadDistanceCount) + "'\r\n");
		greg_sw.Write("}\r\n");
		
		return;
	}
	
}


