using Assets.Common;
using System.Collections.Generic;
using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Ants;

public class NestCount : MonoBehaviour
{
    private Text txtAssessing;
    private Text txtNestId;
    private Text txtPassive;
    private Text txtRecruiting;
    private Text txtReversing;

    public SimulationManager Simulation { get; private set; }

    private Lazy<List<NestCountControl>> _nestCountControls;

    private int? _highlightedNestIndex = null;

    private bool _instantiated = false;

    private void InstantiateUI()
    {
        Simulation = GameObject.FindObjectOfType<SimulationManager>() as SimulationManager;

        if (Simulation != null)
        {
            _instantiated = true;

            _nestCountControls = new Lazy<List<NestCountControl>>(() =>
            {
                var nestCountControlPrefab = Resources.Load("NestNumbers") as GameObject;
                var nestCountControls = new List<NestCountControl>();

                for (int i = 0; i < Simulation.NestInfo.Count; i++)
                {
                    var ctl = GameObject.Instantiate(nestCountControlPrefab);
                    ctl.transform.SetParent(transform);

                    ctl.GetComponent<RectTransform>().anchoredPosition = new Vector2(80 + (50 * i), -150);
                    nestCountControls.Add(ctl.GetComponent<NestCountControl>());
                }

                return nestCountControls;
            });
        }
    }

    void Update()
    {
        if (!_instantiated)
            InstantiateUI();
        if (!_instantiated)
            return;

        int? newHighlight = null;
        for (int i = 0; i < _nestCountControls.Value.Count; i++)
        {
            _nestCountControls.Value[i].SetData(
                Simulation.NestInfo[i].NestId,
                Simulation.NestInfo[i].AntsPassive.transform.childCount,
                Simulation.NestInfo[i].AntsAssessing.transform.childCount,
                Simulation.NestInfo[i].AntsRecruiting.transform.childCount,
                Simulation.NestInfo[i].AntsReversing.transform.childCount
                );

            if (_nestCountControls.Value[i].HasPointer)
                newHighlight = i;
        }

        if (!newHighlight.HasValue && _highlightedNestIndex.HasValue)
        {
            _highlightedNestIndex = null;
            ResetAntNestHighlight();
        }
        else if (_highlightedNestIndex != newHighlight)
        {
            _highlightedNestIndex = newHighlight;
            SetAntNestHighlight(_highlightedNestIndex.Value);
        }
    }

    private void SetAntNestHighlight(int value)
    {
        var nest = Simulation.nests[value].gameObject.NestManager();

        foreach (var ant in Simulation.Ants)
        {
            if (ant.myNest == nest)
            {
                if (ant.state == BehaviourState.Recruiting)
                    ant.SetTemporaryColour(AntColours.NestHighlight.Recruiting);
                else
                    ant.SetTemporaryColour(AntColours.NestHighlight.Home);
            }
            else if (ant.nestToAssess == nest)
            {
                ant.SetTemporaryColour(AntColours.NestHighlight.Assessing);
            }
            else if (ant.oldNest == nest)
            {
                ant.SetTemporaryColour(AntColours.NestHighlight.Old);
            }
            else
            {
                ant.SetTemporaryColour(AntColours.NestHighlight.None);
            }
        }
    }

    private void ResetAntNestHighlight()
    {
        foreach (var ant in Simulation.Ants)
            ant.ClearTemporaryColour();
    }
}
