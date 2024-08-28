using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Title
{
    public static Dictionary<TitleName, Title> AllTitles = new();
    public static Title GetTitle(TitleName titleName) => AllTitles[titleName];
    public static void InitialiseTitles()
    {
        _vocationTitles();
    }

    static void _vocationTitles()
    {
        _addTitle(new Title(
            TitleName.None
            ));
    }

    static void _addTitle(Title title)
    {
        if (title == null && AllTitles.ContainsKey(title.TitleName)) throw new ArgumentException($"Title: {title} is null or exists in AllTitles.");
        
        AllTitles.Add(title.TitleName, title);
    }
}

public enum TitleName
{
    None,
    The_Noob
}

public class Title
{
    public TitleName TitleName;

    public Title(TitleName titleName)
    {
        TitleName = titleName;
    }
}
