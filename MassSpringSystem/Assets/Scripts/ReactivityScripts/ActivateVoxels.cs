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

    //public MassSpringSystem3D msSystem3D;
    // public MassSpawner3D spawner;
    public GameObject[] Spawner;
    public Dictionary<int, GameObject> Spawners = new Dictionary<int, GameObject>();

    public CanvasTouchManager UITouch;
    public GameObject Scared;
    //public GameObject Attract;
    //public GameObject Melt;

    //Postions
    private Vector3[] activePos;

    //Velocities
    private Vector3[] activeVels;

    //Forces
    private Vector3[] activeForces;

    // Use this for initialization
    public void Start() {
        for (int i = 0; i < Spawner.Length; i++)
        {
            Spawners.Add(i, Spawner[i]);
        }
    }

    // Update is called once per frame
    void Update() {


        JiggleAll(Spawner[0]);
        GrowAll(Spawner[0]);


    }

    
    private void OnTriggerEnter(Collider other)
    {

    }


    private bool JiggleAll(GameObject jiggle)
    {
        if (Input.GetKey("i"))
        {
            return jiggle.GetComponent<TestJiggle>().enabled = true;
        }
        else
        {
            return jiggle.GetComponent<TestJiggle>().enabled = false;
        }
    }

    private bool GrowAll(GameObject grow)
    {
        if (Input.GetKey("p"))
        {
            return grow.GetComponent<TestGrow>().enabled = true;
        }

        else
        {
            return grow.GetComponent<TestGrow>().enabled = false;
        }

    }
}




