using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StationComponent_Crafter : StationComponent
{
    public virtual IEnumerator CraftItem(Actor_Base actor)
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public virtual IEnumerator CraftItemAll(Actor_Base actor)
    {
        throw new ArgumentException("Cannot use base class.");
    }
}
