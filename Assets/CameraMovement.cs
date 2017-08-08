using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.W))
        {
            gameObject.transform.position += Vector3.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            gameObject.transform.position += Vector3.down;
        }
        if (Input.GetKey(KeyCode.A))
        {
            gameObject.transform.position += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            gameObject.transform.position += Vector3.right;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            gameObject.GetComponent<Camera>().orthographicSize -= 0.5f;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            gameObject.GetComponent<Camera>().orthographicSize += 0.5f;
        }

    }
}
