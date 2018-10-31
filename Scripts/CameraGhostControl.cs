using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//this script goes on the player parent
public class CameraGhostControl : NetworkBehaviour {

	[SerializeField] Camera cameraObject;
	[SerializeField] float moveSpeed = 2f;
	[SerializeField] float lookSensitivity = 4f;
	[SerializeField] float maxUp = 85f;
	[SerializeField] float maxDown = -85f;

	bool m_cursorIsLocked = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer){
			cameraObject.enabled = false;
			return;
		}
		Strafe();
		Hover();
		Zoom();
		Look();
		InternalLockUpdate();
	}

	void Strafe(){
		float h = (Input.GetKey(KeyCode.D)?1:0) - (Input.GetKey(KeyCode.A)?1:0);
		MoveOnAxis(transform.right, h);
	}

	void Hover(){
		float c = (Input.GetKey(KeyCode.Space)?1:0) - (Input.GetKey(KeyCode.LeftShift)?1:0);
		MoveOnAxis(transform.up, c);
	}

	void Zoom(){
		float v = (Input.GetKey(KeyCode.W)?1:0) - (Input.GetKey(KeyCode.S)?1:0);
		MoveOnAxis(cameraObject.transform.forward, v);
	}

	void MoveOnAxis(Vector3 axis, float magnitude){
		transform.position += axis.normalized * magnitude * moveSpeed * Time.deltaTime;
	}

	//courtesy of Unity's Standard Assets
	void Look(){
		float yRot = Input.GetAxis("Mouse X") * lookSensitivity;
		float xRot = Input.GetAxis("Mouse Y") * lookSensitivity;

		transform.rotation *= Quaternion.Euler (0f, yRot, 0f);
		cameraObject.transform.localRotation *= Quaternion.Euler (-xRot, 0f, 0f);

		cameraObject.transform.localRotation = ClampRotationAroundXAxis(cameraObject.transform.localRotation);
	}

	//courtesy of Unity's Standard Assets
	Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		


		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

		angleX = Mathf.Clamp (angleX, maxDown, maxUp);

		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}

	private void InternalLockUpdate()
	{
//		if(Input.GetKeyUp(KeyCode.Escape))
//		{
//			m_cursorIsLocked = false;
//		}
//		else if(Input.GetMouseButtonUp(0))
//		{
//			m_cursorIsLocked = true;
//		}
//
//		if (m_cursorIsLocked)
//		{
//			Cursor.lockState = CursorLockMode.Locked;
//			Cursor.visible = false;
//		}
//		else if (!m_cursorIsLocked)
//		{
//			Cursor.lockState = CursorLockMode.None;
//			Cursor.visible = true;
//		}
	}
}
