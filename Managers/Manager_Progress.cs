using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

public class Manager_Progress
{
    public static Dictionary<int, Quest> AllQuests = new();
    public Quest GetQuest(int questID) => AllQuests[questID];

    public static void Initialise()
    {
        _mainQuests();
        _sideQuests();
    }

    static void _mainQuests()
    {
        Quest killTestQuest = new Quest(
            0,
            "Kill Test Quest",
            "You have been tasked by the farmer to kill the thing.",
            new List<QuestStage>
            {
                new QuestStage (0, "Find the thing", "The farmer has asked you to kill the thing. First, find it."),
                new QuestStage (1, "Kill the thing", "You have found the thing, now, kill it."),
                new QuestStage (2, "Return with the thing", "You have killed the thing, now, return with it. A hero.")
            }
            );
        AllQuests.Add(0, killTestQuest);
    }

    static void _sideQuests()
    {

    }
}

[Serializable]
public class Quest
{
    public int QuestID;
    public string QuestName;
    public string QuestDescription;
    public int CurrentStage;
    public List<QuestStage> QuestStages;

    public Quest(int questID, string questName, string questDescription, List<QuestStage> questStages)
    {
        QuestID = questID;
        QuestName = questName;
        QuestDescription = questDescription;
        QuestStages = questStages;
    }

    public void SetQuestStage(int stageID, int stageProgress)
    {
        CurrentStage = QuestStages.FirstOrDefault(s => s.StageID == stageID).StageID;
    }
}


[Serializable]
public class QuestStage
{
    public int StageID;
    public string StageName;
    public string StageDescription;

    public QuestStage(int stageID, string stageName, string stageDescription)
    {
        StageID = stageID;
        StageName = stageName;
        StageDescription = stageDescription;
    }
}