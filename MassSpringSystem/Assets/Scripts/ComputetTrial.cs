using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputetTrial : MonoBehaviour {

    public ComputeShader shader;

    void Start()
    {
        ComputeBuffer buffer = new ComputeBuffer(8 * 8, sizeof(int));

        int kernel = shader.FindKernel("CSMain");
        shader.SetBuffer(kernel, "Result", buffer);
        shader.Dispatch(kernel, 1, 1, 1);

        int[] data = new int[8 * 8];
        buffer.GetData(data);

        for (int i = 0; i < 8 * 8; i++)
        {
            Debug.Log(data[i]); // The result should be 0 1 2 3 … 63
        }

        buffer.Release();
    }
}
