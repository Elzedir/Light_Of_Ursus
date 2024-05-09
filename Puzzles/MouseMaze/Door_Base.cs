using System.Collections.Generic;
using UnityEngine;

public class Door_Base : MonoBehaviour
{
    public MouseMazeColour MouseMazeDoorColour { get; private set; }
    public Color DoorColor { get; private set; }

    public void InitialiseDoor(MouseMazeColour doorColour, Color color, Cell_MouseMaze cell)
    {
        MouseMazeDoorColour = doorColour;
        DoorColor = color;
    }

    GameObject CreateDoor(string name)
    {
        GameObject doorGO = new GameObject($"Door_{name}");
        doorGO.transform.parent = transform;
        Door door = doorGO.AddComponent<Door>();
        door.InitialiseDoor(this, DoorColor);
        return doorGO;
    }
}
