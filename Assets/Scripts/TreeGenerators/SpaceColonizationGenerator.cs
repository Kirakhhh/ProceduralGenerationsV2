using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpaceColonizationGenerator : MonoBehaviour
{
    [Header("Settings Algorithm")]
    public int attractorCount = 1000;
    public float influenceRadius = 5.0f;
    public float killRadius = 1.0f;
    public float growthStep = 0.2f;
    public float spawnFigureShiftY = 5f;
    public enum TreeForm { Cube, Sphere, Cone };
    public TreeForm form;

    [Header("Settings Sphere form")]
    [Range(0f, 100f)]
    public float radiusSphere = 5f;

    [Header("Settings Cone form")]
    [Range(0f, 100f)]
    public float radiusCone = 5f;
    public float heightCone = 20f;

    [Header("Settings Cube form")]
    public float boundX = 10f;
    public float boundY = 20f;
    public float boundZ = 10f;

    [Header("Mesh generation")]
    [Range(0, 20)]
    public int radialSubdivisions = 10;
    [Range(0f, 1f)]
    public float extremitiesWidth = 0.05f;
    [Range(0f, 5f)]
    public float widthGrowth = 2f;


    public GameObject dote;

    private List<Attractor> attractors;
    private List<TreeNode> nodes;

    private Transform treeHolder;
    private Transform attractorHolder;

    private MeshFilter _filter;

    public void GenerateTree()
    {
        GenerateAttractors();
        GrowTree();
        ToMesh();

    }

    void GenerateAttractors()
    {
        attractors = new List<Attractor>();
        switch (form){ 
            case TreeForm.Cube:
                for (int i = 0; i < attractorCount; i++)
                {
                    Vector3 position = new Vector3(Random.Range(-boundX, boundX), Random.Range(-boundY + spawnFigureShiftY, boundY + spawnFigureShiftY), Random.Range(-boundZ, boundZ));
                    attractors.Add(new Attractor(position));
                }
                break;
            case TreeForm.Sphere:
                for (int i = 0; i < attractorCount; i++)
                {
                    float radius = Random.Range(0f, 1f);
                    radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI / 2f), 0.8f);
                    radius *= radiusSphere;

                    float alpha = Random.Range(0f, Mathf.PI);
                    float theta = Random.Range(0f, Mathf.PI * 2f);

                    Vector3 pt = new Vector3(
                        radius * Mathf.Cos(theta) * Mathf.Sin(alpha),
                        radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
                        radius * Mathf.Cos(alpha)
                    );

                    pt += transform.position + Vector3.up * spawnFigureShiftY;
                    attractors.Add(new Attractor(pt));
                }
                break;
            case TreeForm.Cone:
                for (int i = 0; i < attractorCount; i++)
                {
                    float t = Random.Range(0.0f, 1.0f);
                    float angle = Random.Range(0.0f, Mathf.PI * 2);
                    float shift = Random.Range(0.0f, 1.0f);

                    float currentRadius = radiusCone * (1 - t);
                    float x = currentRadius * Mathf.Cos(angle) * shift;
                    float z = currentRadius * Mathf.Sin(angle) * shift;
                    float y = t * heightCone;

                    attractors.Add(new Attractor(new Vector3(x, y, z)+ new Vector3(0, spawnFigureShiftY, 0)));
                }
                break;
            default: 
                break;
        }
        DrawAttractors();
       
    }

    void GrowTree()
    {
        treeHolder = GameObject.Find("Colonization Generator").transform;

        //удаление дочерних элементов прошлой генерации
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


        nodes = new List<TreeNode>();
        nodes.Add(new TreeNode(Vector3.zero, null, Vector3.up * growthStep)); // root node

        bool growth = true;
        while (growth)
        {

            foreach (var attractor in attractors)
            {
                float closestDistance = float.MaxValue;

                foreach (var node in nodes)
                {
                    float distance = Vector3.Distance(attractor.position, node.position);
                    if (distance < closestDistance && distance <= influenceRadius)
                    {
                        closestDistance = distance;
                        attractor.closestNode = node;
                    }
                }
            }
            List<TreeNode> newNodes = new List<TreeNode>();
            foreach (var node in nodes.FindAll(x => x.canGrow == true))
            {
                List<Attractor> closeAttractors = attractors.FindAll(x => x.closestNode == node);

                if (closeAttractors.Count > 0)
                {
                    Vector3 centerOfMass = Vector3.zero;
                    foreach (var attractor in closeAttractors)
                    {
                        centerOfMass += attractor.position;
                    }
                    centerOfMass /= closeAttractors.Count;

                    Vector3 direction = (centerOfMass - node.position).normalized;
                    Vector3 newPosition = node.position + direction * growthStep;

                    TreeNode newNode = new TreeNode(newPosition, node, direction);
                    if (node.children.Find(x => x.position == newNode.position) is null)
                    {
                        node.children.Add(newNode);
                        newNodes.Add(newNode);
                    }
                    else
                    {
                        node.canGrow = false;
                    }

                    foreach (var attractor in closeAttractors)
                    {
                        if (Vector3.Distance(newNode.position, attractor.position) <= killRadius)
                        {
                            attractors.Remove(attractor);
                        }
                    }
                }

            }
            nodes.AddRange(newNodes);

            if (newNodes.Count == 0) { growth = false; }
            newNodes.Clear();

        }
    }

    void DrawAttractors()
    {
        attractorHolder = new GameObject("Attractors").transform;
        attractorHolder.SetParent(treeHolder);
        foreach (var attractor in attractors)
        {
            GameObject instance = Instantiate(dote, new Vector3(attractor.position.x, attractor.position.y, attractor.position.z), Quaternion.identity) as GameObject;
            instance.transform.localScale = new Vector3(1f, 1f, 1f);
            instance.transform.SetParent(attractorHolder);
        }
           
    }

    float CalcSizeOfBranch(TreeNode node)
    {
        float size = 0f;
        if (node.children.Count == 0)
        {
            size = extremitiesWidth;
        }
        else
        {
            foreach (TreeNode tc in node.children)
            {
                if (tc.size == 0)
                {
                    var childSize = CalcSizeOfBranch(tc);
                    nodes.Find(x => x == tc).size = childSize;
                    size += Mathf.Pow(childSize, widthGrowth);
                }
                else
                {
                    size += Mathf.Pow(tc.size, widthGrowth);
                }
            }
            size = Mathf.Pow(size, 1f / widthGrowth);
        }
        return size;
    }

    void ToMesh()
    {
        _filter = GetComponent<MeshFilter>();
        Mesh treeMesh = new Mesh();

        nodes[0].size = CalcSizeOfBranch(nodes[0]);

        Vector3[] vertices = new Vector3[(nodes.Count+1) * radialSubdivisions];
        int[] triangles = new int[nodes.Count * radialSubdivisions * 6];

        for (int i = 0; i < nodes.Count; i++)
        {
            TreeNode b = nodes[i];

            int vid = radialSubdivisions * i;
            b.verticesId = vid;

            Quaternion quat = Quaternion.FromToRotation(Vector3.up, b.direction);

            for (int s = 0; s < radialSubdivisions; s++)
            {
                float alpha = ((float)s / radialSubdivisions) * Mathf.PI * 2f;

                Vector3 pos = new Vector3(Mathf.Cos(alpha) * b.size, 0, Mathf.Sin(alpha) * b.size);
                pos = quat * pos;

                pos += b.position;
                
                vertices[vid + s] = pos - transform.position;

                if (b.parent == null)
                {
                    vertices[nodes.Count * radialSubdivisions + s] = b.position + new Vector3(Mathf.Cos(alpha) * b.size, 0, Mathf.Sin(alpha) * b.size) - transform.position;
                }
            }
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            TreeNode b = nodes[i];
            int fid = i * radialSubdivisions * 2 * 3;
            int bId = b.parent != null ? b.parent.verticesId : nodes.Count * radialSubdivisions;
            int tId = b.verticesId;

            for (int s = 0; s < radialSubdivisions; s++)
            {
                triangles[fid + s * 6] = bId + s;
                triangles[fid + s * 6 + 1] = tId + s;
                if (s == radialSubdivisions - 1)
                {
                    triangles[fid + s * 6 + 2] = tId;
                }
                else
                {
                    triangles[fid + s * 6 + 2] = tId + s + 1;
                }

                if (s == radialSubdivisions - 1)
                {
                    triangles[fid + s * 6 + 3] = bId + s;
                    triangles[fid + s * 6 + 4] = tId;
                    triangles[fid + s * 6 + 5] = bId;
                }
                else
                {
                    triangles[fid + s * 6 + 3] = bId + s;
                    triangles[fid + s * 6 + 4] = tId + s + 1;
                    triangles[fid + s * 6 + 5] = bId + s + 1;
                }
            }
        }

        treeMesh.vertices = vertices;
        treeMesh.triangles = triangles;
        treeMesh.RecalculateNormals();
        _filter.mesh = treeMesh;
    }


}


public class TreeNode
{
    public Vector3 position;
    public Vector3 direction;
    public TreeNode parent;
    public List<TreeNode> children = new List<TreeNode>();
    public float size;
    public int verticesId;
    public bool canGrow = true;


    public TreeNode(Vector3 position, TreeNode parent, Vector3 dir)
    {
        this.position = position;
        this.parent = parent;
        this.direction = dir;
    }
}

public class Attractor
{
    public Vector3 position;
    public TreeNode closestNode;


    public Attractor(Vector3 position)
    {
        this.position = position;
    }
}