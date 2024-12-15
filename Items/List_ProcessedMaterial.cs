using System.Collections.Generic;
using Priority;
using Station;
using UnityEngine;

namespace Items
{
    public enum ProcessedMaterialName
    {
        None,
        Plank
    }

    public abstract class List_ProcessedMaterial
    {
        public static Dictionary<uint, Item_Data> GetAllDefaultProcessedMaterials()
        {
            var allProcessedMaterials = new Dictionary<uint, Item_Data>();

            foreach (var material in _metals)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _woods)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _stones)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _gems)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _herbs)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _fibers)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _leathers)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _ores)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _fuels)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _flora)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _animalProducts)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _liquids)
            {
                allProcessedMaterials.Add(material.Key, material.Value);
            }

            return allProcessedMaterials;
        }

        static readonly Dictionary<uint, Item_Data> _metals = new()
        {
            {
                2200,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2200,
                        itemType: ItemType.Processed_Material,
                        itemName: "Iron Ingot",
                        maxStackSize: 100,
                        itemWeight: 5,
                        itemValue: 15),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },

            {
                2201,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2201,
                        itemType: ItemType.Processed_Material,
                        itemName: "Copper Ingot",
                        maxStackSize: 100,
                        itemWeight: 5,
                        itemValue: 10),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },

            {
                2202,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2202,
                        itemType: ItemType.Processed_Material,
                        itemName: "Silver Ingot",
                        maxStackSize: 100,
                        itemWeight: 5,
                        itemValue: 25),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },

            {
                2203,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2202,
                        itemType: ItemType.Processed_Material,
                        itemName: "Gold Ingot",
                        maxStackSize: 100,
                        itemWeight: 5,
                        itemValue: 50),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },

            {
                2204,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2204,
                        itemType: ItemType.Processed_Material,
                        itemName: "Steel Ingot",
                        maxStackSize: 100,
                        itemWeight: 5,
                        itemValue: 20),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _woods = new()
        {
            {
                2300,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2300,
                        itemType: ItemType.Processed_Material,
                        itemName: "Plank",
                        maxStackSize: 100,
                        itemWeight: 3,
                        itemValue: 5),
                    new Item_VisualStats(
                        //itemMesh: GameObject.Find("TestPlank").GetComponent<MeshFilter>().mesh,
                        itemMaterial: Resources.Load<Material>("Materials/Material_Yellow"),
                        itemScale: new Vector3(0.1f, 1, 0.2f)),
                    null,
                    null,
                    null,
                    null,
                    new Item_PriorityStats(
                        new Dictionary<PriorityImportance, List<StationName>>
                        {
                            { PriorityImportance.High, new() { StationName.Log_Pile } },
                            { PriorityImportance.Low, new() { StationName.Sawmill } },
                        }))
            },
            {
                2301,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2301,
                        itemType: ItemType.Processed_Material,
                        itemName: "Timber",
                        maxStackSize: 100,
                        itemWeight: 3,
                        itemValue: 4),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2302,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2302,
                        itemType: ItemType.Processed_Material,
                        itemName: "Board",
                        maxStackSize: 100,
                        itemWeight: 3,
                        itemValue: 6),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _stones = new()
        {
            {
                2400,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2400,
                        itemType: ItemType.Processed_Material,
                        itemName: "Cut Granite",
                        maxStackSize: 100,
                        itemWeight: 10,
                        itemValue: 8),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2401,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2401,
                        itemType: ItemType.Processed_Material,
                        itemName: "Cut Marble",
                        maxStackSize: 100,
                        itemWeight: 10,
                        itemValue: 15),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2402,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2402,
                        itemType: ItemType.Processed_Material,
                        itemName: "Cut Limestone",
                        maxStackSize: 100,
                        itemWeight: 10,
                        itemValue: 5),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _gems = new()
        {
            {
                2500,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2500,
                        itemType: ItemType.Processed_Material,
                        itemName: "Cut Diamond",
                        maxStackSize: 100,
                        itemWeight: 1,
                        itemValue: 100),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2501,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2501,
                        itemType: ItemType.Processed_Material,
                        itemName: "Cut Ruby",
                        maxStackSize: 100,
                        itemWeight: 1,
                        itemValue: 80),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2502,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2502,
                        itemType: ItemType.Processed_Material,
                        itemName: "Cut Sapphire",
                        maxStackSize: 100,
                        itemWeight: 1,
                        itemValue: 75),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _herbs = new()
        {
            {
                2600,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2600,
                        itemType: ItemType.Processed_Material,
                        itemName: "Dried Lavender",
                        maxStackSize: 100,
                        itemWeight: 0.1f,
                        itemValue: 5),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2601,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2601,
                        itemType: ItemType.Processed_Material,
                        itemName: "Dried Mint",
                        maxStackSize: 100,
                        itemWeight: 0.1f,
                        itemValue: 4),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2602,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2602,
                        itemType: ItemType.Processed_Material,
                        itemName: "Dried Basil",
                        maxStackSize: 100,
                        itemWeight: 0.1f,
                        itemValue: 6),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _fibers = new()
        {
            {
                2700,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2700,
                        itemType: ItemType.Processed_Material,
                        itemName: "Spun Cotton",
                        maxStackSize: 100,
                        itemWeight: 0.2f,
                        itemValue: 2),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2701,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2701,
                        itemType: ItemType.Processed_Material,
                        itemName: "Spun Wool",
                        maxStackSize: 100,
                        itemWeight: 0.5f,
                        itemValue: 3),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2702,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2702,
                        itemType: ItemType.Processed_Material,
                        itemName: "Spun Silk",
                        maxStackSize: 100,
                        itemWeight: 0.1f,
                        itemValue: 10),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _leathers = new()
        {
            {
                2800,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2800,
                        itemType: ItemType.Processed_Material,
                        itemName: "Tanned Cowhide",
                        maxStackSize: 100,
                        itemWeight: 2,
                        itemValue: 15),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2801,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2801,
                        itemType: ItemType.Processed_Material,
                        itemName: "Tanned Deerhide",
                        maxStackSize: 100,
                        itemWeight: 2,
                        itemValue: 20),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2802,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2802,
                        itemType: ItemType.Processed_Material,
                        itemName: "Tanned Snakehide",
                        maxStackSize: 100,
                        itemWeight: 2,
                        itemValue: 25),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _ores = new()
        {
            {
                2900,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2900,
                        itemType: ItemType.Processed_Material,
                        itemName: "Iron Powder",
                        maxStackSize: 100,
                        itemWeight: 5,
                        itemValue: 10),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2901,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2901,
                        itemType: ItemType.Processed_Material,
                        itemName: "Copper Powder",
                        maxStackSize: 100,
                        itemWeight: 5,
                        itemValue: 8),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                2902,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 2902,
                        itemType: ItemType.Processed_Material,
                        itemName: "Gold Powder",
                        maxStackSize: 100,
                        itemWeight: 5,
                        itemValue: 20),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _fuels = new()
        {
            {
                3000,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3000,
                        itemType: ItemType.Processed_Material,
                        itemName: "Refined Coal",
                        maxStackSize: 100,
                        itemWeight: 10,
                        itemValue: 5),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                3001,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3001,
                        itemType: ItemType.Processed_Material,
                        itemName: "Refined Oil",
                        maxStackSize: 100,
                        itemWeight: 10,
                        itemValue: 8),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                3002,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3002,
                        itemType: ItemType.Processed_Material,
                        itemName: "Charcoal Briquettes",
                        maxStackSize: 100,
                        itemWeight: 10,
                        itemValue: 4),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _flora = new()
        {
            {
                3100,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3100,
                        itemType: ItemType.Processed_Material,
                        itemName: "Processed Flower Petals",
                        maxStackSize: 100,
                        itemWeight: 0.1f,
                        itemValue: 3),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                3101,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3101,
                        itemType: ItemType.Processed_Material,
                        itemName: "Processed Roots",
                        maxStackSize: 100,
                        itemWeight: 0.2f,
                        itemValue: 4),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _animalProducts = new()
        {
            {
                3200,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3200,
                        itemType: ItemType.Processed_Material,
                        itemName: "Bone Meal",
                        maxStackSize: 100,
                        itemWeight: 2,
                        itemValue: 5),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                3201,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3201,
                        itemType: ItemType.Processed_Material,
                        itemName: "Feather Quills",
                        maxStackSize: 100,
                        itemWeight: 0.1f,
                        itemValue: 2),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                3202,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3202,
                        itemType: ItemType.Processed_Material,
                        itemName: "Claw Powder",
                        maxStackSize: 100,
                        itemWeight: 0.5f,
                        itemValue: 3),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };

        static readonly Dictionary<uint, Item_Data> _liquids = new()
        {
            {
                3300,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3300,
                        itemType: ItemType.Processed_Material,
                        itemName: "Purified Water",
                        maxStackSize: 100,
                        itemWeight: 1,
                        itemValue: 1),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                3301,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3301,
                        itemType: ItemType.Processed_Material,
                        itemName: "Refined Oil",
                        maxStackSize: 100,
                        itemWeight: 1,
                        itemValue: 2),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                3302,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3302,
                        itemType: ItemType.Processed_Material,
                        itemName: "Potion Base",
                        maxStackSize: 100,
                        itemWeight: 1,
                        itemValue: 5),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            },
            {
                3303,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 3303,
                        itemType: ItemType.Processed_Material,
                        itemName: "Concentrated Reagent",
                        maxStackSize: 100,
                        itemWeight: 1,
                        itemValue: 10),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            }
        };
    }
}