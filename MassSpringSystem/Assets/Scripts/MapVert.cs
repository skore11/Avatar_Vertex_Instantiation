
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapVert : MonoBehaviour
{

    /// <summary>
    /// Compute a skinned mesh's deformation
    /// 
    /// The script must be attached aside a SkinnedMeshRenderer,
    /// which is only used to get the bone list and the mesh
    /// (it doesn't even need to be enabled).
    /// 
    /// Make sure the scripts accessing the results run after this one
    /// (otherwise you'll have a 1-frame delay),
    /// </summary>

    public Dictionary<int, int> MassToVertMap = new Dictionary<int, int>();
    public SkinnedMeshRenderer Skin;
    public MassSpawner3D Spawner;

    public void Awake()
    {
        if (Skin == null)
        {
            Skin = GetComponent<SkinnedMeshRenderer>();
        }
        if (Spawner == null)
        {
            Spawner = GetComponent<MassSpawner3D>();
        }
    }

    public int GetNearestVertIndex(Vector3 particlePos, Vector3[] cachedVertices)
    {
        float nearestDist = float.MaxValue;
        int nearestIndex = -1;
        for (int i = 0; i < cachedVertices.Length; i++)
        {
            float dist = Vector3.Distance(particlePos, cachedVertices[i]);
            //Debug.Log("Mass pos: " + particlePos + "mesh vertex pos: " + cachedVertices[i]);
            //Debug.DrawLine(particlePos, cachedVertices[i], Color.blue, 5.5f);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }

    IEnumerator WaitforSpawner()
    {
        while (Spawner.Primitives.Count < 1)
        {
            yield return null;
        }
    }


    IEnumerator Start()
    {
        // block until we have primitives in the spawner:
        yield return StartCoroutine(WaitforSpawner());
        Mesh mesh = Skin.sharedMesh;
        Vector3[] cachedVertices = mesh.vertices;
        print("Got this many primitives: " + Spawner.Primitives.Count);
        foreach (var indexedPrimitive in Spawner.Primitives)
        {
            GameObject mass = indexedPrimitive.Value;
            //Debug.Log(mass.name);
            // TODO: store the index of the nearest mesh vertex for the current mass
            int nearestIndexforMass = GetNearestVertIndex(mass.transform.localPosition, cachedVertices);
            print(string.Format("For mass number {0} I found the vertex number {1}", indexedPrimitive.Key, nearestIndexforMass));
            MassToVertMap[indexedPrimitive.Key] = nearestIndexforMass;
            //print("nearest index: " + nearestIndexforMass + "for Mass:" + mass.transform.position);
            //need an array or dictionary to store the nearest index and associated mass positions
        }
    }

    void Update()
    {

        //print(skin.bones.Length);
        //Matrix4x4[] boneMatrices = new Matrix4x4[skin.bones.Length];// this is an array of 4x4 transformation matrices; to allow for transformations in 3D space for each of the vertices; there will be 75 bones and their resp. bone matrices

        //print(boneMatrices.Length);
        //for (int i = 0; i < boneMatrices.Length; i++)// 1 to 75
        //{

        //    boneMatrices[i] = skin.bones[i].localToWorldMatrix * mesh.bindposes[i];//read the transform from local to world space and multiply with the bind pose (4x4 transform) of each of the bones in the hierarchy

        //}


        //for (int b = 0; b < mesh.vertexCount; b++)//for all the mesh vertices
        //{

        //    BoneWeight weight = mesh.boneWeights[b];//bone weights of each vertex in the Mesh

        //    //print(b);
        //    //Each vertex is skinned with up to four bones. All weights should sum up to one. Weights and bone indices should be defined in the order of decreasing weight. If a vertex is affected by less than four bones, the remaining weights should be zeroes 
        //    Matrix4x4 bm0 = boneMatrices[weight.boneIndex0];// index of first bone

        //    Matrix4x4 bm1 = boneMatrices[weight.boneIndex1];// index of second bone

        //    Matrix4x4 bm2 = boneMatrices[weight.boneIndex2];// index of third bone

        //    Matrix4x4 bm3 = boneMatrices[weight.boneIndex3];// index of fourth bone



        //    Matrix4x4 vertexMatrix = new Matrix4x4();



        //    for (int n = 0; n < 16; n++)
        //    {//each vertex in the vertexmatrix (16 elements of a 4x4 matrix) is a summation of all possible (up to 4) skinning vertex weights influencing a given bone

        //        vertexMatrix[n] =

        //            bm0[n] * weight.weight0 +

        //            bm1[n] * weight.weight1 +

        //            bm2[n] * weight.weight2 +

        //            bm3[n] * weight.weight3;

        //    }
        //    // creating a 4x4 matrix for each of the vertices in the mesh; such that each element in the matrix is a summation of up to 4 bones' weights on that vertex


        //    vertices[b] = vertexMatrix.MultiplyPoint3x4(mesh.vertices[b]);// vertices of the mesh
        //    normals[b] = vertexMatrix.MultiplyVector(mesh.normals[b]);

        //    //animation example
        //    GameObject fetch = GameObject.Find(b.ToString());
        //    fetch.transform.position = vertices[b];
        //}

    }

    public void OnDrawGizmos()
    {
        if (Skin == null)
        {
            return;
        }
        Mesh mesh = Skin.sharedMesh;
        Vector3[] cachedVertices = mesh.vertices;
        for (int i = 0; i < cachedVertices.Length; i++)
        {
            Gizmos.DrawSphere(cachedVertices[i], 0.05f);
        }
    }


}