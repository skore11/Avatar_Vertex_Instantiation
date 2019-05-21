using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMold : MonoBehaviour
{

    public MassSpringSystem3D mySpringSystem;
    public MassSpawner3D mySpawner;
    //public MassSpawner3D spawner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {

        
        //Vector3[] moldForces = new Vector3[Ms3D.VertCount/2];
        //float xNoise = Random.Range (-50, 20);
        //float yNoise = Random.Range (-50, 20);
        //float zNoise = Random.Range (-50, 50);
        //Debug.Log(other.name);
        if (other.tag == "ExcitedSpawner" || other.tag == "AcceptSpawner")
        {
            MassSpawner3D testSpawner = other.GetComponent<MassSpawner3D>();
            MassSpringSystem3D testSpringSystem = other.GetComponent<MassSpringSystem3D>();
            Vector3[] moldPositions = new Vector3[testSpringSystem.VertCount/2];
            //Vector3[] moldPositionsforMyspawner = new Vector3[testSpringSystem.VertCount/2];
            //testSpawner.Primitives[k].transform.position = mySpawner.Primitives[index].transform.position;
            float dist = Vector3.Distance(mySpawner.transform.position,testSpawner.transform.position);
                if (dist < 5.0f)
                {
                //Debug.Log("detected");
                for (int k = 0; k < testSpawner.Primitives.Count/2; k++)
                {
                    testSpawner.transform.GetChild(k/2).position = mySpawner.transform.GetChild(k).position ;
                    //mySpawner.transform.GetChild(k).position = testSpawner.transform.GetChild(k / 2).position;
                    moldPositions[k] = testSpawner.transform.GetChild(k).position;
                    //moldPositionsforMyspawner[k] = mySpawner.transform.GetChild(k).position;
                    //moldForces[k].x = xNoise;
                    //moldForces[k].y = yNoise;
                    //moldForces[k].z = zNoise;
                }
                testSpringSystem.positionBuffer.SetData(moldPositions);
                //mySpringSystem.positionBuffer.SetData(moldPositionsforMyspawner);
                //Ms3D.externalForcesBuffer.SetData(moldForces);
            }
                 
            }
        }


    }

