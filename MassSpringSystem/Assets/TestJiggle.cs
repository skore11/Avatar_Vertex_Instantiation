using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJiggle : MonoBehaviour
{
    public MassSpringSystem3D Ms3D;
    public MassSpawner3D spawner;
    public KeyCode m_key = KeyCode.J;
    //float speed = 1.0f;
    //float amount = 10.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(m_key))
        {
            Vector3[] jiggleForces = new Vector3[Ms3D.VertCount];
            float xNoise = Random.Range(-50, 20);
            float yNoise = Random.Range(-50, 20);
            float zNoise = Random.Range(-50, 50);
            foreach (var indexedPrimitive in spawner.Primitives)
            {
                int index = indexedPrimitive.Key;

                GameObject primi = indexedPrimitive.Value;

                jiggleForces[index].x = xNoise;
                jiggleForces[index].y = -zNoise;
                jiggleForces[index].z = yNoise;

                Ms3D.externalForcesBuffer.SetData(jiggleForces);
            }

            if (Input.GetKeyUp(m_key))
            {
                Ms3D.GetComponent<MassSpringSystem3D>().Gravity = false;
            }

        }
    }
}

    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "ExcitedSpawner")
    //    {
    //        Vector3[] jiggleForces = new Vector3[Ms3D.VertCount];
    //        float xNoise = Random.Range (-50, 20);
    //        float yNoise = Random.Range (-50, 20);
    //        float zNoise = Random.Range (-50, 50);

    //        foreach (var indexedPrimitive in spawner.Primitives)
    //        {
    //            int index = indexedPrimitive.Key;

    //            GameObject primi = indexedPrimitive.Value;

    //            jiggleForces[index].x = xNoise;
    //            jiggleForces[index].y = -zNoise;
    //            jiggleForces[index].z = yNoise;
    //        }
    //        Ms3D.externalForcesBuffer.SetData(jiggleForces);
    //    }


    //}




    //void OnTriggerStay(Collider other)
    //{
    //    if (other.tag == "ExcitedSpawner")
    //    {
    //        Vector3[] jiggleForces = new Vector3[Ms3D.VertCount];
    //        float xNoise = Random.Range (-50, 20);
    //        float yNoise = Random.Range (-50, 20);
    //        float zNoise = Random.Range (-50, 50);

    //        foreach (var indexedPrimitive in spawner.Primitives)
    //        {
    //            int index = indexedPrimitive.Key;

    //            GameObject primi = indexedPrimitive.Value;

    //            jiggleForces[index].x = xNoise;
    //            jiggleForces[index].y = -zNoise;
    //            jiggleForces[index].z = yNoise;
    //        }
    //        Ms3D.externalForcesBuffer.SetData(jiggleForces);
    //    }
    //}
    //void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "ExcitedSpawner")
    //    {
    //        Vector3[] jiggleForces = new Vector3[Ms3D.VertCount];

    //        foreach (var indexedPrimitive in spawner.Primitives)
    //        {
    //            int index = indexedPrimitive.Key;

    //            GameObject primi = indexedPrimitive.Value;

    //            jiggleForces[index].x = 0.0f;
    //            jiggleForces[index].y = 0.0f;
    //            jiggleForces[index].z = 0.0f;
    //        }
    //        Ms3D.externalForcesBuffer.SetData(jiggleForces);
    //    }

    //}



