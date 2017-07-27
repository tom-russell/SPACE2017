using UnityEngine;
using UnityEngine.SceneManagement;

public class BatchRunner : MonoBehaviour
{

    public int firstExperiment = 1;
    public int lastExperiment = 200;
    public string sceneName = "Equidistant";
    public int[] quorumThresholds = new int[10] { 0, 1, 2, 5, 8, 10, 12, 15, 18, 20 };
    public int repeats = 10;

    //public float antennaReach;
    //public float[] averageAntennaReach;
    //public float buffonFrequency;
    //public float[] buffonFrequencyList;
    //public float pheromoneAdjustment;
    //public float[] pheromoneAdjustmentList;
    //public bool UseBuffons;

    public string[] fileName;
    public int index = 0;


    // Use this for initialization
    void Start()
    {

        //		averageAntennaReach = new float[20] {1.29f, 1.35f, 1.37f, 1.46f, 1.49f, 1.57f, 1.65f, 1.75f, 1.82f, 1.88f, 1.93f, 2.0f, 2.04f, 2.12f, 2.17f, 2.22f, 2.35f, 2.46f, 2.54f, 2.64f};
        //		fileName = new string[20] {"0-5R", "0-7R", "0-9R", "1-1R", "1-3R", "1-5R", "1-7R", "1-9R", "2-0R", "2-1R", "2-2R", "2-3R", "2-4R", "2-5R", "2-6R", "2-7R", "2-9R", "3-1R", "3-3R", "3-5R"};

        //		buffonFrequencyList = new float[20] {0.4286f, 0.2143f, 0.1429f, 0.1073f, 0.0857f, 0.0714f, 0.0612f, 0.0536f, 0.0476f, 0.0429f, 0.0390f, 0.0357f, 0.0330f, 0.0306f, 0.0286f, 0.0268f, 0.0252f, 0.0238f, 0.0226f, 0.0214f};
        //		fileName = new string[20] {"1-0R", "2-0R", "3-0R", "4-0R", "5-0R", "6-0R", "7-0R", "8-0R", "9-0R", "10-0R", "11-0R", "12-0R", "13-0R", "14-0R", "15-0R", "16-0R", "17-0R", "18-0R", "19-0R", "20-0R"};

        //		pheromoneAdjustmentList = new float[9] {0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f};
        //		fileName = new string[9] {"0-1R", "0-2R", "0-3R", "0-4R", "0-5R", "0-6R", "0-7R", "0-8R", "0-9R"};

        //		antennaReach = averageAntennaReach[0];					//tandem
        //		buffonFrequency = buffonFrequencyList[0];				//buffon
        //		pheromoneAdjustment = pheromoneAdjustmentList[2];		//pheromone

        fileName = new string[1] { "2-3R" };

        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene(sceneName);

    }

    public string GetNextOutputFile()
    {
        return "Results/" + firstExperiment + ".csv";
    }

    //GREG EDIT
    public string GREGGetNextOutputFile()
    {
        return "Results/tandemRuns_Pheromones_" + fileName[index] + "_" + firstExperiment + ".csv";     //tandem
                                                                                                        //		return "Results/NEWbuffonNeedle2R_Pheromones_" + fileName[index] + "_" + firstExperiment + ".csv";			//buffon
                                                                                                        // 		return "Results/PheromoneTest_Pheromones_" + fileName[index] + "_" + firstExperiment + ".csv";			//buffon
    }

    public void StartExperiment()
    {
        firstExperiment++;
        if ((firstExperiment - 1) % repeats == 0)
        {
            Debug.Log(index);
            // TODO: increment quorum threshold
           // quorumThreshold += 2;
        }
        //		antennaReach = averageAntennaReach[index];		//tandem
        //		buffonFrequency = buffonFrequencyList[index];	//buffon
        //		pheromoneAdjustment = pheromoneAdjustmentList[index];

        SceneManager.LoadScene(sceneName);
    }

    public int GetExperimentID()
    {
        return firstExperiment;
    }

}
