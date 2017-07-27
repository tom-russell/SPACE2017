using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Ants;

public class SimData : MonoBehaviour
{
    public int LeftOld = 0;
    public int firstTandem = 0;
    public int firstCarry = 0;
    public int numTandem = 0;
    public int numCarry = 0;
    public int numSwitch = 0;
    public int firstRev = 0;
    public int numRev = 0;
    public List<BehaviourState> StateHistory = new List<BehaviourState>();
    public List<int> NestDiscoveryTime = new List<int>();
    public List<int> NestRecruitTime = new List<int>();
    public List<int> numAssessments = new List<int>();
    public List<int> numAcceptance = new List<int>();


    // greg edit
    // TANDEM RUNNING
    public List<float> forwardTandemTimeSteps = new List<float>();
    public List<float> reverseTandemTimeSteps = new List<float>();
    public List<float> carryingTimeSteps = new List<float>();

    public int successfulForwardTandemRuns = 0;
    public int successfulReverseTandemRuns = 0;
    public int failedForwardTandemRuns = 0;
    public int failedReverseTandemRuns = 0;

    public int failedLeaderNewNest = 0;
    public int failedLeaderFoundFollower = 0;

    // BUFFON NEEDLE
    public List<int> assessmentFirstTime = new List<int>();
    public List<int> assessmentSecondTime = new List<int>();

    public List<float> assessmentFirstLength = new List<float>();
    public List<float> assessmentSecondLength = new List<float>();

    public List<string> assessmentAreaResult = new List<string>();
    //


    public enum DiscoveryType
    {
        Unfound,
        Found,
        Lead,
        Carried
    };
    public List<DiscoveryType> NestDiscoveryType = new List<DiscoveryType>();

    public void Start()
    {
        GameObject[] newNests = GameObject.FindGameObjectsWithTag("NewNest");
        for (int i = 1; i <= newNests.Length; i++)
        {
            NestDiscoveryTime.Add(0);
            NestRecruitTime.Add(0);
            NestDiscoveryType.Add(DiscoveryType.Unfound);
            numAssessments.Add(0);
            numAcceptance.Add(0);

        }
    }

    public void completeFTR()
    {
        successfulForwardTandemRuns++;
    }

    public void completeRTR()
    {
        successfulReverseTandemRuns++;
    }

    public void failedFTR()
    {
        failedForwardTandemRuns++;
    }

    public void failedRTR()
    {
        failedReverseTandemRuns++;
    }

    public void failedLeaderNewNestAdd()
    {
        failedLeaderNewNest++;
    }

    public void failedLeaderFoundFollowerAdd()
    {
        failedLeaderFoundFollower++;
    }
}