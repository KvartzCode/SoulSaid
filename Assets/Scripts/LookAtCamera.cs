using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public bool lockRotationY = true;

    // Update is called once per frame
    void Update()
    {
        var lookPoint = Camera.main.transform.position;
        lookPoint.y = lockRotationY ? transform.position.y : lookPoint.y;
        transform.LookAt(lookPoint);
    }
}
