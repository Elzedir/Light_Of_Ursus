using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum VocationName
{
    Farmer,
    LumberJack,
    Miner
}

public class Manager_Vocation
{
    public static List<Vocation> AllVocations = new();

    private void InitialiseVocations()
    {
        _lumberjack();
    }

    void _lumberjack()
    {
        AllVocations.Add(new Vocation(
            vocationName: VocationName.LumberJack,
            vocationDescription: "A lumberjack",
            allVocationTitles: new Dictionary<int, VocationTitle> 
            {
                { 1, VocationTitle.Novice },
                { 100, VocationTitle.Apprentice },
                { 1000, VocationTitle.Journeyman },
                { 10000, VocationTitle.Master },
            }
            ));
    }

    public static Vocation GetVocation(VocationName vocationName)
    {
        return AllVocations.FirstOrDefault(v => v.VocationName == vocationName);
    }
}

public enum VocationTitle
{
    None,
    Novice,
    Apprentice,
    Journeyman,
    Master,
}

public class Vocation
{
    public VocationName VocationName;
    public string VocationDescription;

    public Dictionary<int, VocationTitle> AllVocationTitles = new();

    public Vocation(VocationName vocationName, string vocationDescription, Dictionary<int, VocationTitle> allVocationTitles)
    {
        VocationName = vocationName;
        VocationDescription = vocationDescription;
        AllVocationTitles = allVocationTitles;
    }

    public VocationTitle CheckVocationTitle(float currentVocationExperience)
    {
        return AllVocationTitles
            .Where(kv => currentVocationExperience >= kv.Key)
            .OrderByDescending(kv => kv.Key)
            .Select(kv => kv.Value)
            .FirstOrDefault();
    }

    public float GetSuccessChance(float currentExperience, float experienceRequired)
    {
        return ((currentExperience - experienceRequired) / currentExperience) * 100;
    }
}

public class VocationData
{
    public int ActorID;
    public int FactionID;

    public VocationTitle VocationTitle;
    public Dictionary<VocationName, float> Vocations;

    public VocationData(Dictionary<VocationName, float> vocations)
    {
        Vocations = vocations;
    }

    public void SetActorAndFactionID(int actorID, int factionID)
    {
        ActorID = actorID;
        FactionID = factionID;
    }

    public void AddVocation(VocationName vocationName, float vocationExperience)
    {
        if (Vocations.ContainsKey(vocationName)) throw new ArgumentException($"Vocation: {vocationName} already exists in Vocations.");

        Vocations.Add(vocationName, 0);
    }

    public void RemoveVocation(VocationName vocationName)
    {
        if (!Vocations.ContainsKey(vocationName)) throw new ArgumentException($"Vocation: {vocationName} does not exist in Vocations.");

        Vocations.Remove(vocationName);
    }

    public void ChangeVocationExperience(VocationName vocationName, float experienceChange)
    {
        if (!Vocations.ContainsKey(vocationName)) throw new ArgumentException($"Vocation: {vocationName} does not exist in Vocations.");

        Vocations[vocationName] += experienceChange;
    }

    public float GetSuccessChance(VocationName vocationName, float experienceRequired)
    {
        Vocation vocation = Manager_Vocation.GetVocation(vocationName);
        return vocation.GetSuccessChance(Vocations[vocationName], experienceRequired);
    }
}