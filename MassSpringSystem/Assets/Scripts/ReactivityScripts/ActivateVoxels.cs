using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Use this script to access the Mass Spring System parameters.
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

    public void Activate()
    {
        //ApplyForce or
        //Apply changes to float value during certain behaviors
    }
}
