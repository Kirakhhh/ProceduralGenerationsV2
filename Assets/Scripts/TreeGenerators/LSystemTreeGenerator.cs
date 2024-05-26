using System;
using System.Collections.Generic;
using UnityEngine;

public class LSystemTreeGenerator : MonoBehaviour
{
    [Header("Settings")]


    public GameObject branch;
    public LineRenderer lineRend;
    public int randomSeed = 0;
    public int iterations = 5;
    public float defaultLength = 50f;
    public float defaultWidth = 1f;
    public float defaultAngle = 30;

    public string axiom = "X";
    public List<Rule> rules;
    private List<(Vector3, Vector3)> lines = new List<(Vector3, Vector3)>();
    private List<(float, float)> widths = new List<(float, float)>();
    private string resultString;
    private float length;
    private float angle;
    private float startWidth;
    private float endWidth;
    private Transform treeHolder;
    private Transform branchHolder;

    public void GenerateTree()
    {
        resultString = axiom;
        lines.Clear();
        widths.Clear();
        length = defaultLength;
        startWidth = defaultWidth;
        endWidth = defaultWidth;
        angle = defaultAngle;
        resultString = GenerateString(resultString);
        GenerateLines(resultString);
        DrawTree(lines);
    }

    public string GenerateString(string res)
    {
        string tmp = "";
        for (int i = 0; i < iterations; i++)
        {
            length *= 0.5f;
            foreach (char c in res)
            {
                bool found = false;
                foreach (Rule rule in rules)
                {
                    if (rule.key == c)
                    {
                        tmp += rule.result;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    tmp += c;
                }
            }
            res = tmp;
        }
        return res;
    }

    public void GenerateLines(string res)
    {
        Stack <(Vector3, float, float, float, float, float)> BackStates = new Stack<(Vector3, float, float, float, float, float)>();
        Vector3 CurrPoint = new Vector3();
        Vector3 ParrentPoint = new Vector3(0,0,0);
        float HorizontalAngle = 0;
        float VerticalAngle = 0;
        float decrease = 0;
        foreach (char c in res)
        {
            angle *= 1f + UnityEngine.Random.Range(-0.1f, 0.1f);
            switch (c)
            {
                case 'F':
                    CurrPoint = ParrentPoint + new Vector3(0, length, 0);
                    decrease = UnityEngine.Random.value;
                    length *= (decrease < 0.8f ? 0.8f : decrease);
                    startWidth = endWidth;
                    endWidth = startWidth * (decrease < 0.7f ? 0.7f : decrease);

                    Vector3 direction = CurrPoint - ParrentPoint;
                    var rotation = Quaternion.Euler(HorizontalAngle, VerticalAngle, 0);
                    //var rotation = Quaternion.Euler(HorizontalAngle, 0, 0); //поворот для 2D
                    CurrPoint = ParrentPoint + rotation * direction;

                    lines.Add((ParrentPoint, CurrPoint));
                    widths.Add((startWidth, endWidth));
                    
                    ParrentPoint = CurrPoint;
                    break;
                case '+':
                    VerticalAngle += randomBool() ? angle : -angle;
                    //HorizontalAngle -= angle; //2D
                    break;
                case '-':
                    // поворот влево вправо
                    HorizontalAngle += randomBool() ? angle : -angle;
                    //HorizontalAngle += angle;  //2D
                    break;
                case '[':
                    BackStates.Push((ParrentPoint,length, VerticalAngle, HorizontalAngle, startWidth, endWidth));
                    break;
                case ']':
                    var State = BackStates.Pop();
                    ParrentPoint = State.Item1;
                    length = State.Item2;
                    VerticalAngle = State.Item3;
                    HorizontalAngle = State.Item4;
                    startWidth = State.Item5;
                    endWidth = State.Item6;
                    break;
                default:
                    break;
            }
            angle = defaultAngle;
        }
    }

    public void DrawTree(List<(Vector3, Vector3)> lines)
    {
        treeHolder = GameObject.Find("Tree Generator").transform;

        ////удаление дочерних элементов прошлой генерации
        GameObject[] allChildren = new GameObject[transform.childCount];
        int childNum = 0;
        foreach (Transform child in transform)
        {
            allChildren[childNum] = child.gameObject;
            childNum += 1;
        }
        foreach (GameObject child in allChildren)
        {
            Destroy(child.gameObject);
        }

        branchHolder = new GameObject("Branches").transform;
        branchHolder.SetParent(treeHolder);

        for (int i = 0; i < lines.Count; i++)
        {
            GameObject instance = Instantiate(branch, new Vector3(0f, 0f, 1f), Quaternion.identity) as GameObject;
            lineRend = instance.transform.GetComponent<LineRenderer>();
            lineRend.positionCount = 2;
            lineRend.SetPosition(0, lines[i].Item1);
            lineRend.SetPosition(1, lines[i].Item2);
            lineRend.startWidth = widths[i].Item1;
            lineRend.endWidth = widths[i].Item2;
            instance.transform.SetParent(branchHolder);
        }
    }

    public static bool randomBool()
    {
        return UnityEngine.Random.value > .5;
    }

}

[Serializable]
public class Rule
{
    public char key;
    public string result;
}
