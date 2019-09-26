using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class TestMelt : MonoBehaviour, IStorable
{

    public MassSpringSystem3D Ms3D;
    public MassSpawner3D spawner;
    public KeyCode m_key = KeyCode.M;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(m_key))
        {
            Ms3D.GetComponent<MassSpringSystem3D>().Gravity = true;
        }

        if (Input.GetKeyUp(m_key))
        {
            Ms3D.GetComponent<MassSpringSystem3D>().Gravity = false;
        }
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    //Debug.Log(other.name);
    //    if (other.tag == "ThreatenedSpawner")
    //    {
    //        Ms3D.GetComponent<MassSpringSystem3D>().Gravity = true;
    //    }


    //}


    //void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "ThreatenedSpawner")
    //    {
    //        Ms3D.GetComponent<MassSpringSystem3D>().Gravity = false;

    //    }

    //}
}
