using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallOctree : MonoBehaviour {
    public Transform MyContainer;
   public List<Transform> track;
    public float radius;
   // public GameObject myObject;
    public List<GameObject> myObject;
    public Bounds myBounds;
 // track and myOject have to be the same
    //public GameObject boundsTree;
	// Use this for initialization
	void Start () {
       
    }
    void OnDrawGizmos()
    {

        BoundsOctree<GameObject> boundsTree = new BoundsOctree<GameObject>(15, MyContainer.position, 1, 1.5f);
        PointOctree<GameObject> pointTree = new PointOctree<GameObject>(15, MyContainer.position, 1.0f);
        for (int i = 0; i < myObject.Count; i++)
        {
            boundsTree.Add(myObject[i], myBounds);
            boundsTree.DrawAllBounds(); // Draw node boundaries

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, radius);
            boundsTree.DrawAllObjects(); // Draw object boundaries
                                         //.DrawCollisionChecks(); // Draw the last *numCollisionsToSave* collision check boundaries

            pointTree.DrawAllBounds(); // Draw node boundaries
            pointTree.DrawAllObjects(); // Mark object positions

            if (track[i])
                myBounds.center = track[i].position;
        }
        }
    }//
  /* void Update()
   {

        if (track)
           transform.position= track.position;//updates the behavior manager's position to the 
    }
    */
//}
