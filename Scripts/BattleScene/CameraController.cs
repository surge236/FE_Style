using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    Rigidbody cameraRB;

	// Use this for initialization
	void Start () {
        cameraRB = this.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

    }

    private void LateUpdate() {
        cameraRB.MovePosition(new Vector3(cameraRB.position.x + Input.GetAxis("Horizontal View"), 
            cameraRB.position.y + Input.GetAxis("Vertical View"), cameraRB.position.z));
        cameraRB.AddForce(-cameraRB.velocity, ForceMode.VelocityChange);
    }

}
