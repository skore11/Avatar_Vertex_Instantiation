//===========================================================================================
// Summary
//===========================================================================================

/**
 * This class maintains an array of game objects and periodically updates their positions
 * in the FixedUpdate function. It can be used to spawn a collection of objects that are
 * clones of the public 'MassPrefab' GameObject member variable.
 * 
 * The positions of the objects can be set externally using the public UpdatePositions 
 * function. The FixedUpdate function then assigns the latest positions maintained in the
 * positions array to each object in the collection.
 * 
 * This class is designed to act as a game world spawner for a Mass Spring system that is
 * based around a Y=up coordinate system. Input positions in the SpawnPrimitives and 
 * UpdatePositions functions are therefore translated from the Mass Spring system
 * coordinates to Unity world coordinates by swapping Y and Z values. 
 * 
 * TODO: Apply the ability to instantiate a gameObject (cubes or spheres) to the grid on mouse push down. 
 * The instantiated object should be connected to the grid with the same mass-spring properties. 
 * Also the ability to change the weight or mass of the cubes by color.
 * 
 * 
 */

using UnityEngine;
using System.Collections;

public class MassSpawner3D : MonoBehaviour
{
    public GameObject MassPrefab;

    private float MassUnitSize;
    private ArrayList Primitives = new ArrayList();
    private Vector3[] positions;
    SkinnedMeshRenderer skin;
    Mesh mesh;
    public Vector3[] vertices;
    // @HideInInspector
    Vector3[] normals;
    //===========================================================================================
    //Overrides
    //===========================================================================================
    private void Start()
    {
        skin = GetComponent<SkinnedMeshRenderer>();

        mesh = skin.sharedMesh;
        print(skin.name);
    }

        void FixedUpdate()
    {
        
            //animation example
            //GameObject fetch = GameObject.Find(b.ToString());
            //fetch.transform.position = vertices[b];
        int numPositions = Primitives.Count;
        for (int i = 0; i < numPositions; ++i)
            ((GameObject)Primitives[i]).transform.position = TranslateToUnityWorldSpace(positions[i]);
        
    }

    //===========================================================================================
    // Setter Functions
    //===========================================================================================
    public void SetMassUnitSize(float length) { MassUnitSize = length; }

    //===========================================================================================
    // Initialisation
    //===========================================================================================

    public void SpawnPrimitives(Vector3[] p)
    {
        foreach (GameObject obj in Primitives)
            Destroy(obj.gameObject);
        Primitives.Clear();

        positions = p;
        int numPositions = positions.Length;
        Primitives.Clear();
        foreach (Vector3 massPosition in positions)
        {
            //translate y to z so we can use Unity's in-built gravity on the y axis.
            Vector3 worldPosition = TranslateToUnityWorldSpace(massPosition);

            Object springUnit = Instantiate(MassPrefab, worldPosition, Quaternion.identity);
            GameObject springMassObject = (GameObject)springUnit;
            springMassObject.name = positions.ToString();
            springMassObject.transform.localScale = Vector3.one * MassUnitSize;
            /*Matrix4x4[] boneMatrices = new Matrix4x4[skin.bones.Length];// this is an array of 4x4 matrices; to allow for transformations in 3D space for each of the vertices; there will be 75 bones and their resp. bone matrices

            print(boneMatrices.Length);
            for (int i = 0; i < boneMatrices.Length; i++)
            {

                boneMatrices[i] = skin.bones[i].localToWorldMatrix * mesh.bindposes[i];//read the transform from local to world space and multiply with the bind pose of each of the bones in the hierarchy

            }


            for (int b = 0; b < mesh.vertexCount; b++)
            {

                BoneWeight weight = mesh.boneWeights[b];//bone weights of each vertex in the Mesh

                //print(b);
                //Each vertex is skinned with up to four bones. All weights should sum up to one. Weights and bone indices should be defined in the order of decreasing weight. If a vertex is affected by less than four bones, the remaining weights should be zeroes 
                Matrix4x4 bm0 = boneMatrices[weight.boneIndex0];// index of first bone

                Matrix4x4 bm1 = boneMatrices[weight.boneIndex1];// index of second bone

                Matrix4x4 bm2 = boneMatrices[weight.boneIndex2];// index of third bone

                Matrix4x4 bm3 = boneMatrices[weight.boneIndex3];// index of fourth bone



                Matrix4x4 vertexMatrix = new Matrix4x4();



                for (int n = 0; n < 16; n++)
                {//each vertex in the vertexmatrix (16 elements of a 4x4 matrix) is a summation of all possible (up to 4) skinning vertex weights influencing a given bone

                    vertexMatrix[n] =

                        bm0[n] * weight.weight0 +

                        bm1[n] * weight.weight1 +

                        bm2[n] * weight.weight2 +

                        bm3[n] * weight.weight3;

                }



                vertices[b] = vertexMatrix.MultiplyPoint3x4(mesh.vertices[b]);// vertices of the mesh
                normals[b] = vertexMatrix.MultiplyVector(mesh.normals[b]);
            }*/
            Primitives.Add(springUnit);

        }
        
    }


    //===========================================================================================
    // Position Updating
    //===========================================================================================

    public void UpdatePositions(Vector3[] p)
    {
        positions = p;//check for interfacing with vertex positions of animations
        //Debug.Log(p);
    }

    //===========================================================================================
    // Helper Functions
    //===========================================================================================

    private Vector3 TranslateToUnityWorldSpace(Vector3 gridPosition)
    {
        return new Vector3(gridPosition.x, gridPosition.z, gridPosition.y);
    }
}
