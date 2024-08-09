using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Jobsite_Lumberjack : Jobsite_Base
{
    public List<Interactable_Lumberjack> AllStations;

    public override void Initialise(CityComponent city)
    {
        base.Initialise(city);

        AllStations = _getStationsInArea();
    }

    protected override void _initialise()
    {
        base._initialise();

        _initialiseJobs();
    }

    void _initialiseJobs()
    {
        AllocateJobPositions();
        FillJobPositions();
    }

    List<Interactable_Lumberjack> _getStationsInArea()
    {
        return Physics.OverlapBox(JobsiteArea.bounds.center, JobsiteArea.bounds.extents)
        .Select(collider => collider.GetComponent<Interactable_Lumberjack>()).Where(lumberjack => lumberjack != null).ToList();
    }

    public void AllocateJobPositions()
    {
        foreach (var position in AllStations.SelectMany(station => station.EmployeePositions).Where(position => !AllJobPositions.ContainsKey(position)).Distinct())
        {
            AllJobPositions[position] = new();
        }
    }

    public void FillJobPositions()
    {
        foreach (var position in AllJobPositions.Where(position => position.Value.Count == 0).ToList())
        {
            if (!_findEmployeeFromCity(position.Key, out Actor_Base actor)) 
            { 
                Debug.Log($"Actor {actor} is null and therefore new employee generated"); 
                actor = _generateNewEmployee(position.Key); 
            }

            if (!AllJobPositions.ContainsKey(position.Key))
            {
                AllJobPositions[position.Key] = new List<Actor_Base>();
            }

            AllJobPositions[position.Key].Add(actor);
            actor.JobComponent.AddJob(JobName.Lumberjack, this);
        }

        StartCoroutine(ShowPositions());
    }

    private Dictionary<string, GameObject> allPanels = new();

    public IEnumerator ShowPositions()
    {
        showPositions();

        yield return new WaitForSeconds(1);

        showPositions();

        yield return new WaitForSeconds(1);

        showPositions();
    }

    private void showPositions()
    {
        var positionPanel = GameObject.Find("PositionPanel");

        if (positionPanel == null)
        {
            Debug.LogError("PositionPanel not found!");
            return;
        }

        foreach (var position in AllJobPositions)
        {
            foreach (var actor in position.Value)
            {
                string panelName = $"PositionPanel_{position.Key}_{actor.name}";

                if (allPanels.TryGetValue(panelName, out var existingPanel))
                {
                    UpdatePanel(existingPanel, position.Key, actor);
                }
                else
                {
                    var newPanel = Instantiate(positionPanel, positionPanel.transform.parent);
                    newPanel.name = panelName;

                    UpdatePanel(newPanel, position.Key, actor);

                    allPanels[panelName] = newPanel;
                }
            }
        }
    }

    private void UpdatePanel(GameObject panel, EmployeePosition position, Actor_Base actor)
    {
        Manager_Game.FindTransformRecursively(panel.transform, "PositionText").GetComponent<TextMeshProUGUI>().text = position.ToString();
        Manager_Game.FindTransformRecursively(panel.transform, "EmployeeText").GetComponent<TextMeshProUGUI>().text = actor.name;
        panel.SetActive(true);
    }
}
