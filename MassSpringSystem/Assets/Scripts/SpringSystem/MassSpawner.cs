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

public class MassSpawner : MonoBehaviour
{
    public GameObject MassPrefab;

    private float     MassUnitSize;
    private ArrayList Primitives = new ArrayList();
    private Vector3[] positions;

    //===========================================================================================
    //Overrides
    //===========================================================================================
    void FixedUpdate()
    {
        int numPositions = Primitives.Count;
        for (int i = 0; i < numPositions; ++i)
            ((GameObject)Primitives[i]).transform.position = TranslateToUnityWorldSpace (positions[i]);
        
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
            Destroy (obj.gameObject);
        Primitives.Clear();

        positions = p;
        int numPositions = positions.Length;
        Primitives.Clear();
        foreach (Vector3 massPosition in positions)
        {
            //translate y to z so we can use Unity's in-built gravity on the y axis.
            Vector3 worldPosition = TranslateToUnityWorldSpace (massPosition);

            Object     springUnit       = Instantiate (MassPrefab, worldPosition, Quaternion.identity);
            GameObject springMassObject = (GameObject) springUnit;
            springMassObject.name = positions.ToString();
            springMassObject.transform.localScale = Vector3.one * MassUnitSize;
            Primitives.Add (springUnit);
           
        }
        
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
        return new Vector3 (gridPosition.x, gridPosition.z, gridPosition.y);
    }  
}
