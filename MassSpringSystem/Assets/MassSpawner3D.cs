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
 * 
 * 
 * TODO: Apply the ability to instantiate a gameObject (cubes or spheres) to the grid on mouse push down. 
 * The instantiated object should be connected to the grid with the same mass-spring properties. 
 * Also the ability to change the weight or mass of the cubes by color.
 * 
 * 
 */

using UnityEngine;
using System.Collections.Generic;

public class MassSpawner3D : MonoBehaviour
{
    public GameObject MassPrefab;

    public static int index =0;
    private float     MassUnitSize;
    private float     MassUnitMass;
    public Dictionary<int, GameObject> Primitives = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> NonPrimitivesprimitive = new Dictionary<int, GameObject>();
    public Vector3[] nextPositions = null;
    

    public CanvasTouchManager UITouchHandler;
    //internal int childCount;

    //===========================================================================================

    //===========================================================================================
    //Overrides
    //===========================================================================================

    void Start()
    {
        //Debug.Log(MassPrefab.GetComponent<Rigidbody>().mass);
    }



    void FixedUpdate()
    {
        //insert skeleton
        if (this.nextPositions == null || this.nextPositions.Length == 0)
        {
            return;
        }
        foreach (var indexedPrimitive in Primitives)
        {
            //Vector3 newPosition = TranslateToUnityWorldSpace(nextPositions[indexedPrimitive.Key]);
            //print(indexedPrimitive.Key);
            Vector3 newPosition = nextPositions[indexedPrimitive.Key];

            GameObject primi = indexedPrimitive.Value;
            Rigidbody rb = primi.GetComponent<Rigidbody>();

            //if (rb)
            //{

            //    Vector3 dist = newPosition - rb.position;
            //    //print(dist);
            //    rb.AddForce(dist * Time.deltaTime);

            //    //rb.position = newPosition;

            //}
            //else
            //{
                primi.transform.position = newPosition;
            //}
        }
        this.nextPositions = null;
        // Debug.Log(MassPrefab.GetComponent<Rigidbody>().velocity);
        //float moveHorizontal = Input.GetAxis("Horizontal");
        //float moveVertical = Input.GetAxis("Vertical");
        //float moveUpDown = Input.GetAxis("UpandDown");

        //Vector3 movement = new Vector3 (moveHorizontal, moveUpDown, moveVertical);
        //transform.Translate(movement);
    }

    //===========================================================================================
    // Setter Functions
    //===========================================================================================
    public void SetMassUnitSize(float length) { MassUnitSize = length; }
    public void SetMassUnitMass(float mass) { MassUnitMass = mass; }

    //===========================================================================================
    // Initialisation
    //===========================================================================================

    public void SpawnPrimitives(Vector3[] p)
    {
        foreach (GameObject obj in Primitives.Values)
        {
            Destroy(obj.gameObject);
        }
        Primitives.Clear();

        int index = 0;
        
        InsideTester insideTester = GetComponent<InsideTester>();
        insideTester.meshCollider.gameObject.SetActive(true);

                
        foreach (Vector3 massPosition in p)
        {
            //translate y to z so we can use Unity's in-built gravity on the y axis.

            //Vector3 worldPosition = TranslateToUnityWorldSpace (massPosition);
            Vector3 worldPosition = massPosition;
            //Check to see if each mass's world position is within the convex hull of character's mesh collider
            //If so activate gameObject Mass
            if (insideTester.IsInside(worldPosition))
            {
                GameObject springMassObject = Instantiate<GameObject>(MassPrefab, worldPosition, Quaternion.identity, this.transform);
                //print(index);
                springMassObject.name = "MassObj" + index + " " + massPosition.ToString();
                springMassObject.transform.localScale = Vector3.one * MassUnitSize;
                // set the mass of the object:
                Rigidbody rb = springMassObject.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.mass = MassUnitMass;
                    Primitives[index] = springMassObject;
                    springMassObject.SetActive(true);
                }
                //Primitives[index] = springMassObject;
                //springMassObject.SetActive(true);
                else
                {
                    NonPrimitivesprimitive[index] = springMassObject;
                    springMassObject.SetActive(false);

                }
            }
            index++;
            
        }

        
       
        //insideTester.meshCollider.gameObject.SetActive(false);
        //Debug.Log(this.gameObject.name + " spawned " + Primitives.Count + " primitives/voxels");
    }



    //===========================================================================================
    // Position Updating in Mass Spring System
    //===========================================================================================

        //Changed from Vector3[]
    public void UpdatePositions(Vector3[] p)
    {
        nextPositions = p;
    }

    //===========================================================================================
    // Helper Functions during spawning and updating positions in world space
    // As of now it is clamped to a certain bound in 3 axes
    //===========================================================================================

    public Vector3 TranslateToUnityWorldSpace(Vector3 gridPosition)
    {
        return new Vector3(
            Mathf.Clamp(gridPosition.x, -1000f, 1000f),
            Mathf.Clamp(gridPosition.y, -1000f, 1000f),
            Mathf.Clamp(gridPosition.z, -1000f, 1000f));
    }


}
