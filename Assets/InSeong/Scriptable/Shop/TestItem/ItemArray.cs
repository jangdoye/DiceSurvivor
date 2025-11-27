using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TestItemArray", menuName = "Scriptable Objects/TestItemArray")]
public class TestItemArray : ScriptableObject
{
    public List<TestItem> items;
}
