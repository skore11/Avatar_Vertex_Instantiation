using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
TODO: when the internal voxel tests are being run, if a certain voxel is in proximity to 
a surface vertex which has a corresponding skinning weight, then transfer said skinning weight 
to the internal voxel
*/
public class InsideTester : MonoBehaviour {

    public MeshCollider meshCollider;

    private bool concaveHull; // TODO: recheck if needed from outside
    public float distance=100f;
 
    Ray right   = new Ray(Vector3.zero , -Vector3.right);
    Ray left    = new Ray(Vector3.zero , -Vector3.left);
    Ray up      = new Ray(Vector3.zero , -Vector3.up);
    Ray down    = new Ray(Vector3.zero , -Vector3.down);
    Ray forward = new Ray(Vector3.zero , -Vector3.forward);
    Ray back    = new Ray(Vector3.zero , -Vector3.back);
    Ray tempRay = new Ray();
    bool r,l,u,d,f,b;
 
    RaycastHit rightHit   = new RaycastHit();
    RaycastHit leftHit    = new RaycastHit();
    RaycastHit upHit      = new RaycastHit();
    RaycastHit downHit    = new RaycastHit();
    RaycastHit forwardHit = new RaycastHit();
    RaycastHit backHit    = new RaycastHit();
    RaycastHit tempHit    = new RaycastHit();
 
    public bool IsInside(Vector3 position) {
        right.origin = -right.direction * distance + position;
        left.origin = -left.direction * distance + position;
        up.origin = -up.direction * distance + position;
        down.origin = -down.direction * distance + position;
        forward.origin = -forward.direction * distance + position;
        back.origin = -back.direction * distance + position;

        r = meshCollider.Raycast(right, out rightHit, distance);
        l = meshCollider.Raycast(left, out leftHit, distance);
        u = meshCollider.Raycast(up, out upHit, distance);
        d = meshCollider.Raycast(down, out downHit, distance);
        f = meshCollider.Raycast(forward, out forwardHit, distance);
        b = meshCollider.Raycast(back, out backHit, distance);

        bool In = true;
        return (r && l && u && d && f && b);
        if (r && l && u && d && f && b)
        {
            if (ConcaveHull(right, rightHit)) In = false;
            else if (ConcaveHull(left, leftHit)) In = false;
            else if (ConcaveHull(up, upHit)) In = false;
            else if (ConcaveHull(down, downHit)) In = false;
            else if (ConcaveHull(forward, forwardHit)) In = false;
            else if (ConcaveHull(back, backHit)) In = false;
            else { In = true; concaveHull = false; }
        }
        else
        {
            In = false;
        }
        return In;
    }
 
     bool ConcaveHull(Ray ray, RaycastHit hit){
 
 
         tempRay.origin = transform.position;
         tempRay.direction = -ray.direction;
         float customDistance = distance-hit.distance;
         int lastPoint = hit.triangleIndex;
 
         while(meshCollider.Raycast(tempRay, out tempHit, customDistance)){
 
             if(tempHit.triangleIndex == lastPoint) break;
             lastPoint = tempHit.triangleIndex;
             customDistance = tempHit.distance;
             ray.origin = -ray.direction * customDistance + transform.position;
 
             if(!meshCollider.Raycast(ray, out tempHit, customDistance)) {
 
                 concaveHull = true;
                 return true;
 
             }
 
             if(tempHit.triangleIndex == lastPoint) break;
             lastPoint = tempHit.triangleIndex;
             customDistance -= tempHit.distance;
 
         }
 
         return false;
 
     }
 
}
