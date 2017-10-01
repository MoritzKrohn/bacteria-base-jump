using UnityEngine;
using System.Collections;

public class Spinning : MonoBehaviour
{
    public float Speed = 2f;
    public bool Rotate3D = false;

    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, Speed);
        if (Rotate3D)
            transform.Rotate(Vector3.forward, Speed);
    }
}
