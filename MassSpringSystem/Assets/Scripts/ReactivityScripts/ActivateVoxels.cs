using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Use this script to access the Mass Spring System parameters.
//Send buffers with position and velocity info from here
//The different areas of access are defined in 3 separate functions 
//Define all voxels to be active
//Use this script to define which parts of voxelized model can activate its voxels
//
public class ActivateVoxels : MonoBehaviour {
    public MassSpawner voxels;
    public GameObject spawner;
	// Use this for initialization
	public void Start () {
        //For testing initially make all the voxels reactive
        //See if pausing the game and assigning activity is doable.
        float toChange = spawner.GetComponent<MassSpringSystem3D>().SpringStiffness;
        float toChange1 = spawner.GetComponent<MassSpringSystem3D>().Damping;
        float toChange2 = spawner.GetComponent<MassSpringSystem3D>().SpringLength;


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ActivateonAnim()
    {
        // Apply Forces
        //Bone joints positions and velocities converted into forces
        //Apply bone forces to localtoWorld matrix of corresponding mass

        //get bone localToWorld positions
        //define each mass spring as a rigid body
        //get Transform of nearest gameobject mass in mass Spring system 3D
        //apply velocity of bone to mass rigid body
        //calculate force from velocity and apply to mass spring system OR
        //calculate velocity every frame and send it shader via velocity buffer

    }

    public void ActivateonMouse()
    {
        //ApplyForce or
        //Apply changes to float value during certain behaviors


    }

    public void ActivateonEnv()
    {
        //Apply Force 
        //Changes in positions/velocity on interaction with environment
    }
}
