using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSlot { None, Head, Neck, Chest, LeftHand, RightHand, Ring, Waist, Legs, Feet }

public class Equipment_Base : MonoBehaviour
{
    public EquipmentSlot EquipmentSlot { get; protected set; }
    public MeshFilter MeshFilter { get; protected set; }
    public MeshRenderer MeshRenderer { get; protected set; }
    public Collider Collider { get; protected set; }

    protected virtual void Awake()
    {
        MeshFilter = GetComponent<MeshFilter>();
        MeshRenderer = GetComponent<MeshRenderer>();
        Collider = GetComponent<Collider>();
    }
}
