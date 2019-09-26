
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

    float refreshRate = 1.0f / 30.0f;
    float timeDelta;

    public Dictionary<int, Vector3> MassToVertMap = new Dictionary<int, Vector3>();
    //public Dictionary<int, int> MassToVertMap = new Dictionary<int, int>();
    public SkinnedMeshRenderer Skin;
    public MassSpawner3D Spawner;

    //Required for calculating the nearest mesh vertex index for each of the mass positions
    public List<int> nearestVertIndex;
    public List<int> unique_Index;//unique index of mesh vertices to map on to Mass positions

    private List<Vector3> particlePositions; // world particle positions

    //Needed to update the mass particle positions after assigning bone weights from vertex mapping
    public List<Vector3> particleRestPositions;

    public WeightList[] particleNodeWeights; // one per node (vert). Weights of standard mesh

    Vector3[] _cachedVertices;
    Matrix4x4[] _cachedBindposes;
    BoneWeight[] _cachedBoneWeights;

    private bool firstRun = true;

    

    public class VertexWeight
    {
        public int index;
        public Vector3 localPosition;
        public float weight;

        public VertexWeight()
        {
        }

        public VertexWeight(int i, Vector3 p, float w)
        {
            index = i;
            localPosition = p;
            weight = w;
        }
    }

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

    public class WeightList
    {
        private Transform _temp; // cached on use, not serialized
        public Transform transform {
            get {
                if (_temp == null)
                {
                    _temp = new GameObject().transform;
                    _temp.position = pos;
                    _temp.rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w);
                    _temp.localScale = scale;
                }
                return _temp;
            }
            set {
                pos = value.position;
                rot = new Vector4(value.rotation.x, value.rotation.y, value.rotation.z, value.rotation.w);
                scale = value.localScale;
            }
        }
        public int boneIndex; // for transform
        public Vector3 pos;
        public Vector4 rot;
        public Vector3 scale;

        public List<VertexWeight> weights = new List<VertexWeight>();
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

    public Vector3 GetNearestVertPos(Vector3 particlePos, Vector3[] cachedVertices)
    //public int GetNearestVertIndex(Vector3 particlePos, Vector3[] cachedVertices)
    {
        float nearestDist = float.MaxValue;
        int nearestIndex = -1;
        Vector3 nearestVertexPosition = new Vector3(0.0f, 0.0f,0.0f);
        for (int i = 0; i < cachedVertices.Length; i++)
        {
            float dist = Vector3.Distance(particlePos, cachedVertices[i]);
            //Debug.Log("Mass pos: " + particlePos + "mesh vertex pos: " + cachedVertices[i]);
            //Debug.DrawLine(particlePos, cachedVertices[i], Color.blue, 5.5f);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestVertexPosition = cachedVertices[i];
                nearestIndex = i;
            }
        }
        //return nearestIndex;
        return nearestVertexPosition;
    }

    private void SetBoneWeights(List<Vector3> uniqueParticlePositions, List<int> uniqueParticleIndices)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = Skin;

        // Cache used values rather than accessing straight from the mesh on the loop below
        Vector3[] _cachedVertices = skinnedMeshRenderer.sharedMesh.vertices;
        Matrix4x4[] _cachedBindposes = skinnedMeshRenderer.sharedMesh.bindposes;
        BoneWeight[] _cachedBoneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

        // Make an array of WeightLists for each bone in the skinned mesh
        // return bone index for mesh and associated weights for each bone and associated vertex weights
        WeightList[] nodeWeights = new WeightList[skinnedMeshRenderer.bones.Length];
        for (int i = 0; i < skinnedMeshRenderer.bones.Length; i++)
        {
            nodeWeights[i] = new WeightList();
            nodeWeights[i].boneIndex = i;
            nodeWeights[i].transform = skinnedMeshRenderer.bones[i];
        }

        //Go thru all mesh vertex indices which are now unique vertex indices
        for (int uniqueIndex = 0; uniqueIndex < uniqueParticleIndices.Count; uniqueIndex++)
        {
            //assign a Vector3 temp to unique vertex positions
            Vector3 particlePos = uniqueParticlePositions[uniqueIndex];
            //store the index temporarily of the unique vertex index
            int i = uniqueParticleIndices[uniqueIndex];
            //print(i);
            //nearestVertIndex.Add(i);
            //unique_Index.Add(uniqueIndex);
            //get the bone weight of the associated unique mesh vertex
            BoneWeight bw = _cachedBoneWeights[i];

            //Set bone weights for each of the mesh vertices 
            if (bw.weight0 != 0.0f)
            {
                Vector3 localPt = _cachedBindposes[bw.boneIndex0].MultiplyPoint3x4(particlePos);// cachedVertices[i]);
                nodeWeights[bw.boneIndex0].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight0));
            }
            if (bw.weight1 != 0.0f)
            {
                Vector3 localPt = _cachedBindposes[bw.boneIndex1].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex1].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight1));
            }
            if (bw.weight2 != 0.0f)
            {
                Vector3 localPt = _cachedBindposes[bw.boneIndex2].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex2].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight2));
            }
            if (bw.weight3 != 0.0f)
            {
                Vector3 localPt = _cachedBindposes[bw.boneIndex3].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex3].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight3));
            }
        }

        //Reassign the nodeweights to Weightlist[] of particleNodeweights
        particleNodeWeights = nodeWeights;
        //print(nodeWeights.Length);
        //foreach (var i in particleNodeWeights)
        //{
        //    print(i.ToString());
        //}

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
        List<Vector3> tempCache = new List<Vector3>();
        for (int i = 0; i < cachedVertices.Length; i++)
        {
            tempCache.Add(cachedVertices[i]);
        }
        
        print("Got this many primitives: " + Spawner.Primitives.Count);
        print("Got this many mesh vertices: " + mesh.vertexCount);
        

        foreach (var indexedPrimitive in Spawner.Primitives)
        {
            GameObject mass = indexedPrimitive.Value;
            Vector3 restPosition = mass.transform.position;
            //Debug.Log(restPosition);
            //rest positions of Masses
            particleRestPositions.Add(restPosition);
            //Debug.Log(mass.name);
            //Store the index of the nearest mesh vertex for the current mass
            int nearestIndexforMass = GetNearestVertIndex(mass.transform.localPosition, cachedVertices);
            Vector3 nearestVertexPosforMass = GetNearestVertPos(mass.transform.localPosition, cachedVertices);
            
            
            //print(string.Format("For mass number {0}, found the vertex number {1}", indexedPrimitive.Key, nearestIndexforMass));
            //perhaps the change the keys and values to Vector3
            //MassToVertMap[indexedPrimitive.Key] = nearestIndexforMass;
            MassToVertMap[indexedPrimitive.Key] = nearestVertexPosforMass;
            //print("Mass no. : " + indexedPrimitive.Value + "Mass rest postion: " + restPosition + "and associated vertex: " + nearestIndexforMass);//correct rest positions of masses in MS grid

            unique_Index.Add(nearestIndexforMass);   
        }

        //set the bone weights using the original cached mesh vertices and the indices of said vertices obtained from above
            SetBoneWeights(tempCache, unique_Index);

       
            particlePositions = particleRestPositions;
         


    }

    void FixedUpdate()
    {
        timeDelta += Time.deltaTime;


        //First run to make sure all positions are not empty
        if (firstRun)
        {
            firstRun = false;

            _cachedVertices = Skin.sharedMesh.vertices;
            _cachedBindposes = Skin.sharedMesh.bindposes;
            _cachedBoneWeights = Skin.sharedMesh.boneWeights;

            
            

            List<Vector3> tempCache = new List<Vector3>();
            for (int i = 0; i < _cachedVertices.Length; i++)
            {
                tempCache.Add(_cachedVertices[i]);
            }
            SetBoneWeights(tempCache, unique_Index);
            particlePositions = particleRestPositions;
           

            UpdateParticlePositions();
        }
        else
        {
            if (timeDelta < refreshRate)
                return;
            UpdateParticlePositions();
        }
        
    }


    public void UpdateParticlePositions()
    {
        //set all particle postions to zero Vector first
        for (int i = 0; i < particlePositions.Count; i++)
        {
            particlePositions[i] = Spawner.nextPositions[i];
        }


        // Now get the local positions of all weighted indices...
        foreach (WeightList wList in particleNodeWeights)
        {
            //print(wList);
            foreach (VertexWeight vw in wList.weights)
            {
                Transform t = Skin.bones[wList.boneIndex];
                particlePositions[vw.index] += t.localToWorldMatrix.MultiplyPoint3x4(vw.localPosition) * vw.weight;
                
                //print(particlePositions[vw.index]);
            }
        }

        //print(particlePositions.Count);

        //if (Spawner.nextPositions == null && particlePositions.Count == 0)
        //{
        //    return;
        //}
        //print(particlePositions.Count);
        // Now convert each point into local coordinates of this object.
        //List<Vector3> nextPos = new List<Vector3>(particlePositions.Count);
        for (int i = 0; i < particlePositions.Count; i++)
        {
            
            particlePositions[i] = transform.InverseTransformPoint(particlePositions[i]);
            //foreach (var indexedPrimitive in Spawner.Primitives)
            //{

            //foreach (var a in MassToVertMap)
            //{
            //    int x = a.Key;//Mass name and index
            //    int y = a.Value;//Mesh vertex name and index
            //    Spawner.nextPositions[x] = particlePositions[i];
            //}

            Spawner.nextPositions[i] = particlePositions[i];
            ////}
            ////print(Spawner.nextPositions[i]);
            //Spawner.nextPositions[i] = Spawner.TranslateToUnityWorldSpace(Spawner.nextPositions[i]);
        }

        
        //print(Spawner.nextPositions.Length);



        //Spawner.nextPositions = particlePositions;

        //    foreach (var indexedPrimitive in Spawner.Primitives)
        //    {
        //        //Vector3 newPosition = Spawner.TranslateToUnityWorldSpace(Spawner.nextPositions[indexedPrimitive.Key]);

        //        Vector3 newPosition = particlePositions[indexedPrimitive.Key];

        //        GameObject primi = indexedPrimitive.Value;
        //        Rigidbody rb = primi.GetComponent<Rigidbody>();

        //        if (rb)
        //        {
        //            Vector3 dist = newPosition - rb.position;
        //            rb.AddForce(dist * Time.deltaTime);

        //            //rb.position = newPosition;

        //        }
        //        else
        //        {
        //            primi.transform.position = newPosition;
        //        }
        //    }
        //    Spawner.nextPositions = null;
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