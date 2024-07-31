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
        // Add vocations
    }

    public static Vocation GetVocation(VocationName vocationName)
    {
        return AllVocations.FirstOrDefault(v => v.VocationName == vocationName);
    }
}

public class VocationComponent
{
    public Actor_Base Actor;

    public VocationTitle VocationTitle;
    public Dictionary<Vocation, float> Vocations;

    public VocationComponent(Actor_Base actor, Dictionary<Vocation, float> vocations)
    {
        Actor = actor;
        Vocations = vocations;
    }

    public void AddVocation(VocationName vocationName, float vocationExperience)
    {
        Vocation vocation = Manager_Vocation.GetVocation(vocationName);

        if (Vocations.ContainsKey(vocation)) throw new ArgumentException($"Vocation: {vocation} already exists in Vocations.");

        Vocations.Add(vocation, 0);
    }

    public void RemoveVocation(VocationName vocationName)
    {
        Vocation vocation = Manager_Vocation.GetVocation(vocationName);

        if (!Vocations.ContainsKey(vocation)) throw new ArgumentException($"Vocation: {vocation} does not exist in Vocations.");

        Vocations.Remove(vocation);
    }

    public void ChangeVocationExperience(VocationName vocationName, float experienceChange)
    {
        Vocation vocation = Manager_Vocation.GetVocation(vocationName);

        if (!Vocations.ContainsKey(vocation)) throw new ArgumentException($"Vocation: {vocation} does not exist in Vocations.");

        Vocations[vocation] += experienceChange;
    }

    public float GetSuccessChance(VocationName vocationName, float experienceRequired)
    {
        Vocation vocation = Manager_Vocation.GetVocation(vocationName);
        return vocation.GetSuccessChance(Vocations[vocation], experienceRequired);
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