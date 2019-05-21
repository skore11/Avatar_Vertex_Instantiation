using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMelt : MonoBehaviour
{

    public MassSpringSystem3D Ms3D;
    public MassSpawner3D spawner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name);
        if (other.tag == "ThreatenedSpawner")
        {
            Ms3D.GetComponent<MassSpringSystem3D>().Gravity = true;
        }


    }


    void OnTriggerExit(Collider other)
    {
        if (other.tag == "ThreatenedSpawner")
        {
            Ms3D.GetComponent<MassSpringSystem3D>().Gravity = false;

        }

    }
}
