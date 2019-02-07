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
using System.Collections.Generic;

public class MassSpawner3D : MonoBehaviour
{
    public GameObject MassPrefab;
    // public GameObject character;
    // public  GameObject objectSkeleton;

    //public static Transform[] objectTransform;
    public static int index =0;
    private float     MassUnitSize;
    public List<GameObject> Primitives = new List<GameObject>();
    private Vector3[] positions;

    public CanvasTouchManager UITouchHandler;

    public bool foundGravityForces = false;
    //===========================================================================================

    //===========================================================================================
    //Overrides
    //===========================================================================================

    void Start()
    {
        
    }



    void FixedUpdate()
    {
        int numPositions = Primitives.Count;
        for (int i = 0; i < numPositions; ++i)
        {
            Primitives[i].transform.position = TranslateToUnityWorldSpace(positions[i]);
        }
        //can use the skinning parameters here
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
        {
            Destroy(obj.gameObject);
        }
        Primitives.Clear();

        positions = p;
        int index = 0;
        InsideTester insideTester = GetComponent<InsideTester>();
        insideTester.meshCollider.gameObject.SetActive(true);
        foreach (Vector3 massPosition in positions)
        {
            //translate y to z so we can use Unity's in-built gravity on the y axis.

            Vector3 worldPosition = TranslateToUnityWorldSpace (massPosition);


            GameObject springMassObject = Instantiate<GameObject>(MassPrefab, worldPosition, Quaternion.identity, this.transform);
            //springMassObject.SetActive(true);//has to be first set to true
            //springMassObject.GetComponent<CsObject>().enabled = true;

            springMassObject.name = "MassObj" + index + " " + massPosition.ToString();
            springMassObject.transform.localScale = Vector3.one * MassUnitSize;
            Primitives.Add(springMassObject);
            if (insideTester.IsInside(springMassObject.transform.position))
            {
                springMassObject.SetActive(true);
                //Check to optimize further
                if (springMassObject.GetComponent<Rigidbody>().useGravity == true)
                {
                    int i = int.Parse(springMassObject.name.Substring(7, springMassObject.name.IndexOf(' ') - 7));//avoid parsing string
                    UITouchHandler.GridTouches.Add(new Vector2(i, UITouchHandler.SimulatedPressure));//Might have to initialize the vector 2 before hand, assign using "=="
                    foundGravityForces = true;
                    // Debug.Log(springMassObject.transform.position.x + "," + springMassObject.transform.position.y + "," + springMassObject.transform.position.z);
                }
            }
            else
            {
                springMassObject.SetActive(false);
            }
            
            index++;
        }
        insideTester.meshCollider.gameObject.SetActive(false);
    }



    //===========================================================================================
    // Position Updating
    //===========================================================================================

    public void UpdatePositions(Vector3[] p)
    {
        positions = p;
                      //Debug.Log(p);

    }

    //===========================================================================================
    // Helper Functions
    //===========================================================================================

    private Vector3 TranslateToUnityWorldSpace(Vector3 gridPosition)
    {
        return new Vector3(
            Mathf.Clamp(gridPosition.x, -100f, 100f),
            Mathf.Clamp(gridPosition.z, -100f, 100f),
            Mathf.Clamp(gridPosition.y, -100f, 100f));
    }


}
