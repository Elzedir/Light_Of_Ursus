using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable_MouseMaze : Collectable
{
    protected override void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.name != "Focus") return;
        _spawner.CollectableCollected(this);
    }
}
