using UnityEngine;
using System;
using System.Collections.Generic;

namespace MeshVoxelizerProject// the namespace is required to access components in other scripts that are present in the scripts folder
{

    public class VoxelizerDemo : MonoBehaviour
    {

        public int size = 16;// define resolution for each axis with size 

        public bool drawAABBTree;// allows the drawing of the lines representing the AABB tree

        private MeshVoxelizer m_voxelizer;// calling the Meshvoxelizer script which is in the scripts folder /Assets

        void Start()
        {
            RenderEvent.AddRenderEvent(Camera.main, DrawOutline);// notify camera of render event; needed in the main camera to render the voxels

            MeshFilter filter = GetComponent<MeshFilter>();// takes the mesh filter of mesh this script is attached to
            MeshRenderer renderer = GetComponent<MeshRenderer>();// takes the mesh renderer of mesh this script is attached to

            if (filter == null || renderer == null)// if the mesh is not present in the parent then check in the children for meshes
            {
                filter = GetComponentInChildren<MeshFilter>();// same as above
                renderer = GetComponentInChildren<MeshRenderer>();
            }

            if (filter == null || renderer == null) return;

            renderer.enabled = false;//set renderer to false?

            Mesh mesh = filter.mesh;// derives only the mesh from the mesh filter
            Material mat = renderer.material;// derives material from renderer

            Box3 bounds = new Box3(mesh.bounds.min, mesh.bounds.max);// check for bounding box's bounds; Box3 is a function defined in the current namespace; mesh.bounds: This is the axis-aligned bounding box of the mesh in its local space (that is, not affected by the transform)

            m_voxelizer = new MeshVoxelizer(size, size, size);// voxelize mesh with given size resolution
            m_voxelizer.Voxelize(mesh.vertices, mesh.triangles, bounds);// access the .Voxelize public void function in MeshVoxelizer.cs; takes in the mesh's vertices triangles and bounds

            Vector3 scale = new Vector3(bounds.Size.x / size, bounds.Size.y / size, bounds.Size.z / size);// scale of each individual voxel in local space
            Vector3 m = new Vector3(bounds.Min.x, bounds.Min.y, bounds.Min.z);
            mesh = CreateMesh(m_voxelizer.Voxels, scale, m);//come back to this 
            

            //assign local positionns, scale and rotations to the voxelized mesh
            GameObject go = new GameObject("Voxelized");
            go.transform.parent = transform;// transform of parent applied to voxelized child gameobject "voxelized" and is instantiated at the position of parent
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;

            filter = go.AddComponent<MeshFilter>();
            renderer = go.AddComponent<MeshRenderer>();

            filter.mesh = mesh;// assign the new mesh created after voxelization to the mesh filter
            renderer.material = mat;// do the same for material as well
        }
        /*private void FixedUpdate()// is not present originally; might have to be called only during start()
        {
            RenderEvent.AddRenderEvent(Camera.main, DrawOutline);

            MeshFilter filter = GetComponent<MeshFilter>();
            MeshRenderer renderer = GetComponent<MeshRenderer>();

            if (filter == null || renderer == null)
            {
                filter = GetComponentInChildren<MeshFilter>();
                renderer = GetComponentInChildren<MeshRenderer>();
            }

            if (filter == null || renderer == null) return;

            renderer.enabled = false;

            Mesh mesh = filter.mesh;
            Material mat = renderer.material;

            Box3 bounds = new Box3(mesh.bounds.min, mesh.bounds.max);

            m_voxelizer = new MeshVoxelizer(size, size, size);
            m_voxelizer.Voxelize(mesh.vertices, mesh.triangles, bounds);

            Vector3 scale = new Vector3(bounds.Size.x / size, bounds.Size.y / size, bounds.Size.z / size);
            Vector3 m = new Vector3(bounds.Min.x, bounds.Min.y, bounds.Min.z);
            mesh = CreateMesh(m_voxelizer.Voxels, scale, m);

            GameObject go = new GameObject("Voxelized");
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;

            filter = go.AddComponent<MeshFilter>();
            renderer = go.AddComponent<MeshRenderer>();

            filter.mesh = mesh;
            renderer.material = mat;
        }*/
        void OnDestroy()
        {
            RenderEvent.RemoveRenderEvent(Camera.main, DrawOutline);// finish render event after lines of AABB tree have been drawn; Drawoutlines function if defined below
        }

        private void DrawOutline(Camera camera)
        {

            if (drawAABBTree && m_voxelizer != null)
            {
                Matrix4x4 m = transform.localToWorldMatrix;
                
                foreach (Box3 box in m_voxelizer.Bounds)
                {
                    DrawLines.DrawBounds(camera, Color.red, box, m);
                    print(m);
                }
            }

        }

        private Mesh CreateMesh(int[,,] voxels, Vector3 scale, Vector3 min)// creating a new mesh with voxel coordinates and quads created from vertices' indices and combined into one mesh called "voxelized"
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();


            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        if (voxels[x, y, z] != 1) continue;

                        Vector3 pos = min + new Vector3(x * scale.x, y * scale.y, z * scale.z);

                        if (x == size - 1 || voxels[x + 1, y, z] == 0)
                            AddRightQuad(verts, indices, scale, pos); //Debug.Log(verts);

                        if (x == 0 || voxels[x - 1, y, z] == 0)
                            AddLeftQuad(verts, indices, scale, pos);

                        if (y == size - 1 || voxels[x, y + 1, z] == 0)
                            AddTopQuad(verts, indices, scale, pos);

                        if (y == 0 || voxels[x, y - 1, z] == 0)
                           AddBottomQuad(verts, indices, scale, pos);

                        if (z == size - 1 || voxels[x, y, z + 1] == 0)
                            AddFrontQuad(verts, indices, scale, pos);

                        if (z == 0 || voxels[x, y, z - 1] == 0)
                            AddBackQuad(verts, indices, scale, pos);
                    }
                }
            }

            if(verts.Count > 65000)
            {
                Debug.Log("Mesh has too many verts. You will have to add code to split it up.");
                return new Mesh();
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        private void AddRightQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;
            //Debug.Log(verts.Count);

            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));

            indices.Add(count + 2); indices.Add(count + 1); indices.Add(count + 0);
            indices.Add(count + 5); indices.Add(count + 4); indices.Add(count + 3);
        }

        private void AddLeftQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));

            indices.Add(count + 0); indices.Add(count + 1); indices.Add(count + 2);
            indices.Add(count + 3); indices.Add(count + 4); indices.Add(count + 5);
        }

        private void AddTopQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));

            indices.Add(count + 0); indices.Add(count + 1); indices.Add(count + 2);
            indices.Add(count + 3); indices.Add(count + 4); indices.Add(count + 5);
        }

        private void AddBottomQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

            indices.Add(count + 2); indices.Add(count + 1); indices.Add(count + 0);
            indices.Add(count + 5); indices.Add(count + 4); indices.Add(count + 3);
        }

        private void AddFrontQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));

            indices.Add(count + 2); indices.Add(count + 1); indices.Add(count + 0);
            indices.Add(count + 5); indices.Add(count + 4); indices.Add(count + 3);
        }

        private void AddBackQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

            indices.Add(count + 0); indices.Add(count + 1); indices.Add(count + 2);
            indices.Add(count + 3); indices.Add(count + 4); indices.Add(count + 5);
        }

    }

}