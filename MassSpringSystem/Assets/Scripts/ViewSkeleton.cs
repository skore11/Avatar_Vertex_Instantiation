using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewSkeleton : MonoBehaviour {

    public Transform rootNode;
    public Transform[] childNodes;
   
    void OnDrawGizmosSelected()
    {
        if (rootNode != null)
        {
            if (childNodes == null|| childNodes.Length == 0)
            {
                //get all joints to draw
                PopulateChildren();
            }


            foreach (Transform child in childNodes)
            {

                if (child == rootNode)
                {
                    //list includes the root, if root then larger, green cube
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(child.position, new Vector3(.1f, .1f, .1f));
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(child.position, child.parent.position);
                    float dist = Vector3.Distance(child.position, child.parent.position);
                    Debug.Log(child.name);
                    Debug.Log(child.position);
                    Debug.Log(child.parent.name);
                    Debug.Log(child.parent.position);
                    Debug.Log(dist);
                    float linepos = 16.0f;
                    for (int s =1; s<=16; s++)
                    {
                        Debug.Log(s);
                        //Debug.Log(linepos);
                        float percentage = s / linepos;
                        Debug.Log(percentage);
                        Vector3 split = Vector3.Lerp(child.position, child.parent.position, percentage);
                        Debug.Log(split);
                        Gizmos.DrawCube(split, new Vector3(0.01f, 0.01f, 0.01f));
                    }
    
    
    

    //Gizmos.DrawCube(child.position, new Vector3(.01f, .01f, .01f));
                }
            }

        }
    }

    public void PopulateChildren()
    {
        childNodes = rootNode.GetComponentsInChildren<Transform>();
    }
}
