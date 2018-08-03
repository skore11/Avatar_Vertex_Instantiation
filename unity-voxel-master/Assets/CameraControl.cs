using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    public float speed = 0.1f;
    private float x;
    private float y;
    private Vector3 rotSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Take transform of the camera and update every frame based on push button and add the speed float to each axis; allow transfor.postion of camera to be updated by a Vector3 oin appropriate button push down; alternatively can use getaxisraw as well.
        if (Input.GetKey(KeyCode.D))
        {
            transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position = new Vector3(transform.position.x - speed, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - speed, transform.position.z);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + speed, transform.position.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - speed);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y , transform.position.z + speed);
        }
        y = Input.GetAxis("Mouse X");
        x = Input.GetAxis("Mouse Y");
        //Debug.Log(x + ":" + y);
        rotSpeed = new Vector3(x, y * -1, 0);
        transform.eulerAngles = transform.eulerAngles - rotSpeed;

    }
}
