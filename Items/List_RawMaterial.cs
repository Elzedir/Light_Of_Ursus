using System.Collections.Generic;
using Priorities;
using Priority;
using Station;

namespace Items
{
    public abstract class List_RawMaterial
    {
        static Dictionary<ulong, Item_Data> _defaultRawMaterials;

        public static Dictionary<ulong, Item_Data> DefaultRawMaterials =>
            _defaultRawMaterials ??= _initialiseDefaultRawMaterials();

        static Dictionary<ulong, Item_Data> _initialiseDefaultRawMaterials()
        {
            var allRawMaterials = new Dictionary<ulong, Item_Data>();

            foreach (var material in _metals())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _woods())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _stones())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _gems())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _herbs())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _fibers())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _leathers())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _ores())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _fuels())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _flora())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _animalProducts())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            foreach (var material in _liquids())
            {
                allRawMaterials.Add(material.Key, material.Value);
            }

            return allRawMaterials;
        }

        static Dictionary<ulong, Item_Data> _metals()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1000,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1000,
                            itemType: ItemType.Raw_Material,
                            itemName: "Iron Nuggets",
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
                    1001,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1001,
                            itemType: ItemType.Raw_Material,
                            itemName: "Copper Nuggets",
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
                    1002,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1002,
                            itemType: ItemType.Raw_Material,
                            itemName: "Silver Nuggets",
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
                    1003,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1003,
                            itemType: ItemType.Raw_Material,
                            itemName: "Gold Nuggets",
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
                    1004,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1004,
                            itemType: ItemType.Raw_Material,
                            itemName: "Steel Nuggets",
                            maxStackSize: 100,
                            itemWeight: 5,
                            itemValue: 20),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1005,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1005,
                            itemType: ItemType.Raw_Material,
                            itemName: "Steel Fragments",
                            maxStackSize: 100,
                            itemWeight: 2,
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

        static Dictionary<ulong, Item_Data> _woods()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1100,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1100,
                            itemType: ItemType.Raw_Material,
                            itemName: "Log",
                            maxStackSize: 100,
                            itemWeight: 3,
                            itemValue: 5),
                        null,
                        null,
                        null,
                        null,
                        null,
                        new Item_PriorityStats(
                            new Dictionary<PriorityImportance, List<StationName>>
                            {
                                { PriorityImportance.High, new() { StationName.Sawmill } },
                                { PriorityImportance.Medium, new() { StationName.Log_Pile } },
                            }
                        ))
                },
                {
                    1101,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1101,
                            itemType: ItemType.Raw_Material,
                            itemName: "Pine Wood",
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
                    1102,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1102,
                            itemType: ItemType.Raw_Material,
                            itemName: "Birch Wood",
                            maxStackSize: 100,
                            itemWeight: 3,
                            itemValue: 6),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1103,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1103,
                            itemType: ItemType.Raw_Material,
                            itemName: "Maple Wood",
                            maxStackSize: 100,
                            itemWeight: 3,
                            itemValue: 7),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1104,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1104,
                            itemType: ItemType.Raw_Material,
                            itemName: "Mahogany Wood",
                            maxStackSize: 100,
                            itemWeight: 3,
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

        static Dictionary<ulong, Item_Data> _stones()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1200,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1200,
                            itemType: ItemType.Raw_Material,
                            itemName: "Granite",
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
                    1201,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1201,
                            itemType: ItemType.Raw_Material,
                            itemName: "Marble",
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
                    1202,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1202,
                            itemType: ItemType.Raw_Material,
                            itemName: "Limestone",
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
                    1203,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1203,
                            itemType: ItemType.Raw_Material,
                            itemName: "Slate",
                            maxStackSize: 100,
                            itemWeight: 10,
                            itemValue: 7),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1204,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1204,
                            itemType: ItemType.Raw_Material,
                            itemName: "Sandstone",
                            maxStackSize: 100,
                            itemWeight: 10,
                            itemValue: 6),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                }
            };
        }

        static Dictionary<ulong, Item_Data> _gems()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1300,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1300,
                            itemType: ItemType.Raw_Material,
                            itemName: "Diamond",
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
                    1301,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1301,
                            itemType: ItemType.Raw_Material,
                            itemName: "Ruby",
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
                    1302,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1302,
                            itemType: ItemType.Raw_Material,
                            itemName: "Sapphire",
                            maxStackSize: 100,
                            itemWeight: 1,
                            itemValue: 75),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1303,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1303,
                            itemType: ItemType.Raw_Material,
                            itemName: "Emerald",
                            maxStackSize: 100,
                            itemWeight: 1,
                            itemValue: 90),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1304,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1304,
                            itemType: ItemType.Raw_Material,
                            itemName: "Amethyst",
                            maxStackSize: 100,
                            itemWeight: 1,
                            itemValue: 70),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                }
            };
        }

        static Dictionary<ulong, Item_Data> _herbs()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1400,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1400,
                            itemType: ItemType.Raw_Material,
                            itemName: "Lavender",
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
                    1401,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1401,
                            itemType: ItemType.Raw_Material,
                            itemName: "Mint",
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
                    1402,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1402,
                            itemType: ItemType.Raw_Material,
                            itemName: "Basil",
                            maxStackSize: 100,
                            itemWeight: 0.1f,
                            itemValue: 6),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1403,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1403,
                            itemType: ItemType.Raw_Material,
                            itemName: "Rosemary",
                            maxStackSize: 100,
                            itemWeight: 0.1f,
                            itemValue: 7),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1404,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1404,
                            itemType: ItemType.Raw_Material,
                            itemName: "Thyme",
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
        }

        static Dictionary<ulong, Item_Data> _fibers()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1500,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1500,
                            itemType: ItemType.Raw_Material,
                            itemName: "Cotton",
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
                    1501,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1501,
                            itemType: ItemType.Raw_Material,
                            itemName: "Wool",
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
                    1502,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1502,
                            itemType: ItemType.Raw_Material,
                            itemName: "Silk",
                            maxStackSize: 100,
                            itemWeight: 0.1f,
                            itemValue: 10),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1503,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1503,
                            itemType: ItemType.Raw_Material,
                            itemName: "Hemp",
                            maxStackSize: 100,
                            itemWeight: 0.3f,
                            itemValue: 4),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1504,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1504,
                            itemType: ItemType.Raw_Material,
                            itemName: "Linen",
                            maxStackSize: 100,
                            itemWeight: 0.2f,
                            itemValue: 5),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                }
            };
        }

        static Dictionary<ulong, Item_Data> _leathers()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1600,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1600,
                            itemType: ItemType.Raw_Material,
                            itemName: "Cowhide",
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
                    1601,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1601,
                            itemType: ItemType.Raw_Material,
                            itemName: "Deerhide",
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
                    1602,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1602,
                            itemType: ItemType.Raw_Material,
                            itemName: "Snakehide",
                            maxStackSize: 100,
                            itemWeight: 2,
                            itemValue: 25),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1603,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1603,
                            itemType: ItemType.Raw_Material,
                            itemName: "Alligatorhide",
                            maxStackSize: 100,
                            itemWeight: 2,
                            itemValue: 30),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1604,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1604,
                            itemType: ItemType.Raw_Material,
                            itemName: "Dragonhide",
                            maxStackSize: 100,
                            itemWeight: 2,
                            itemValue: 50),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                }
            };
        }

        static Dictionary<ulong, Item_Data> _ores()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1700,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1700,
                            itemType: ItemType.Raw_Material,
                            itemName: "Iron Ore",
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
                    1701,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1701,
                            itemType: ItemType.Raw_Material,
                            itemName: "Copper Ore",
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
                    1702,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1702,
                            itemType: ItemType.Raw_Material,
                            itemName: "Gold Ore",
                            maxStackSize: 100,
                            itemWeight: 5,
                            itemValue: 20),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1703,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1703,
                            itemType: ItemType.Raw_Material,
                            itemName: "Silver Ore",
                            maxStackSize: 100,
                            itemWeight: 5,
                            itemValue: 18),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1704,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1704,
                            itemType: ItemType.Raw_Material,
                            itemName: "Mithril Ore",
                            maxStackSize: 100,
                            itemWeight: 5,
                            itemValue: 25),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                }
            };
        }

        static Dictionary<ulong, Item_Data> _fuels()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1800,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1800,
                            itemType: ItemType.Raw_Material,
                            itemName: "Coal",
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
                    1801,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1801,
                            itemType: ItemType.Raw_Material,
                            itemName: "Oil",
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
                    1802,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1802,
                            itemType: ItemType.Raw_Material,
                            itemName: "Firewood",
                            maxStackSize: 100,
                            itemWeight: 10,
                            itemValue: 3),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1803,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1803,
                            itemType: ItemType.Raw_Material,
                            itemName: "Charcoal",
                            maxStackSize: 100,
                            itemWeight: 10,
                            itemValue: 4),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1804,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1804,
                            itemType: ItemType.Raw_Material,
                            itemName: "Magic Crystals",
                            maxStackSize: 100,
                            itemWeight: 1,
                            itemValue: 50),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                }
            };
        }

        static Dictionary<ulong, Item_Data> _flora()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1900,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1900,
                            itemType: ItemType.Raw_Material,
                            itemName: "Flower",
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
                    1901,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1901,
                            itemType: ItemType.Raw_Material,
                            itemName: "Root",
                            maxStackSize: 100,
                            itemWeight: 0.2f,
                            itemValue: 4),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1902,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1902,
                            itemType: ItemType.Raw_Material,
                            itemName: "Leaf",
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
                    1903,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1903,
                            itemType: ItemType.Raw_Material,
                            itemName: "Berry",
                            maxStackSize: 100,
                            itemWeight: 0.1f,
                            itemValue: 1),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    1904,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1904,
                            itemType: ItemType.Raw_Material,
                            itemName: "Mushroom",
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
        }

        static Dictionary<ulong, Item_Data> _animalProducts()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    2000,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2000,
                            itemType: ItemType.Raw_Material,
                            itemName: "Bone",
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
                    2001,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2001,
                            itemType: ItemType.Raw_Material,
                            itemName: "Feather",
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
                    2002,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2002,
                            itemType: ItemType.Raw_Material,
                            itemName: "Claw",
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
                    2003,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2003,
                            itemType: ItemType.Raw_Material,
                            itemName: "Scale",
                            maxStackSize: 100,
                            itemWeight: 0.5f,
                            itemValue: 4),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    2004,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2004,
                            itemType: ItemType.Raw_Material,
                            itemName: "Meat",
                            maxStackSize: 100,
                            itemWeight: 1,
                            itemValue: 6),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                }
            };
        }

        static Dictionary<ulong, Item_Data> _liquids()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    2100,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2100,
                            itemType: ItemType.Raw_Material,
                            itemName: "Water",
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
                    2101,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2101,
                            itemType: ItemType.Raw_Material,
                            itemName: "Oil",
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
                    2102,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2102,
                            itemType: ItemType.Raw_Material,
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
                    2103,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2103,
                            itemType: ItemType.Raw_Material,
                            itemName: "Alchemical Reagent",
                            maxStackSize: 100,
                            itemWeight: 1,
                            itemValue: 10),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                {
                    2104,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2104,
                            itemType: ItemType.Raw_Material,
                            itemName: "Blood",
                            maxStackSize: 100,
                            itemWeight: 1,
                            itemValue: 8),
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
}