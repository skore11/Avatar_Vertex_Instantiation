using UnityEngine;
using System.Collections.Generic;
//using uFlex;

//#if UNITY_EDITOR
//using UnityEditor; // required to build assets
//#endif

public class VertMap /*: ScriptableObject*/
//public class VertMapAsset : MonoBehaviour
{
    // index = soft body particle index. Value = vertex index.
    public List<int> vertexParticleMap;
    public List<Vector3> particleRestPositions;
    public List<int> nearestVertIndex;
    public List<int> uniqueIndex;
    public WeightList[] particleNodeWeights; // one per node (vert). Weights of standard mesh
                                             //public List<ShapeIndex> shapeIndex; NOT used


}

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
    public int boneIndex; // for transform and returns the following:
    public Vector3 pos;
    public Vector4 rot;
    public Vector3 scale;

    public List<VertexWeight> weights = new List<VertexWeight>();//list of weights to be used for vertices
}

public class VertMapBuilder
    // This class is used to create a vertex map of transforming vertices from original animated mesh 
{
    
    public ShapeMatching ShapeMatching;
    public VertMap vertMap;
    public SkinnedMeshRenderer skin;

    //public VertMapAssetBuilder(FlexShapeMatching flexShapeMatching, VertMapAsset vertMapAsset, SkinnedMeshRenderer skin)

    //Constructor for vert map builder, pass values from SoftAnimation into this constructor 
    public VertMapBuilder(ShapeMatching ShapeMatching, VertMap vertMap, SkinnedMeshRenderer skin)
    {
        //changes for mass spring to accomodate the postions of vertices 
        this.ShapeMatching = ShapeMatching;
        this.vertMap = vertMap;
        this.skin = skin;
    }

    
    //Build the shape matching positions, this function returns a list of Vector3 for the vert Positions
    private List<Vector3> GetShapeMatchingPositions()
    {
        List<Vector3> vertPos = new List<Vector3>();

        ShapeMatching shapes = this.ShapeMatching;
        int shapeIndex = 0;
        int shapeIndexOffset = shapes.m_shapesIndex;
        int shapeStart = 0;

        for (int s = 0; s < shapes.m_shapesCount; s++)
        {
            shapeIndex++;
            int shapeEnd = shapes.m_shapeOffsets[s];
            for (int i = shapeStart; i < shapeEnd; ++i)
            {
                Vector3 pos = ShapeMatching.m_shapeRestPositions[i] + shapes.m_shapeCenters[s];
                vertPos.Add(pos);
                shapeIndexOffset++;
            }

            shapeStart = shapeEnd;
        }

        return vertPos;
    }

    Vector3 RoundVec(Vector3 value)
    {
        float dp = 100000.0f;
        value.x = Mathf.RoundToInt(value.x * dp) / dp;
        value.y = Mathf.RoundToInt(value.y * dp) / dp;
        value.z = Mathf.RoundToInt(value.z * dp) / dp;
        return value;
    }

    private int GetNearestVertIndex(Vector3 particlePos, ref Vector3[] cachedVertices)
    {
        float nearestDist = float.MaxValue;
        int nearestIndex = -1;
        for (int i = 0; i < cachedVertices.Length; i++)
        {
            float dist = Vector3.Distance(particlePos, cachedVertices[i]);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }

    private void SetBoneWeights(ref List<Vector3> uniqueParticlePositions, ref List<int> uniqueParticleIndices)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = skin;

        // Cache used values rather than accessing straight from the mesh on the loop below
        Vector3[] cachedVertices = skinnedMeshRenderer.sharedMesh.vertices;
        Matrix4x4[] cachedBindposes = skinnedMeshRenderer.sharedMesh.bindposes;
        BoneWeight[] cachedBoneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

        // Make an array of WeightLists for each bone in the skinned mesh
        WeightList[] nodeWeights = new WeightList[skinnedMeshRenderer.bones.Length];
        for (int i = 0; i < skinnedMeshRenderer.bones.Length; i++)
        {
            nodeWeights[i] = new WeightList();
            nodeWeights[i].boneIndex = i;
            nodeWeights[i].transform = skinnedMeshRenderer.bones[i];
        }

        for (int uniqueIndex = 0; uniqueIndex < uniqueParticleIndices.Count; uniqueIndex++)
        {
            Vector3 particlePos = uniqueParticlePositions[uniqueIndex];
            int i = GetNearestVertIndex(particlePos, ref cachedVertices);

            vertMap.nearestVertIndex.Add(i);
            vertMap.uniqueIndex.Add(uniqueIndex);
            BoneWeight bw = cachedBoneWeights[i];

            if (bw.weight0 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex0].MultiplyPoint3x4(particlePos);// cachedVertices[i]);
                nodeWeights[bw.boneIndex0].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight0));
            }
            if (bw.weight1 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex1].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex1].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight1));
            }
            if (bw.weight2 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex2].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex2].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight2));
            }
            if (bw.weight3 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex3].MultiplyPoint3x4(particlePos);//cachedVertices[i]);
                nodeWeights[bw.boneIndex3].weights.Add(new VertexWeight(uniqueIndex, localPt, bw.weight3));
            }
        }

        vertMap.particleNodeWeights = nodeWeights;

    }


    public void CreateAsset()
    {
        vertMap.vertexParticleMap = new List<int>();

        //List<Vector3> particlePositions = GetFlexShapeMatchingPositions();
        List<Vector3> particlePositions = GetShapeMatchingPositions();
        List<Vector3> uniqueParticlePositions = new List<Vector3>();
        List<int> uniqueParticleIndices = new List<int>();
        for (int i = 0; i < particlePositions.Count; i++)
        {
            Vector3 vert = particlePositions[i];
            vert = RoundVec(vert);

            if (uniqueParticlePositions.Contains(vert) == false)
            {
                uniqueParticleIndices.Add(i);
                uniqueParticlePositions.Add(vert);
            }


            vertMap.vertexParticleMap.Add(uniqueParticlePositions.IndexOf(vert));
        }

        vertMap.particleRestPositions = uniqueParticlePositions;
        vertMap.nearestVertIndex = new List<int>();
        vertMap.uniqueIndex = new List<int>();
        SetBoneWeights(ref uniqueParticlePositions, ref uniqueParticleIndices);

        // trigger save
        //#if UNITY_EDITOR
        //        UnityEditor.EditorUtility.SetDirty(vertMapAsset);
        //#endif
        //    }
    }

}

//public class FlexAnimation : MonoBehaviour
public class SoftAnimation : MonoBehaviour
    {
        public bool rebuildVertMapAsset = false;
        public VertMap vertMap;
        //Skinned mesh renderer required from original animated mesh
        public SkinnedMeshRenderer skinnedMeshRenderer;
        //private FlexShapeMatching flexShapeMatching;
        private ShapeMatching ShapeMatching;
        private bool firstRun = true;
        private Vector3[] particlePositions; // world particle positions , ###can be substitued with MassSpawner positions, or initializerfor _positions in MassSpringSystem ####
        public bool drawVertMapAsset = false;


        //TODO: the below data is imported from vertmapasset.cs, might not need a scriptable object
        public List<int> vertexParticleMap;
        public List<Vector3> particleRestPositions;
        public List<int> nearestVertIndex;
        public List<int> uniqueIndex;
        public WeightList[] particleNodeWeights; // one per node (vert). Weights of standard mesh


        Vector3[] _cachedVertices;
        Matrix4x4[] _cachedBindposes;
        BoneWeight[] _cachedBoneWeights;
        float refreshRate = 1.0f / 30.0f;
        float timeDelta;

        void OnEnable()
        {
            //if (flexShapeMatching == null)
            //    flexShapeMatching = GetComponent<FlexShapeMatching>();
            if (ShapeMatching == null)
                ShapeMatching = GetComponent<ShapeMatching>();
        }

        //public override void PreContainerUpdate(FlexSolver solver, FlexContainer cntr, FlexParameters parameters)
        //{
        //    if (enabled == false)
        //        return;

        public void CheckUpdate()
        {

            if (firstRun)
            {
                firstRun = false;

                _cachedVertices = skinnedMeshRenderer.sharedMesh.vertices;
                _cachedBindposes = skinnedMeshRenderer.sharedMesh.bindposes;
                _cachedBoneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

                //bool "CreateVertMap" = vertMapAsset == null || rebuildVertMapAsset;

                //if (vertMapAsset == null)
                //{
                //#if UNITY_EDITOR
                //                vertMapAsset = ScriptableObject.CreateInstance<VertMapAsset>();
                //                AssetDatabase.CreateAsset(vertMapAsset, "Assets/" + this.name + "VertMapAsset.asset");
                //                AssetDatabase.SaveAssets();
                //                EditorUtility.FocusProjectWindow();
                //                Selection.activeObject = vertMapAsset;
                //#endif// turn back on if needed to create an asset in the project path

                //}


                vertexParticleMap = new List<int>();
                //VertMapAssetBuilder vertMapAssetBuilder = new VertMapAssetBuilder(flexShapeMatching, vertMapAsset, skinnedMeshRenderer);
                VertMapBuilder vertMapBuilder = new VertMapBuilder(ShapeMatching, vertMap, skinnedMeshRenderer);
                vertMapBuilder.CreateAsset();


                particlePositions = new Vector3[particleRestPositions.Count];
                UpdateParticlePositions();

            }
            else
            {
                if (timeDelta < refreshRate)
                    return;

                // Only process once 30 times a second
                UpdateParticlePositions();
                MatchShapes(); // apply to soft body

                while (timeDelta >= refreshRate)
                    timeDelta -= refreshRate;
            }
        }

        public void UpdateParticlePositions()
        {
            for (int i = 0; i < particlePositions.Length; i++)
            {
                particlePositions[i] = Vector3.zero;
            }

            // Now get the local positions of all weighted indices...
            foreach (WeightList wList in particleNodeWeights)
            {
                foreach (VertexWeight vw in wList.weights)
                {
                    Transform t = skinnedMeshRenderer.bones[wList.boneIndex];
                    particlePositions[vw.index] += t.localToWorldMatrix.MultiplyPoint3x4(vw.localPosition) * vw.weight;
                }
            }

            // Now convert each point into local coordinates of this object.
            for (int i = 0; i < particlePositions.Length; i++)
            {
                particlePositions[i] = transform.InverseTransformPoint(particlePositions[i]);
            }
        }

        private void MatchShapes()
        {
            //FlexShapeMatching shapes = this.flexShapeMatching;
            ShapeMatching shapes = this.ShapeMatching;

            int shapeIndex = 0;
            int shapeIndexOffset = shapes.m_shapesIndex;
            int shapeStart = 0;

            int vertIndex = 0;
            for (int s = 0; s < shapes.m_shapesCount; s++)
            {
                Vector3 shapeCenter = new Vector3();
                shapeIndex++;

                int shapeEnd = shapes.m_shapeOffsets[s];

                int shapeCount = shapeEnd - shapeStart;
                int origShapeIndexOffset = shapeIndexOffset;
                for (int i = shapeStart; i < shapeEnd; ++i)
                {
                    int mappedIndex = vertexParticleMap[vertIndex];
                    Vector3 pos = particlePositions[mappedIndex];
                    shapes.m_shapeRestPositions[shapeIndexOffset] = pos;
                    shapeCenter += pos;
                    shapeIndexOffset++;
                    vertIndex++;
                }

                shapeCenter /= shapeCount;

                for (int i = shapeStart; i < shapeEnd; ++i)
                {
                    Vector3 pos = shapes.m_shapeRestPositions[origShapeIndexOffset];
                    pos -= shapeCenter;
                    shapes.m_shapeRestPositions[origShapeIndexOffset] = pos;
                    origShapeIndexOffset++;
                }

                shapeStart = shapeEnd;
            }
        }

        public void Update()
        {
            timeDelta += Time.deltaTime;

        }

        public virtual void OnDrawGizmos()
        {
            //if (drawVertMapAsset != null)
            //{
            float boxSize = 0.2f;
            /*
            Gizmos.color = Color.red;
            foreach (Vector3 vert in vertMapAssetBuilder._cachedVertices)
            {
                Gizmos.DrawCube(vert, new Vector3(boxSize, boxSize, boxSize));
            }
 
            Gizmos.color = Color.blue;
            foreach (Vector3 vert in vertMapAssetBuilder._uniqueParticlePositions)
            {
                Gizmos.DrawCube(vert, new Vector3(boxSize, boxSize, boxSize));
            }
            */

            if (particlePositions != null)
            {
                Gizmos.color = Color.red;
                foreach (Vector3 vert in particlePositions)
                {
                    Gizmos.DrawCube(vert, new Vector3(boxSize, boxSize, boxSize));
                }
            }

            //}
        }

    }

