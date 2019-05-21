using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAround : MonoBehaviour
{
    public Transform[] target;
    public float speed;

    private int currentPos;
    // Start is called before the first frame update
    
    // Update is called once per frame
    void Update()
    {
        if (transform.position != target[currentPos].position)
        {
            Vector3 pos = Vector3.MoveTowards(transform.position,target[currentPos].position,speed*Time.deltaTime);
            GetComponent<Rigidbody>().MovePosition(pos);
        }
        else currentPos = (currentPos + 1) % target.Length;
    }
}
