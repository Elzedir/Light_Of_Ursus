using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class Item_VisualStats : Data_Class
    {
        public Sprite                    ItemIcon;
        public Mesh                      ItemMesh;
        public Material                  ItemMaterial;
        public Collider                  ItemCollider;
        public RuntimeAnimatorController ItemAnimatorController;
        public Vector3                   ItemPosition;
        public Quaternion                ItemRotation;
        public Vector3                   ItemScale;

        public Item_VisualStats(
            Sprite                    itemIcon               = null,
            Mesh                      itemMesh               = null,
            Material                  itemMaterial           = null,
            Collider                  itemCollider           = null,
            RuntimeAnimatorController itemAnimatorController = null,
            Vector3?                  itemPosition           = null,
            Quaternion?               itemRotation           = null,
            Vector3?                  itemScale              = null

        )
        {
            ItemIcon               = itemIcon;
            ItemMesh               = itemMesh;
            ItemMaterial           = itemMaterial;
            ItemCollider           = itemCollider;
            ItemAnimatorController = itemAnimatorController;
            ItemPosition           = itemPosition ?? Vector3.zero;
            ItemRotation           = itemRotation ?? Quaternion.identity;
            ItemScale              = itemScale    ?? Vector3.one;
        }

        public Item_VisualStats(Item_VisualStats other)
        {
            ItemIcon               = other.ItemIcon;
            ItemMesh               = other.ItemMesh;
            ItemMaterial           = other.ItemMaterial;
            ItemCollider           = other.ItemCollider;
            ItemAnimatorController = other.ItemAnimatorController;
            ItemPosition           = other.ItemPosition;
            ItemRotation           = other.ItemRotation;
            ItemScale              = other.ItemScale;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "ItemIcon", $"{ItemIcon}" },
                { "ItemMesh", $"{ItemMesh}" },
                { "ItemMaterial", $"{ItemMaterial}" },
                { "ItemCollider", $"{ItemCollider}" },
                { "ItemAnimatorController", $"{ItemAnimatorController}" },
                { "ItemPosition", $"{ItemPosition}" },
                { "ItemRotation", $"{ItemRotation}" },
                { "ItemScale", $"{ItemScale}" }
            };
        }
        
        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Visual Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return _dataToDisplay;
        }

        public void DisplayVisuals(GameObject go)
        {
            _addColliderToGameObject(go);
            _addMeshToGameObject(go);

            go.transform.localPosition = ItemPosition;
            go.transform.localRotation = ItemRotation;
            go.transform.localScale    = ItemScale;
        }

        void _addColliderToGameObject(GameObject itemGO)
        {
            if (ItemCollider is BoxCollider)
            {
                BoxCollider original = ItemCollider as BoxCollider;
                BoxCollider copy     = itemGO.AddComponent<BoxCollider>();
                copy.center = original.center;
                copy.size   = original.size;
            }
            else if (ItemCollider is SphereCollider)
            {
                SphereCollider original = ItemCollider as SphereCollider;
                SphereCollider copy     = itemGO.AddComponent<SphereCollider>();
                copy.center = original.center;
                copy.radius = original.radius;
            }
            else if (ItemCollider is CapsuleCollider)
            {
                CapsuleCollider original = ItemCollider as CapsuleCollider;
                CapsuleCollider copy     = itemGO.AddComponent<CapsuleCollider>();
                copy.center    = original.center;
                copy.radius    = original.radius;
                copy.height    = original.height;
                copy.direction = original.direction;
            }
            else if (ItemCollider is MeshCollider)
            {
                MeshCollider original = ItemCollider as MeshCollider;
                MeshCollider copy     = itemGO.AddComponent<MeshCollider>();
                copy.sharedMesh = original.sharedMesh;
                copy.convex     = original.convex;
            }
            else if (ItemCollider == null)
            {
                BoxCollider copy = itemGO.AddComponent<BoxCollider>();
            }
            else
            {
                Debug.LogWarning("Collider type not supported: " + ItemCollider.GetType());
            }
        }

        void _addMeshToGameObject(GameObject go)
        {
            MeshRenderer itemRenderer = go.AddComponent<MeshRenderer>();
            MeshFilter   itemFilter   = go.AddComponent<MeshFilter>();

            itemRenderer.material = ItemMaterial;
            itemFilter.mesh       = ItemMesh;
        }
    }
}