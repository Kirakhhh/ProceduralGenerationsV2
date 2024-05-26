using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(LSystemTreeGenerator))]
public class LSystemTreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LSystemTreeGenerator treeGen = (LSystemTreeGenerator)target;

        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Generate"))
        {
            treeGen.GenerateTree();
        }
    }
}
