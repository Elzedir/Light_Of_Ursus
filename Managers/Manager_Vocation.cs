using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum VocationName
{
    None,
    Farming,
    Logging,
    Sawying,
    Mining
}

public class Manager_Vocation
{
    public static List<Vocation> AllVocations = new();

    private void InitialiseVocations()
    {
        _logger();
        _lumberjack();
    }

    void _logger()
    {
        AllVocations.Add(new Vocation(
            vocationName: VocationName.Logging,
            vocationDescription: "A logger",
            allVocationTitles: new Dictionary<int, VocationTitle>
            {
                { 1, VocationTitle.Novice },
                { 100, VocationTitle.Apprentice },
                { 1000, VocationTitle.Journeyman },
                { 10000, VocationTitle.Master },
            }
            ));
    }

    void _lumberjack()
    {
        AllVocations.Add(new Vocation(
            vocationName: VocationName.Sawying,
            vocationDescription: "A sawyer",
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

    public VocationTitle GetVocationTitle(float currentVocationExperience)
    {
        return AllVocationTitles
            .Where(kv => currentVocationExperience >= kv.Key)
            .OrderByDescending(kv => kv.Key)
            .Select(kv => kv.Value)
            .FirstOrDefault();
    }
}