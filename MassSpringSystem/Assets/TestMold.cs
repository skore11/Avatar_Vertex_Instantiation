using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    
    public class TestMold : MonoBehaviour, IStorable
    {
    
        public MassSpringSystem3D mySpringSystem;
        public MassSpawner3D mySpawner;
        public float radius = 0.1f;
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
                Vector3[] moldPositions = new Vector3[testSpringSystem.VertCount / 2];
                //Vector3[] moldPositionsforMyspawner = new Vector3[testSpringSystem.VertCount/2];
                //testSpawner.Primitives[k].transform.position = mySpawner.Primitives[index].transform.position;
                float dist = Vector3.Distance(mySpawner.transform.position, testSpawner.transform.position);
                if (dist < 5.0f)
                {
                    //string debugText = "";
                    foreach (var indexmass in testSpawner.Primitives)
                    {
                        int index = indexmass.Key;
                        GameObject mass = indexmass.Value;
                        // use overlapsphere to check whether a voxel of mine is close:
                        foreach (Collider coll in Physics.OverlapSphere(mass.transform.position, radius))
                        {
                            if (coll.transform.parent == this.transform)
                            {
                                Vector3 force = coll.transform.position - mass.transform.position;
                                //debugText += "\nAdding force from " + mass.name + " to " + coll.gameObject.name + " of " + force;
                                break;
                            }
                        }
                        //mass.GetComponent<Rigidbody>().AddForce(force);
                        //mySpawner.transform.GetChild(k).position = testSpawner.transform.GetChild(k / 2).position;
                        //Debug.Log(testSpawner.transform.GetChild(k).name);
                        //moldPositions[k] = testSpawner.transform.GetChild(k).position;
                        //moldPositionsforMyspawner[k] = mySpawner.transform.GetChild(k).position;
                        //moldForces[k].x = xNoise;
                        //moldForces[k].y = yNoise;
                        //moldForces[k].z = zNoise;
                    }
                    //Debug.Log(debugText);
                    //testSpringSystem.positionBuffer.SetData(moldPositions);
                    //mySpringSystem.positionBuffer.SetData(moldPositionsforMyspawner);
                    //Ms3D.externalForcesBuffer.SetData(moldForces);
                }
            }
        }


    }

