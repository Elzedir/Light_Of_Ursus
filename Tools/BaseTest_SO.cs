using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    [CreateAssetMenu(fileName = "BaseTest_SO", menuName = "SOList/BaseTest_SO")]
    [Serializable]
    public class BaseTest_SO : Base_SO<Test_Data>
    {
        a
    
        // Find a way to pass through each individual data type instead of Base_Object, but still retain its baseObjectIDs and AllDataCategories.
        // Can't make it a child since the data types will already be children of other things, so find a way to make it pass through with <T>.
        
        public Base_Object<Test_Data>[] Tests                          => BaseObjects;
        public Test_Data       GetTest_Data(uint      testID) => GetBaseObject_Master(testID);

        public override uint GetBaseObjectID(int id) => Tests[id].TestID;

        public void UpdateTest(uint testID, Test_Data test_Data) => UpdateBaseObject(testID, test_Data);
        public void UpdateAllTests(Dictionary<uint, Test_Data> allTests) => UpdateAllBaseObjects(allTests);

        public void PopulateSceneTests()
        {
            
        }

        protected override Dictionary<uint, Test_Data> _populateDefaultBaseObjects()
        {
            return new Dictionary<uint, Test_Data>
            {
                { 1, new Test_Data(1, "Test 1") },
                { 2, new Test_Data(2, "Test 2") },
                { 3, new Test_Data(3, "Test 3") },
                { 4, new Test_Data(4, "Test 4") },
                { 5, new Test_Data(5, "Test 5") }
            };
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
    }
    
    public class Test_SOEditor : Base_SOEditor<BaseTest_SO>
    {
        
    }

    public class Test_Data
    {
        public readonly uint TestID;
        public readonly string TestName;

        public Test_Data(uint testID, string testName)
        {
            TestID        = testID;
            TestName = testName;
        }
    }
}