
using UnityEngine;
using System.Collections;

public class InstantiateVert : MonoBehaviour
{

    /// <summary>

    /// Compute a skinned mesh's deformation.

    /// 

    /// The script must be attached aside a SkinnedMeshRenderer,

    /// which is only used to get the bone list and the mesh

    /// (it doesn't even need to be enabled).

    /// 

    /// Make sure the scripts accessing the results run after this one

    /// (otherwise you'll have a 1-frame delay),

    /// </summary>

    //@HideInInspector
    Mesh mesh;
    //@HideInInspector
    SkinnedMeshRenderer skin;


    // @HideInInspector
    private int vertexCount = 0;
    // @HideInInspector
    public Vector3[] vertices;
    // @HideInInspector
    Vector3[] normals;
    //public GameObject ObjectToInstantiate;
    public GameObject MassPrefab;
    //public MassSpawner SpawnerAnim;

    private float MassUnitSize;
    private ArrayList Primitives = new ArrayList();
    private Vector3[] positions;




    void Start()
    {

        skin = GetComponent<SkinnedMeshRenderer>();

        mesh = skin.sharedMesh;
        print(skin.name);


        vertexCount = mesh.vertexCount;//number of primitives , vertexCount = SpawnerAnim.Primitives.Count;



        vertices = new Vector3[vertexCount];//the vertices that have skin weights that need to be updated every frame (check line 115)

        normals = new Vector3[vertexCount];

        //animation example
        for (int b = 0; b < mesh.vertexCount; b++)
        {
            //GameObject cube= new GameObject.CreatePrimitive(PrimitiveType.Cube);//the gameobject that is being instantiated
            //GameObject cube = Instantiate(ObjectToInstantiate);
            GameObject cube = Instantiate(MassPrefab);
            cube.name = b.ToString();
            //cube.AddComponent.<Rigidbody>();

            //cube.Transform.localScale.x = 0.05f;
            //cube.Transform.localScale.y = 0.05f;
            //cube.Transform.localScale.z = 0.05f;
            cube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
        print(skin.bones.Length);
    }



    void Update()
    {
        //print(skin.bones.Length);
        Matrix4x4[] boneMatrices = new Matrix4x4[skin.bones.Length];// this is an array of 4x4 transformation matrices; to allow for transformations in 3D space for each of the vertices; there will be 75 bones and their resp. bone matrices

        print(boneMatrices.Length);
        for (int i = 0; i < boneMatrices.Length; i++)// 1 to 75
        {

            boneMatrices[i] = skin.bones[i].localToWorldMatrix * mesh.bindposes[i];//read the transform from local to world space and multiply with the bind pose (4x4 transform) of each of the bones in the hierarchy

        }


        for (int b = 0; b < mesh.vertexCount; b++)//for all the mesh vertices
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
            // creating a 4x4 matrix for each of the vertices in the mesh; such that each element in the matrix is a summation of up to 4 bones' weights on that vertex


            vertices[b] = vertexMatrix.MultiplyPoint3x4(mesh.vertices[b]);// vertices of the mesh
            normals[b] = vertexMatrix.MultiplyVector(mesh.normals[b]);

            //animation example
            GameObject fetch = GameObject.Find(b.ToString());
            fetch.transform.position = vertices[b];
        }

        //Primitives.Count = vertices.Length;
        /*int numPositions = Primitives.Count;
         for (int i = 0; i < numPositions; ++i)
             ((GameObject)Primitives[i]).transform.position = TranslateToUnityWorldSpace(positions[i]);

     }


     public void SetMassUnitSize(float length) { MassUnitSize = length; }

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

             springMassObject.transform.localScale = Vector3.one * MassUnitSize;
             Primitives.Add(springUnit);
         }
     }


     public void UpdatePositions(Vector3[] p)
     {
         positions = p;//check for interfacing with vertex positions of animations
         //Debug.Log(p);
     }
     private Vector3 TranslateToUnityWorldSpace(Vector3 gridPosition)
     {
         return new Vector3(gridPosition.x, gridPosition.z, gridPosition.y);
     }*/
    }
}