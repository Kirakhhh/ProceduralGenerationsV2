using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpaceColonizationGenerator))]
public class SpaceColonizationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpaceColonizationGenerator spaceColGen = (SpaceColonizationGenerator)target;

        if (DrawDefaultInspector())
        {

        }
        if (GUILayout.Button("Generate"))
        {
            spaceColGen.GenerateTree();
        }
    }
}
