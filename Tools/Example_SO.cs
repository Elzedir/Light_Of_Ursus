using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CreateAssetMenu(fileName = "BaseTest_SO", menuName = "SOList/BaseTest_SO")]
    [Serializable]
    public class Example_SO : Data_SO<Test_Data>
    {
        public Object_Data<Test_Data>[] Tests                     => Objects_Data;
        public Object_Data<Test_Data>   GetTest_Data(uint testID) => GetObject_Data(testID);

        public override uint GetDataObjectID(int id) => Tests[id].DataObject.TestID;

        public void UpdateTest(uint testID, Test_Data test_Data) => UpdateDataObject(testID, test_Data);
        public void UpdateAllTests(Dictionary<uint, Test_Data> allTests) => UpdateAllDataObjects(allTests);

        public override void PopulateSceneData()
        {

        }

        protected override Dictionary<uint, Object_Data<Test_Data>> _populateDefaultDataObjects()
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

            return _convertDictionaryToDataObject(tests);
        }

        protected override Object_Data<Test_Data> _convertToDataObject(Test_Data data)
        {
            return new Object_Data<Test_Data>(
                dataObjectID: data.TestID, 
                dataObject: data,
                dataObjectTitle: $"{data.TestID}{data.TestName}",
                null);
        }

        static uint _lastUnusedTestID = 1;

        public uint GetUnusedTestID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedTestID))
            {
                _lastUnusedTestID++;
            }

            return _lastUnusedTestID;
        }

        // enum TestCategories
        // {
        //     FullIdentification,
        //     ExampleCategory,
        //     AnotherExampleCategory,
        //     Prosperity,
        //     Population
        // }

        // public override Dictionary<uint, DataSO_Object> GetAllDataCategories(Test_Data data)
        // {
        //     return new Dictionary<uint, DataSO_Object>
        //     {
        //         {
        //             (uint)TestCategories.FullIdentification, new DataSO_Object(
        //                 title: "Full Identification",
        //                 dataDisplay: new Dictionary<string, Action>
        //                 {
        //                     $"Test ID: {data.TestID}",
        //                     $"Test Name: {data.TestName}"
        //                 },
        //                 dataDisplayType: DataDisplayType.Item)
        //         },
        //         {
        //
        //
        //             (uint)TestCategories.ExampleCategory, new DataSO_Object(
        //                 title: "Example Category",
        //                 dataDisplay: new List<string>(data.TestList),
        //                 dataDisplayType: DataDisplayType.List)
        //         },
        //         {
        //
        //             (uint)TestCategories.AnotherExampleCategory, new DataSO_Object(
        //                 title: "Another Example Category",
        //                 dataDisplay: data.SmallerTestList.Select(smallerTest =>
        //                     $"{smallerTest.SmallerTestName}, {smallerTest.SmallerTestID}").ToList(),
        //                 dataDisplayType: DataDisplayType.SelectableList)
        //         },
        //         {
        //
        //             (uint)TestCategories.Prosperity, new DataSO_Object(
        //                 title: "Prosperity",
        //                 dataDisplay: new List<string>
        //                 {
        //                     $"Prosperity 1: Prosperity1",
        //                     $"Prosperity 2: Prosperity2"
        //                 },
        //                 dataDisplayType: DataDisplayType.Item)
        //         },
        //         {
        //
        //             (uint)TestCategories.Population, new DataSO_Object(
        //                 title: "Population",
        //                 dataDisplay: new List<string>
        //                 {
        //                     $"Population 1: Population1",
        //                     $"Population 2: Population2"
        //                 },
        //                 dataDisplayType: DataDisplayType.Item)
        //         }
        //     };
        //}
    }

    [CustomEditor(typeof(Example_SO))]
    public class Example_SOEditor : Data_SOEditor<Test_Data>
    {
        public override Data_SO<Test_Data> SO => _so ??= (Example_SO)target;
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