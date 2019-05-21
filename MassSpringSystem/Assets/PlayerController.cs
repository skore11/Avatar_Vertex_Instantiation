using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    public float speed;

    private MassSpringSystem3D sb;

    void Start()
    {
        sb = GetComponent<MassSpringSystem3D>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        float moveUpDown = Input.GetAxis("UpandDown");

        Vector3 movement = new Vector3 (moveHorizontal, moveUpDown, moveVertical);
        transform.Translate(movement);
        
        sb.TranslateMassSpringPositions(movement);
    }
}
