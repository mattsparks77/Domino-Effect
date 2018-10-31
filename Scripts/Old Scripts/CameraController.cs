using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform target;
    public GameObject player;
    // Use this for initialization
    private Vector3 offset;         //Private variable to store the offset distance between the player and camera

    public float cameraHeight = 20.0f;
    // Use this for initialization
    void Start()
    {
        //Calculate and store the offset value by getting the distance between the player's position and camera's position.
//        offset = transform.position - player.transform.position;
//
//        transform.position = offset;
		player = GameObject.FindGameObjectWithTag("Player");
    }

    // LateUpdate is called after Update each frame

    void LateUpdate()
    {
        //transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        //transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
    }
    //void Update()
    //{
    //    Vector3 pos = player.transform.position;
    //    //pos.y += cameraHeight;
    //    transform.position = pos;
    //}

    // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
    //transform.position = new Vector3(transform.position.x , transform.position.y , player.transform.position.z );
    //transform.rotation = player.transform.rotation;
    //transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);

}
