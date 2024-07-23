using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Dynasty
{
    public static HashSet<Dynasty> AllDynasties = new();
    public static HashSet<string> AllFamilyNames = new();

    public static void OnSceneLoad()
    {
        _initialiseOriginalFamilyNames();
    }

    static void _initialiseOriginalFamilyNames()
    {
        new Family
            (
                familyName: "TestFamily",
                familyMotto: "This is a test",
                familyFoundingDate: new Date(1, 1, 50),
                allFamilyMembers: new HashSet<FamilyMember>
                {
                    new FamilyMember(100, "TestDaddy"),
                    new FamilyMember(101, "TestMommy"),
                    new FamilyMember(102, "TestBaby")
                },
                null
            );
    }
}

public class Dynasty
{
    public string DynastyName;
    public string DynastyMotto;
    public Date DynastyFoundingDate;

    public HashSet<Family> AllDynastyFamilies = new();

    public Dynasty(string dynastyName, string dynastyMotto, Date dynastyFoundingDate, Family foundingFamily)
    {
        DynastyName = dynastyName;
        DynastyMotto = dynastyMotto;
        DynastyFoundingDate = dynastyFoundingDate;

        AllDynastyFamilies.Add(foundingFamily);

        if (!Manager_Dynasty.AllDynasties.Add(this)) throw new ArgumentException($"Dynasty: {DynastyName} already exists in the dynasty list.");
    }

    public void AddFamilyToDynasty(Family family)
    {
        if (family == null) throw new ArgumentException($"Family: {family} does not exist.");
        if (!AllDynastyFamilies.Add(family)) throw new ArgumentException($"Family name: {family.FamilyName} already exists in the family list.");
    }

    public void RemoveFamilyFromDynasty(string familyName)
    {
        if (!AllDynastyFamilies.Remove(AllDynastyFamilies.First(f => f.FamilyName == familyName))) throw new ArgumentException($"Family name: {familyName} does not exist in the family list.");
    }
}

public class Family
{
    public string FamilyName;
    public string FamilyMotto;
    public Date FamilyFoundingDate;
    public Dynasty Dynasty;

    public HashSet<FamilyMember> AllFamilyMembers = new();

    public Family(string familyName, string familyMotto, Date familyFoundingDate, HashSet<FamilyMember> allFamilyMembers, Dynasty dynasty) 
    {
        // For now, does not allow the same string of words, however can change later on to be a combination of things.

        if (!Manager_Dynasty.AllFamilyNames.Add(familyName)) throw new ArgumentException($"Family name: {familyName} has already been used");

        FamilyName = familyName;
        FamilyMotto = familyMotto;
        FamilyFoundingDate = familyFoundingDate;
        AllFamilyMembers = allFamilyMembers;

        Dynasty = dynasty != null ? dynasty : new Dynasty(FamilyName, FamilyMotto, FamilyFoundingDate, this); 
    }

    public void AddFamilyMember(FamilyMember familyMember)
    {
        if (familyMember == null) throw new ArgumentException($"Family member: {familyMember} does not exist.");
        if (!AllFamilyMembers.Add(familyMember)) throw new ArgumentException($"Family member ID: {familyMember.MemberActorID} already exists in the family member list.");
    }

    public void RemoveFamilyMember(int familyMemberID)
    {
        if (!AllFamilyMembers.Remove(AllFamilyMembers.First(fm => fm.MemberActorID == familyMemberID))) throw new ArgumentException($"Family member ID: {familyMemberID} does not exist in the family member list.");
    }
}

public class FamilyMember
{
    public int MemberActorID;
    public string MemberName;
    
    public FamilyMember(int memberActorID, string memberName)
    {
        MemberName = memberName;
        MemberActorID = memberActorID;
    }
}

