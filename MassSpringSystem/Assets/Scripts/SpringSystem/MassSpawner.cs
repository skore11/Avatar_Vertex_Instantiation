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

public class MassSpawner : MonoBehaviour
{
    public GameObject MassPrefab;
   // public GameObject character;
   // public  GameObject objectSkeleton;
    
    //public static Transform[] objectTransform;
    public static int index =0;
    private float     MassUnitSize;
    public List<GameObject> Primitives = new List<GameObject>();
    private Vector3[] positions;



    //===========================================================================================

    //===========================================================================================
    //Overrides
    //===========================================================================================

    /*void Start()
    {
        Transform[] objectBones = objectSkeleton.GetComponent<ViewSkeleton>().childNodes;
        for (int i = 0; i < objectBones.Length; i++)
        {
            Debug.Log("Object's bone" + objectBones[i] + "X:" + objectBones[i].transform.position.x + ", Y:" + objectBones[i].transform.position.y + ", Z:" + objectBones[i].transform.position.z);
        }
    }*/



    void Update()//what exactly happens here?
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
    public void SetMassUnitSize         (float length) { MassUnitSize = length; }

    //===========================================================================================
    // Initialisation
    //===========================================================================================

    public void SpawnPrimitives (Vector3[] p)
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
                Debug.Log(springMassObject.transform.position.x + "," + springMassObject.transform.position.y + "," + springMassObject.transform.position.z);
            }
            else
            {
                springMassObject.SetActive(false);
            }
            //Transform[] objectBones = objectSkeleton.GetComponent<ViewSkeleton>().childNodes;
            //for (int i = 0; i < objectBones.Length; i++)
            //{
            //    if (springMassObject.active)
            //    {
            //        if (Vector3.Distance(springMassObject.transform.position, objectBones[i].transform.position) < 0.01f)
            //        {
            //            objectBones[i].transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            //            objectBones[i].transform.position = springMassObject.transform.position;
            //            Debug.Log("springMassObject.transform.position");
            //        }
            //        else
            //        {
            //            Debug.Log("nothing close by");
            //        }
            //    }
            //    Debug.Log("Object's bone after transfering " + objectBones[i] + "X:" + objectBones[i].transform.position.x + ", Y:" + objectBones[i].transform.position.y + ", Z:" + objectBones[i].transform.position.z);
            //}

            /*
            if (Physics.OverlapSphere(springMassObject.transform.position, MassUnitSize / 2f).Length > 0)
            {
            Debug.Log(springMassObject.name + " is inside target mesh " + character.name);
             springMassObject.SetActive(true);
            }
            */

            // ray intersection with mesh of character and hide masses not in side mesh

            index++;
        }
        insideTester.meshCollider.gameObject.SetActive(false);
    }



    //===========================================================================================
    // Position Updating
    //===========================================================================================

    public void UpdatePositions (Vector3[] p)
    {
        positions = p;//check for interfacing with vertex positions of animations
                      //Debug.Log(p);
    
    }

    //===========================================================================================
    // Helper Functions
    //===========================================================================================

    private Vector3 TranslateToUnityWorldSpace (Vector3 gridPosition)
    {
        return new Vector3(
            Mathf.Clamp(gridPosition.x, -100f, 100f),
            Mathf.Clamp(gridPosition.z, -100f, 100f),
            Mathf.Clamp(gridPosition.y, -100f, 100f));
    }


}
