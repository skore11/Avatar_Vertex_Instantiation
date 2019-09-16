using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrow : MonoBehaviour
{
    //public MassSpringSystem3D[] Ms3D; use the array for ontriggerfunctions
    public MassSpringSystem3D Ms3D;
    public MassSpawner3D spawner;
    public float sl;
    
    // Start is called before the first frame update
    void Start()
    {
        sl = Ms3D.SpringLength;
        
    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetKey("t"))
         //if (Ms3D.GetComponent<TestGrow>().enabled == true)
        {
            sl += 0.05f;
            Ms3D.SpringLength = sl;
            foreach (var indexedPrimitive in spawner.Primitives)
            {
                GameObject primi = indexedPrimitive.Value;
                primi.GetComponent<Transform>().localScale += new Vector3(0.05f, 0.05f, 0.05f);
            }
        }
        if (Input.GetKey("u"))
        //else if (Ms3D.GetComponent<TestGrow>().enabled == false)
        {
            sl -= 0.05f;
            Ms3D.SpringLength = sl;
            foreach (var indexedPrimitive in spawner.Primitives)
            {
                GameObject primi = indexedPrimitive.Value;
                primi.GetComponent<Transform>().localScale -= new Vector3(0.05f, 0.05f, 0.05f);
            }
        }

    }

    //void OnTriggerEnter(Collider other)
    //{
    //    //Debug.Log(other.name);
    //    if (other.tag == "ScaredSpawner" || other.tag == "ThreatenedSpawner")
    //    {
    //        float sl = other.GetComponent<MassSpringSystem3D>().SpringLength;
    //        MassSpawner3D testSpawner = other.GetComponent<MassSpawner3D>();
    //        sl *= 2f;
    //        for (int k= 0; k < Ms3D.Length; k++) { 
    //        Ms3D[k].SpringLength = sl;
    //        }
    //        foreach (var indexedPrimitive in testSpawner.Primitives)
    //        {
    //            GameObject primi = indexedPrimitive.Value;
    //            primi.GetComponent<Transform>().localScale += new Vector3(sl, sl, sl);
    //        }
    //    }


    //}


    //void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "ScaredSpawner" || other.tag == "ThreatenedSpawner")
    //    {
    //        float sl = other.GetComponent<MassSpringSystem3D>().SpringLength;
    //        MassSpawner3D testSpawner = other.GetComponent<MassSpawner3D>();
    //        sl /= 2f;
    //        for (int k = 0; k < Ms3D.Length; k++)
    //        {
    //            Ms3D[k].SpringLength = sl;
    //        }
    //        foreach (var indexedPrimitive in testSpawner.Primitives)
    //        {
    //            GameObject primi = indexedPrimitive.Value;
    //            primi.GetComponent<Transform>().localScale -= new Vector3(sl, sl, sl);
    //        }

    //    }

    //}
    //void OnTriggerStay(Collider other)
    //{
    //    if (other.tag == "ScaredSpawner" || other.tag == "ThreatenedSpawner")
    //    {
    //        float sl = other.GetComponent<MassSpringSystem3D>().SpringLength;
    //        MassSpawner3D testSpawner = other.GetComponent<MassSpawner3D>();
    //        sl *= 0.1f;
    //        //Ms3D.SpringLength = sl;
    //        foreach (var indexedPrimitive in testSpawner.Primitives)
    //        {
    //            GameObject primi = indexedPrimitive.Value;
    //            primi.GetComponent<Transform>().localScale += new Vector3(sl, sl, sl);
    //        }
    //    }
    //}

}
