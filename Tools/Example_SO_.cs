using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CreateAssetMenu(fileName = "BaseTest_SO", menuName = "SOList/BaseTest_SO")]
    [Serializable]
    public class Example_SO_ : Base_SO_Test<Test_Data>
    {
        public Base_Object<Test_Data>[] Tests                     => BaseObjects;
        public Base_Object<Test_Data>   GetTest_Data(uint testID) => GetBaseObject_Master(testID);

        public override uint GetBaseObjectID(int id) => Tests[id].DataObject.TestID;

        public void UpdateTest(uint testID, Test_Data test_Data) => UpdateBaseObject(testID, test_Data);
        public void UpdateAllTests(Dictionary<uint, Test_Data> allTests) => UpdateAllBaseObjects(allTests);

        public void PopulateSceneTests()
        {

        }

        protected override Dictionary<uint, Base_Object<Test_Data>> _populateDefaultBaseObjects()
        {
            var tests = new Dictionary<uint, Test_Data>
            {
                {
                    1,
                    new Test_Data(1, "Test 1", new List<string> { "A", "B" },
                        new List<Smaller_Test_Data>
                            { new Smaller_Test_Data("Smaller Test 1", 1), new Smaller_Test_Data("Smaller Test 2", 2) })
                },
                {
                    456,
                    new Test_Data(456, "Test 2", new List<string> { "QEE", "QWE" },
                        new List<Smaller_Test_Data>
                            { new Smaller_Test_Data("QWEQ", 3), new Smaller_Test_Data("QWEQWE", 4) })
                },
                {
                    878596,
                    new Test_Data(878596, "Test 3", new List<string> { "TYYYY", "TYYYYR" },
                        new List<Smaller_Test_Data>
                            { new Smaller_Test_Data("YYYTTYY", 6), new Smaller_Test_Data("YTYYYT", 5) })
                },
                {
                    4567,
                    new Test_Data(4567, "Test 4", new List<string> { "AAAFFG", "aAAAAF" },
                        new List<Smaller_Test_Data>
                            { new Smaller_Test_Data("AAAFFA", 7), new Smaller_Test_Data("FAFAFAAA", 8) })
                },
                {
                    56780,
                    new Test_Data(56780, "Test 5", new List<string> { "HGHGH", "HGGHGG" },
                        new List<Smaller_Test_Data>
                            { new Smaller_Test_Data("HHHJHGGH", 9), new Smaller_Test_Data("HHJJGGG", 10) })
                }
            };

            return _convertDictionaryToBaseObject(tests);
        }

        protected override Dictionary<uint, Base_Object<Test_Data>> _convertDictionaryToBaseObject(
            Dictionary<uint, Test_Data> ability_Masters)
        {
            return ability_Masters.ToDictionary(kvp => kvp.Key,
                kvp => new Base_Object<Test_Data>(kvp.Key, GetDataToDisplay(kvp.Value), kvp.Value,
                    $"{kvp.Key}{kvp.Value.TestName}"));
        }

        protected override Base_Object<Test_Data> _convertToBaseObject(Test_Data ability_Master)
        {
            return new Base_Object<Test_Data>(ability_Master.TestID, GetDataToDisplay(ability_Master), ability_Master,
                $"{ability_Master.TestID}{ability_Master.TestName}");
        }

        static uint _lastUnusedTestID = 1;

        public uint GetUnusedTestID()
        {
            while (BaseObjectIndexLookup.ContainsKey(_lastUnusedTestID))
            {
                _lastUnusedTestID++;
            }

            return _lastUnusedTestID;
        }

        enum TestCategories
        {
            FullIdentification,
            ExampleCategory,
            AnotherExampleCategory,
            Prosperity,
            Population
        }

        public override Dictionary<uint, DataToDisplay> GetDataToDisplay(Test_Data actor_Data)
        {
            return new Dictionary<uint, DataToDisplay>
            {
                {
                    (uint)TestCategories.FullIdentification, new DataToDisplay(
                        data: new List<string>
                        {
                            $"Test ID: {actor_Data.TestID}",
                            $"Test Name: {actor_Data.TestName}"
                        },
                        dataDisplayType: DataDisplayType.Item)
                },
                {


                    (uint)TestCategories.ExampleCategory,
                    new DataToDisplay(
                        data: new List<string>(actor_Data.TestList),
                        dataDisplayType: DataDisplayType.List)
                },
                {

                    (uint)TestCategories.AnotherExampleCategory,
                    new DataToDisplay(
                        data: actor_Data.SmallerTestList.Select(smallerTest =>
                            $"{smallerTest.SmallerTestName}, {smallerTest.SmallerTestID}").ToList(),
                        dataDisplayType: DataDisplayType.SelectableList)
                },
                {

                    (uint)TestCategories.Prosperity,
                    new DataToDisplay(
                        data: new List<string>
                        {
                            $"Prosperity 1: Prosperity1",
                            $"Prosperity 2: Prosperity2"
                        },
                        dataDisplayType: DataDisplayType.Item)
                },
                {

                    (uint)TestCategories.Population,
                    new DataToDisplay(
                        data: new List<string>
                        {
                            $"Population 1: Population1",
                            $"Population 2: Population2"
                        },
                        dataDisplayType: DataDisplayType.Item)
                }
            };
        }
    }

    [CustomEditor(typeof(Example_SO_))]
    public class _SOEditor : Base_SOEditor<Test_Data>
    {
        public override Base_SO_Test<Test_Data> SO => _so ??= (Example_SO_)target;
    }

    [Serializable]
    public class Test_Data
    {
        public readonly uint                    TestID;
        public readonly string                  TestName;
        public          List<string>            TestList;
        public          List<Smaller_Test_Data> SmallerTestList;

        public Test_Data(uint testID, string testName, List<string> testList, List<Smaller_Test_Data> smallerTestList)
        {
            TestID          = testID;
            TestName        = testName;
            TestList        = testList;
            SmallerTestList = smallerTestList;
        }
    }

    [Serializable]
    public class Smaller_Test_Data
    {
        public string SmallerTestName;
        public int    SmallerTestID;

        public Smaller_Test_Data(string smallerTestName, int smallerTestID)
        {
            SmallerTestName = smallerTestName;
            SmallerTestID   = smallerTestID;
        }
    }
}