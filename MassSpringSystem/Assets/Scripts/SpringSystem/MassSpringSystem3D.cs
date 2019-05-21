using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//===========================================================================================
// Simple class that holds various property names and attributes for the 
// MassSpringSystem.compute shader.
//
// Is used to implement physical constraints on each particle of voxelized ragdoll
//===========================================================================================

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

//===========================================================================================
// Simple class that holds various property names and attributes for the 
// SpringRenderShader.shader.
//===========================================================================================

public static class MassSpringRenderShaderProperties3D
{
    public const string PositionsBuffer = "buf_Points";
    public const string DebugBuffer = "buf_Debug";
    public const string VelocityBuffer = "buf_Vels";
}

//===========================================================================================
// Summary
//===========================================================================================
/**
 * This class is used to periodically run position and velocity buffers for each of the voxels through compute
 * shader kernels that update them according to a mass spring model. It also provides access
 * to properties of the model that can be tweaked from the editor. This class is used to 
 * update these property values in the compute shader as they are changed externally.
 * 
 * The MassSpawner Spawner member variable is used to spawn and update game objects according
 * to the positions on the mass spring grid. Alternatively, points can be rendered using
 * the RenderShader Shader member variable. In order to do this, uncomment the OnPostRender
 * function and comment the Update function and the call to SpawnPrimitives in the 
 * Initialise function.
 * 
 * Various ComputeBuffer variables are used to read and write data to and from the compute 
 * shader (MassSpringComputeShader).
 * 
 * This class can also be used to translate touch and mouse input (from the UITouchHandler) 
 * into forces that are applied to the mass spring system (implemented by the 
 * MassSpringComputeShader). 
 * 
 * Various other forces can be defined through the externalforcesBuffer that can allow 
 * for reactive deformations in the discretized topology
 */

public class MassSpringSystem3D : MonoBehaviour
{
    /** The compute shader that implements the mass spring model.
     */
    public ComputeShader MassSpringComputeShader;

    /** A Shader that can be used to render the mass points directly
     *  rather than using game objects in the game world.
     */
    public Shader RenderShader;

    /** The mass of individual mass points in the mass spring model.
     *  Increasing this will make the mass points more resistive to
     *  the springs in the model, but will also reduce their velocity.
     */
    [Range(1.0f, 100.0f)] public float Mass = 1.0f;

    /** The level of damping in the system. Increasing this value
     *  will cause the system to return to a more 'stable' state quicker,
     *  and will reduce the propagation of forces throughout the grid.
     */
    [Range(0.1f, 0.999f)] public float Damping = 0.1f;

    /** The stiffness of the spings in the grid. Increasing this will
     *  cause mass points to 'rebound' with higher velocity, and will
     *  also decrease the time taken for the system to return to a
     *  'stable' state.
     */
    [Range(0.1f, 100.0f)] public float SpringStiffness = 10.0f;

    /** The lenght of the springs in the grid. This defines how far
     *  each mass unit is at a resting state.
     */
    [Range(0.1f, 10.0f)] public float SpringLength = 1.0f;

    /** The controller of the game object spawner object.
     */
    public MassSpawner3D Spawner;
   

    /** The controller of the touch and mouse input handler object.
     */
    public CanvasTouchManager UITouchHandler;

    /** This is the force that will be applied from touch and mouse 
     *  input events on the grid.
     */
    [Range(0.0f, 1000.0f)] public float MaxTouchForce = 100.0f;

    /** Various ComputeBuffer variables are used to read and write data to and from the compute 
     *  shader (MassSpringComputeShader). 
     *  Note: comment out debugbuffer for testing
     */
    //private ComputeBuffer debugBuffer;
    private ComputeBuffer propertiesBuffer;
    // We fill a buffer of grid neigbour positions (the grid needs to be defined as the bounds of the animated model) and send it to the compute buffer on intialisation, such that 
    // we have access to neighbouring positions in our compute kernels. The neighbours buffer is a buffer of Vector2
    // elements, where the x of each element is the neighbour position and the y is whether that position exists within
    // the bounds of the grid.
    private ComputeBuffer neighboursBuffer;
    private ComputeBuffer deltaTimeBuffer;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer velocityBuffer;
    private ComputeBuffer externalForcesBuffer;

    private const int numNeighbours = 32; // 12 for the 2D grid case, see compute shader.

    /** Our compute shader runs the same kernels in parallel on mutliple blocks of our
     *  mass spring grid. These blocks are of dimensions gridUnitSideX by gridUnitSideY by gridUnitSideZ,
     *  and there are numThreadsPerGroupX blocks along the x dimension of our grid and
     *  numThreadsPerGroupY along the Y dimension as well as the numThreadsGroupZ along the Z dimension.
     *  
     *  These values MUST be identical to the gX, gY and gZ values in the MassSpringCompute compute shader.
     *  
     *  Change resolution of 3D grid using these values
     */
    private const int gridUnitSideX = 2;
    private const int gridUnitSideY = 2;
    private const int gridUnitSideZ = 2; // leave it at 7 for now
    private const int numThreadsPerGroupX = 10;
    private const int numThreadsPerGroupY = 10;
    private const int numThreadsPerGroupZ = 10; // leave it at 1 for now

    /** The resolution of our entire grid, according to the resolution and layout of the individual
     *  blocks processed in parallel by the compute shader. Include 3rd dimenion Z by iniitalising:
     *  private int GridResZ;
     */
    private int GridResX = gridUnitSideX * numThreadsPerGroupX;
    private int GridResY = gridUnitSideY * numThreadsPerGroupY;
    private int GridResZ = gridUnitSideZ * numThreadsPerGroupZ;

    /** The total number of mass points (vertices) in the grid.
     */
    private int VertCount;

    /** The two kernels in the compute shader for updating the positions and velocities, respectively. 
     */
    private int PosKernel;
    private int VelKernel;

    /** This material can used to render the mass points directly (rather than using game objects).
     *  This material is instantiated using the RenderShader shader.
     */
    private Material RenderMaterial;

    //Check for gravity
    public bool foundGravityForces = false;


    //Check for skeleton
    public  GameObject objectSkeleton;
    private Transform[] skeletonJoints;

    void Start()
    {
        //Use the bone transforms from viewskeleton to transfer control to appropriate masses and 
        //run active ragdoll
        skeletonJoints = objectSkeleton.GetComponent<ViewSkeleton>().childNodes;
        Initialise();
    }

    void FixedUpdate()
    {
        HandleGravityAndTouches();
        Dispatch();
        UpdatePrimitivePositions();
    }

    /* This function can be used for graphical debugging puproses,
     * or if you simply want to render the mass points as points rather
     * than maintaining game objects.
    */

    /*void OnPostRender ()
    {
        Dispatch ();
        RenderDataPoints ();
    }*/

    public void OnDrawGizmos()
    {
        // Display the size of the massspringsystem
        Gizmos.color = new Color(1, 1, 0, 0.75F);
        float x = gridUnitSideX * numThreadsPerGroupX * SpringLength;
        float y = gridUnitSideY * numThreadsPerGroupY * SpringLength;
        float z = gridUnitSideZ * numThreadsPerGroupZ * SpringLength;
        //Debug.Log("Gizmo size: " + x + y + z);
        Gizmos.DrawWireCube(transform.position, new Vector3(x, z, y));
    }

    private void OnDisable()
    {
        ReleaseBuffers();
    }

    //===========================================================================================
    // Accessors
    //===========================================================================================

    /** Checks if an object is recognised as a spring mass model game object. 
     * Additionally there is bool to check if the mass unit is also a bone transferred from viewskeleton function
     */
    public static bool IsMassUnit(string objectTag) { return objectTag == "MassUnit"; }
    public static bool IsBoneMassUnit(string objectTag) { return objectTag == "MassUnit + BoneUnit"; }

    /** Get the values of the mass positions from the compute buffer.
     */
    public Vector3[] GetPositions()
    {
        Vector3[] positions = new Vector3[VertCount];
        positionBuffer.GetData(positions);
        return positions;
    }

    /** Helper functions to get grid dimension properties in the world space.
     */
    public float GetWorldGridSideLengthX()
    {
        return GridResX * SpringLength;
    }

    public float GetWorldGridSideLengthY()
    {
        return GridResY * SpringLength;
    }

    public float GetWorldGridSideLengthZ()
    {
        return GridResZ * SpringLength;
    }
    //===========================================================================================
    // Construction / Destruction
    //===========================================================================================

    /** Initialise all of the compute buffers that will be used to update and read from the compute shader.
     *  Fill all of these buffers with data in order to construct a resting spring mass grid of the correct dimensions.
     *  Initialise the position and velocity kernel values using their name values from the SpringComputeShaderProperties static class.
     *  
     *  Typically VertCount is the total number of voxels initialised by compute shader in the regualr grid
     *  After the convex hull tester only the voxels inside the mesh collider of the character are maintained 
     *  in the mass list
     *  
     *  position and velocity need to read Vector3 data so the memory allocated is 4 bytes per axes i.e. 12 bytes in total for each position and velocity
     *  
     */
    public void CreateBuffers()
    {
        positionBuffer = new ComputeBuffer(VertCount, sizeof(float) * 3);
        velocityBuffer = new ComputeBuffer(VertCount, sizeof(float) * 3);
        externalForcesBuffer = new ComputeBuffer(VertCount, sizeof(float) * 3);
        //debugBuffer = new ComputeBuffer(VertCount, sizeof(float) * 3);
        neighboursBuffer = new ComputeBuffer(VertCount, sizeof(float) * numNeighbours * 2); 
        propertiesBuffer = new ComputeBuffer(SpringComputeShaderProperties3D.NumProperties, sizeof(float));
        deltaTimeBuffer = new ComputeBuffer(1, sizeof(float));

        ResetBuffers();

        PosKernel = MassSpringComputeShader.FindKernel(SpringComputeShaderProperties3D.PosKernel);//same as FindKernel("CSmainPos")
        VelKernel = MassSpringComputeShader.FindKernel(SpringComputeShaderProperties3D.VelKernel);
    }

    /** Fills all of the compute buffers with starting data to construct a resting spring mass grid of the correct dimensions
     *  according to GridResX, GridResY and GridResZ. For each vertex position we also calculate the positions of each of the neighbouring 
     *  vertices so that we can send this to the compute shader.
     */
    public void ResetBuffers()
    {
        Vector3[] positions = new Vector3[VertCount];
        Vector3[] velocities = new Vector3[VertCount];
        Vector3[] extForces = new Vector3[VertCount];
        Vector2[] neighbours = new Vector2[VertCount * numNeighbours];
        int neighboursArrayIndex = 0;
        int vertex = 0;
        // calculating 3D coordinates: innermost loop is x, then y, then z. this order that is expected by the compute shader!
        for (int k = (int)Spawner.transform.position.z; k < GridResZ + Spawner.transform.position.z; k++)
        {
            float z = (((k ) - GridResZ / 2.0f) / GridResZ) * GetWorldGridSideLengthZ();
            for (int j = (int)Spawner.transform.position.y; j < GridResY + Spawner.transform.position.y; j++)
            {
                float y = (((j ) - GridResY / 2.0f) / GridResY) * GetWorldGridSideLengthY();
                for (int i = (int)Spawner.transform.position.x; i < GridResX + Spawner.transform.position.x; i++)
                {
                    float x = (((i ) - GridResX / 2.0f) / GridResX) * GetWorldGridSideLengthX();
                    //Debug.Log("x " + x + " y " + y + " z " + z);
                    positions[vertex] = new Vector3(x, y, z);
                    velocities[vertex] = new Vector3(0.0f, 0.0f, 0.0f);
                    extForces[vertex] = new Vector3(0.0f, 0.0f, 0.0f);

                    Vector2[] neighbourIndexFlagPairs = GetNeighbourIndexFlagPairs(vertex, i, j, k);
                    for (int n = 0; n < 32; ++n)
                    {
                        neighbours[neighboursArrayIndex] = neighbourIndexFlagPairs[n];
                        neighboursArrayIndex++;
                    }
                    vertex++;
                }
            }
        }

        positionBuffer.SetData(positions);// setting the position buffer that is sent to the compute shader with the positions of the vertices count
        velocityBuffer.SetData(velocities);
        //debugBuffer.SetData(positions);
        externalForcesBuffer.SetData(extForces);
        neighboursBuffer.SetData(neighbours);// setting the neighbor buffer that is sent to the compute shader wit the positions of the neighbor indices
    }

    public void ReleaseBuffers()
    {
        if (positionBuffer != null)
            positionBuffer.Release();
        if (velocityBuffer != null)
            velocityBuffer.Release();
        //if (debugBuffer != null)
           // debugBuffer.Release();
        if (propertiesBuffer != null)
            propertiesBuffer.Release();
        if (deltaTimeBuffer != null)
            deltaTimeBuffer.Release();
        if (externalForcesBuffer != null)
            externalForcesBuffer.Release();
        if (neighboursBuffer != null)
            neighboursBuffer.Release();
    }

    void CreateMaterialFromRenderShader()
    {
        if (RenderShader != null)
            RenderMaterial = new Material(RenderShader);
        else
            Debug.Log("Warning! Attempting to initialise MassSpringSystem without setting the Shader variable.");
    }

    /** Calculate our entire grid resolution and vertex count from the structure of the compute shader.
     *  Create our render material, and initialise and fill our compute buffers. Send the vertex neighbour 
     *  positions to the compute shader (we only need to do this once, whereas the position and velocities
     *  need to be sent continuously). Finally, we get the initial positions from the compute buffer and use
     *  them to spawn our game objects using the Spawner.
     */
    public void Initialise()
    {
        VertCount = GridResX * GridResY * GridResZ;
        CreateMaterialFromRenderShader();
        CreateBuffers();// creates all the buffers for positions, velocity, neighbors ,forces  also finds both kernels Poskernel and Velkernel
        MassSpringComputeShader.SetBuffer(VelKernel/*PosKernel*/, SpringComputeShaderProperties3D.NeighboursBufferName, neighboursBuffer);// set neighbors buffer
        Vector3[] positions = new Vector3[VertCount];// VertCount for 3D grid => VertCount = GridResX * GridResY * GridResZ; 
        positionBuffer.GetData(positions);//Read values from positionbuffer into the positions array
        Spawner.SetMassUnitSize(SpringLength);
        Spawner.SpawnPrimitives(positions);// called from mass spawner.cs
    }

    //===========================================================================================
    // Touch Input
    //===========================================================================================

    /** Fill and return an array of vertex positions that are the direct neighbouring positions of position
     *  'index' in the mass spring grid.
     *  
     *  Neighbours are listed in 'clockwise' order for immediate neighbours in the mass voxel's 2D plane:
     *  north, north-east, east, south-east, south, south-west, west, north-west,
     *  similarily the same ordered is performed for neighbours in the above and below plane as well
     *  
     *  This function does NOT check the index bounds. 
     */
    public int[] GetNeighbours(int index)
    {
        int[] neighbours = new int[26] {index + GridResX, index + GridResX + 1, index + 1, index - GridResX + 1,
                                       index - GridResX, index - GridResX - 1, index - 1, index + GridResX - 1,
                                       index + (GridResX * GridResY), index + (GridResX * GridResY) + GridResX, index + (GridResX * GridResY) + GridResX +1, index + (GridResX * GridResY) + 1 /*GridResY*/,
                                       index + (GridResX * GridResY) - GridResX +1, index + (GridResX * GridResY) - GridResX, index + (GridResX * GridResY) - GridResX -1, index + (GridResX * GridResY) - 1/*GridResY*/, index + (GridResX * GridResY) + GridResX -1,
        index - (GridResX * GridResY), index - (GridResX * GridResY) + GridResX, index - (GridResX * GridResY) + GridResX + 1, index - (GridResX * GridResY) + 1 /*GridResY*/,
                                       index - (GridResX * GridResY) - GridResX + 1, index - (GridResX * GridResY) - GridResX, index - (GridResX * GridResY) - GridResX - 1, index - (GridResX * GridResY) - 1/*GridResY*/, index - (GridResX * GridResY) + GridResX - 1};
        return neighbours;
    }
    // List all possible neighbors in the each of the planes that are surrounding a particular mass voxel
    // 32 neighbors in total who need to be parsed to the compute shader as well

    List<int> eastIndices  = new List<int> {1,2,3,10,11,12,19,20,21,27};
    List<int> westIndices  = new List<int> {5,6,7,14,15,16,23,24,25,29};
    List<int> northIndices = new List<int> {0,1,7,9,10,16,18,19,25,26};
    List<int> southIndices = new List<int> {3,4,5,12,13,14,21,22,23,28};
    List<int> upIndices    = new List<int> {8,9,10,11,12,13,14,15,16,30};
    List<int> downIndices  = new List<int> {17,18,19,20,21,22,23,24,25,31};

    /** Fill and return an array of Vector2 where x = neighbour position and y = neighbour exists in grid, 
     *  including both direct neighbour positions and "bend" positions.
     *  Bend positions are 2 grid spaces away on both x, y and z axes, and implement
     *  resistance to bending in the mass spring grid.
     *  
     *  Neighbours are listed in 'clockwise' order (in 2D) of direct neighbours followed by clockwise bend neighbour positions:
     *  north, north-east, east, south-east, south, south-west, west, north-west, north-bend, east-bend, south-bend, west-bend. 
     *  the same order needs to be followed while calculating for neighbors in 3D as well.
     */
    public Vector2[] GetNeighbourIndexFlagPairs(int index, int xLoopIndex, int yLoopIndex, int zLoopIndex)
    {
        //TODO: needs to use the same numNeighbours (32) neighbours as the compute shader in the same order

        //n, ne, e, se, s, sw, w, nw, nb, eb, sb, wb
        int[] neighburIndexes = GetNeighbours(index);
        int[] bendIndexes = { neighburIndexes[0] + GridResX, neighburIndexes[2] + 1, neighburIndexes[4] - GridResX, neighburIndexes[6] - 1,
                              neighburIndexes[8] + (GridResX * GridResY), neighburIndexes[17] - (GridResX * GridResY)};//3D 
        int[] neighbours = new int[32];

        neighburIndexes.CopyTo(neighbours, 0);// copies everything from array to another, starting from the given index (here it is from 0th index)
        bendIndexes.CopyTo(neighbours, 26);

        /** Depending on the specific neighbour position, we need to check varying bounds conditions.
         *  check within loop indicies if neighbor exists within the bounds
         */
        Vector2[] neighbourFlagPairs = new Vector2[32];
        for (int i = 0; i < 32; ++i)
        {
            int id = neighbours[i];
            float flag = 1.0f;
            if (id < 0 || id >= VertCount)
            {
                flag = 0f;
                id = 0;
            }
            else
            {
                if ((xLoopIndex == 0) && (westIndices.Contains(i)))
                {
                    flag = 0f;
                }
                if ((xLoopIndex == 1) && (i == 29))
                {
                    flag = 0f;
                }
                if ((xLoopIndex == GridResX - 1) && (eastIndices.Contains(i)))
                {
                    flag = 0f;
                }
                if ((xLoopIndex == GridResX - 2) && (i == 27))
                {
                    flag = 0f;
                }
                if ((yLoopIndex == 0) && (southIndices.Contains(i)))
                {
                    flag = 0f;
                }
                if ((yLoopIndex == 1) && (i == 28))
                {
                    flag = 0f;
                }
                if ((yLoopIndex == GridResY - 1) && (northIndices.Contains(i)))
                {
                    flag = 0f;
                }
                if ((yLoopIndex == GridResY - 2) && (i == 26))
                {
                    flag = 0f;
                }
                if ((zLoopIndex == 0) && (downIndices.Contains(i)))
                {
                    flag = 0f;
                }
                if ((zLoopIndex == 1) && (i == 31))
                {
                    flag = 0f;
                }
                if ((zLoopIndex == GridResZ - 1) && (upIndices.Contains(i)))
                {
                    flag = 0f;
                }
                if ((zLoopIndex == GridResZ - 2) && (i == 30))
                {
                    flag = 0f;
                }
            }
            neighbourFlagPairs[i] = new Vector2(id, flag);
        }
        // now run through all the neighbours again, but check if they are inside the model you want to form, or not:
        // and disable the neighbour's influence if it is outside:

        return neighbourFlagPairs;
    }

    /** Applies a given pressure value to a given mass index. This pressure is added to the 
     *  external forces acting on the grid of masses.
     *  
     *  Might have to change this for gravity because only a force of Maxtouch is always applied
     */
    public void ApplyPressureToMass(int index, float pressure, ref Vector3[] extForces)
    {
        if (index >= 0 && index < extForces.Length)
        {
            extForces[index] = new Vector3(0.0f, 0.0f, MaxTouchForce * pressure * -1.0f);//make it +1.0f to make it a pull
        }
    }

    /** Gets the neighbouring positions of a mass index and applies reduced pressure to them.
     */
    public void ApplyPressureToNeighbours(int index, float pressure, ref Vector3[] extForces)
    {
        int[] neighbours = GetNeighbours(index);
        foreach (int i in neighbours)
        {
            ApplyPressureToMass(i, pressure * 0.5f, ref extForces);
        }
    }

    /** Takes in an existing touch or mouse event and transforms it into pressure to be applied at
     *  a specific point on the grid.
     */
    public void UITouchInputUpdated(int index, float pressure, ref Vector3[] extForces)
    {
        //Debug.Log("Touch input, pressure applied to masssobj index " + index + "pressure" + pressure);
        ApplyPressureToMass(index, pressure, ref extForces);
        ApplyPressureToNeighbours(index, pressure, ref extForces);
    }

    static int count =0;
    /** Called continuously by the update function to transform input data to grid forces.
     */
    private void HandleGravityAndTouches()
    {
        /*
                        Vector3[] prepositionbuffer = new Vector3[VertCount];
                        positionBuffer.GetData(prepositionbuffer);
                            List<string> lines = new List<string>();
                            foreach (Vector3 pos in prepositionbuffer)
                            {
                                lines.Add("x:" + pos.x + "y:" + pos.y + "z:" + pos.z);
                            }
                            System.IO.File.WriteAllLines(Path.Combine(
                                Path.GetTempPath(), count + "preposition.txt"), lines.ToArray());
        */
        Vector3[] extForces = new Vector3[VertCount];
        for (int i = 0; i < VertCount; i++)
        {
            extForces[i] = new Vector3(0.0f, 0.0f, 0.0f);
        }
        Vector3[] gravityForces = new Vector3[VertCount];
        for (int i = 0; i < VertCount; i++)
        {
            gravityForces[i] = new Vector3(0.0f, 0.0f, 0.0f);
        }
        bool foundExternalForces = false;
       
        // Check Gravity Effects:
        // ask MassSpawner to go through all spawned primitives:
        int index = 0;
        foreach (GameObject mass in Spawner.Primitives.Values)
        {
            //    // there: check if there is a rigidbody, check if that has velocity applied to it
            //    // if so, set the extforces entry for that rigidbody (use the name thing as in ProjectScreenPositionToMassSpringGrid
            //    // to get the index)
            //for (int i = 0; i < skeletonJoints.Length; i++)
            //{
            //   float dist = Vector3.Distance(skeletonJoints[i].position,mass.transform.position);
            //    if (dist < 0.1f)
            //   {
            //        mass.AddComponent<Rigidbody>().useGravity = true;
            ////        //mass.gameObject.tag = "MassUnit + BoneUnit";
            //        Vector3 velocity = mass.GetComponent<Rigidbody>().velocity;
            //        gravityForces[index].z = velocity.y;
            //        //        //        //Debug.Log("index" + index + " " + velocity.x);
            //        Debug.Log(mass.name);
            //        index++;

            //        foundGravityForces = true;
            //    }
            //    else
            //    {
            //        foundGravityForces = false;
            //    }
            //}
            if (mass.GetComponent<Rigidbody>().useGravity == true)
            {
            //    //        // TODO: actually check if the rigidbody has any velocity on it:
            //    //        // only then add a corresponding external force
            //    //        //int index = int.Parse(mass.name.Substring(7, mass.name.IndexOf(' ') - 7));
            //    //        //Debug.Log(index);
            //    //        //UITouchHandler.GridTouches.Add(new Vector2(index, UITouchHandler.SimulatedPressure));
            //    //        // if any gravity effects were found (bigger than 0 or a threshold), set foundExternalForce true
               Vector3 velocity = mass.GetComponent<Rigidbody>().velocity;
            //    //        //Debug.Log("index" + index + "" + velocity);
            //    //        //extForces[index] = new Vector3 (0.0f, 0.0f, -9.8f);//flipping 'y' and 'z' axes
            //    //        //gravity velocity of initial sphere applied to all rigidbodies in grid
            //    //        //extForces[index].x = velocity.x;
            //    //        //extForces[index].y = velocity.z;
                //        //extForces[index].z = velocity.y;
                        gravityForces[index].z = velocity.y;
                //Debug.Log("index" + index + " " + velocity.x + " " + velocity.y + " " + velocity.z);
                // Debug.Log(velocity.y / Time.deltaTime);
                //Debug.Log(Time.deltaTime);
                index++;
              
                foundGravityForces = true;
                    }
                }
                
                // Check Touch Effects:
                if (UITouchHandler.GridTouches.Count > 0)
        {
            foreach (Vector2 gridTouch in UITouchHandler.GridTouches)
            {
                UITouchInputUpdated((int)gridTouch.x, gridTouch.y,ref extForces);
            }
            foundExternalForces = true;
            UITouchHandler.GridTouches.Clear();
        }
        if (foundGravityForces)
        {
            externalForcesBuffer.SetData(gravityForces);
        }

        if (foundExternalForces)
        {
            //Debug.Log("External forces" + "index:" + " " + index + " " + "x:" + extForces[index].x + " " + "y:" + extForces[index].y + " " + "z:" + extForces[index].z);
            externalForcesBuffer.SetData(extForces);

        }
        
    }

    //===========================================================================================
    // Shader Values
    //===========================================================================================

    void SetGridPropertiesAndTime()
    {
        propertiesBuffer.SetData(new float[] { Mass, Damping, SpringStiffness, SpringLength });
        deltaTimeBuffer.SetData(new float[] { Time.deltaTime }); // should use correct delta even when running with FixedUpdate (hopefully)
    }

    void SetPositionBuffers()//Is used in Dispatch
    {
        MassSpringComputeShader.SetBuffer(PosKernel, SpringComputeShaderProperties3D.DeltaTimeBufferName, deltaTimeBuffer);
        MassSpringComputeShader.SetBuffer(PosKernel, SpringComputeShaderProperties3D.PositionBufferName, positionBuffer);//Check position buffers are being passed in the compute shader
        MassSpringComputeShader.SetBuffer(PosKernel, SpringComputeShaderProperties3D.VelocityBufferName, velocityBuffer);
        MassSpringComputeShader.SetBuffer(PosKernel, SpringComputeShaderProperties3D.ExternalForcesBufferName, externalForcesBuffer);
    }

    void SetVelocityBuffers()//Is used in Dispatch
    {
        MassSpringComputeShader.SetBuffer(VelKernel, SpringComputeShaderProperties3D.PropertiesBufferName, propertiesBuffer);
        MassSpringComputeShader.SetBuffer(VelKernel, SpringComputeShaderProperties3D.DeltaTimeBufferName, deltaTimeBuffer);
        MassSpringComputeShader.SetBuffer(VelKernel, SpringComputeShaderProperties3D.ExternalForcesBufferName, externalForcesBuffer);
        MassSpringComputeShader.SetBuffer(VelKernel, SpringComputeShaderProperties3D.VelocityBufferName, velocityBuffer);
        MassSpringComputeShader.SetBuffer(VelKernel, SpringComputeShaderProperties3D.PositionBufferName, positionBuffer);
    }

    float lastDebugTime = 0f;

    void UpdatePrimitivePositions()// start here
    {
        //if (true && Time.fixedTime > lastDebugTime + 0.3f)
        //{ // DEBUGGING
        //    lastDebugTime = Time.fixedTime;
        //    Vector3[] vels = new Vector3[VertCount];
        //    velocityBuffer.GetData(vels);
        //    // print debug info for velocities higher than a certain threshold:
        //    //string debug = "";
        //    //for (int i = 0; i < vels.Length; ++i)
        //    //{
        //    //    Vector3 vel = vels[i];
        //    //    if (vel.sqrMagnitude > 0.2f)
        //    //    {
        //    //        debug += " " + i + ": " + vel;
        //    //    }
        //    //}
        //    //if (debug.Length > 0)
        //    //{
        //    //    //Debug.Log("High Velocities: " + debug);
        //    //}
        //}

        Vector3[] positions = new Vector3[VertCount];
        positionBuffer.GetData(positions);// get data from the position buffer and pass it to the positions' array variable

        if (Spawner != null)
            Spawner.UpdatePositions(positions);

    }

    void Dispatch()
    {
        SetGridPropertiesAndTime();

        //In order to Dispatch for a 3D grid add the gridUnitSideZ as well for both PosKernel and VelKernel
        SetVelocityBuffers();
        
        MassSpringComputeShader.Dispatch(VelKernel, gridUnitSideX, gridUnitSideY, gridUnitSideZ);

        SetPositionBuffers();

        MassSpringComputeShader.Dispatch(PosKernel, gridUnitSideX, gridUnitSideY, gridUnitSideZ);

    }

    //===========================================================================================
    // Rendering
    //===========================================================================================

    /*
     * If you want to use this to debug the positions, you need to override OnPostRender() instead of Update().
     */
    void RenderDataPoints()
    {
        RenderMaterial.SetPass(0);
        //RenderMaterial.SetBuffer(MassSpringRenderShaderProperties3D.DebugBuffer, debugBuffer);
        RenderMaterial.SetBuffer(MassSpringRenderShaderProperties3D.PositionsBuffer, positionBuffer);
        RenderMaterial.SetBuffer(MassSpringRenderShaderProperties3D.VelocityBuffer, velocityBuffer);
        Graphics.DrawProcedural(MeshTopology.Points, VertCount);
        //Graphics.DrawProcedural(MeshTopology.Points, vertexCount);
    }

    //===========================================================================================
    // Animation
    //===========================================================================================

    /** This function can be used to simulate some movement on the grid.
     *  It should be called continuously (e.g. in the Update or OnPostRender
     *  functions).
     */
    void AnimateGrid()
    {
        Vector3[] velocities = new Vector3[VertCount];
        velocityBuffer.GetData(velocities);
        float gestureTime = 1.0f;
        float damp = Time.time < gestureTime ? (gestureTime - Time.time) / gestureTime : 0.0f;
        float amp = 20.0f;
        float freq = 0.1f;
        for (int i = 2 * GridResX + 2; i < 3 * GridResX - 2; i++)
            velocities[i] = new Vector3(0.0f, 0.0f, Mathf.Sin(Time.time * freq) * damp * amp);
        velocityBuffer.SetData(velocities);
    }


}
