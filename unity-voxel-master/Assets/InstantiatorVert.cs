using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatorVert : MonoBehaviour {

    public GameObject dot;//the object to instantiate

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        //int i = 0;
   
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        var matrix = transform.localToWorldMatrix;
        for (var i = 0; i < vertices.Length; i++)
        {
            var crm = Instantiate(dot, matrix.MultiplyPoint3x4(vertices[i]), transform.rotation);
        }
            //for (int vertId = 0; vertId < vertices.Length; vertId++ )
        //{
            //Instantiate(dot, vertices[vertId], transform.rotation);
        //}

	}
}
