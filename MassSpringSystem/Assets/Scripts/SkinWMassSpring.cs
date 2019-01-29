using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;


/*
public static class SpringComputeShaderProperties3D
{
    public const string PositionBufferName = "posBuffer";
    public const string VelocityBufferName = "velBuffer";
    public const string ExternalForcesBufferName = "externalForcesBuffer";
    public const string NeighboursBufferName = "neighboursBuffer";
    public const string PropertiesBufferName = "propertiesBuffer";
    public const string DeltaTimeBufferName = "deltaTimeBuffer";
    public const int NumProperties = 4;
    public const string PosKernel = "CSMainPos";
    public const string VelKernel = "CSMainVel";
}

namespace VoxelSystem.Demo
{

    public class SkinWMassSpring : MonoBehaviour
    {
        enum MeshType
        {
            Volume, Surface
        };

        [SerializeField] MeshType type = MeshType.Volume;
        [SerializeField] new protected SkinnedMeshRenderer renderer;


        [SerializeField] protected ComputeShader voxelizer, particleUpdate;
        [SerializeField] protected int count = 64;

        protected ComputeBuffer particleBuffer;

        GPUVoxelData data;

        private int VertCount;
        // Use this for initialization
        void Start()
        {
            var mesh = Sample();
            data = GPUVoxelizer.Voxelize(voxelizer, mesh, count, (type == MeshType.Volume));
            var pointMesh = BuildPoints(data);
            particleBuffer = new ComputeBuffer(pointMesh.vertexCount, Marshal.SizeOf(typeof(VParticle_t)));
        }

        // Update is called once per frame
        void Update()
        {

        }

        Mesh Sample()
        {
            var mesh = new Mesh();
            renderer.BakeMesh(mesh);
            return mesh;
        }
        Mesh BuildPoints(GPUVoxelData data)
        {
            var count = data.Width * data.Height * data.Depth;
            //Debug.Log(count);
            //Debug.Log(data.Width);
            //Debug.Log(data.Height);
            //Debug.Log(data.Depth);
            var mesh = new Mesh();
            mesh.indexFormat = (count > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = new Vector3[count];//asigns a new vertex position array.

            var indices = new int[count];
            for (int i = 0; i < count; i++)
            {
                indices[i] = i;
                Debug.Log(indices[i]);
            }
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        public Vector3[] GetPositions()
        {
            Vector3[] positions = new Vector3[VertCount];
            positionBuffer.GetData(positions);
            return positions;
            // Debug.Log(positions);
        }


        void UpdatePrimitivePositions()// start here
        {

            Vector3[] positions = new Vector3[VertCount];
            positionBuffer.GetData(positions);// get data from the position buffer and pass it to the positions' array variable

            if (Spawner != null)
                Spawner.UpdatePositions(positions);
            // if (VertSpawner != null)
            // VertSpawner.UpdatePositions(positions);
        }


        public void CreateBuffers()
        {
            positionBuffer = new ComputeBuffer(VertCount, sizeof(float) * 3);
            velocityBuffer = new ComputeBuffer(VertCount, sizeof(float) * 3);
            externalForcesBuffer = new ComputeBuffer(VertCount, sizeof(float) * 3);
            debugBuffer = new ComputeBuffer(VertCount, sizeof(float) * 3);
            neighboursBuffer = new ComputeBuffer(VertCount, sizeof(float) * numNeighbours * 2); // 2D: 24 = 12 float pairs; 3D: 64 = 32 float pairs (index, and is-it-inside-bounds flag)
            propertiesBuffer = new ComputeBuffer(SpringComputeShaderProperties3D.NumProperties, sizeof(float));
            deltaTimeBuffer = new ComputeBuffer(1, sizeof(float));

            ResetBuffers();

            PosKernel = MassSpringComputeShader.FindKernel(SpringComputeShaderProperties3D.PosKernel);//same as FindKernel("CSmainPos")
            VelKernel = MassSpringComputeShader.FindKernel(SpringComputeShaderProperties3D.VelKernel);
        }
    }
}*/