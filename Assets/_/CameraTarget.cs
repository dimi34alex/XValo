using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class CameraTarget : MonoBehaviour {

    public enum Axis {
        XZ,
        XY,
    }

    [SerializeField] private Axis axis = Axis.XZ;
    [SerializeField] private float moveSpeed = 50f;



    private void Update() {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W)) {
            moveY = +1f;
        }
        if (Input.GetKey(KeyCode.S)) {
            moveY = -1f;
        }
        if (Input.GetKey(KeyCode.A)) {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D)) {
            moveX = +1f;
        }
        if (Input.GetKey(KeyCode.Q)) {
            transform.eulerAngles += new Vector3(0, +75, 0) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E)) {
            transform.eulerAngles += new Vector3(0, -75, 0) * Time.deltaTime;
        }

        Vector3 moveDir;

        switch (axis) {
            default:
            case Axis.XZ:
                moveDir = new Vector3(moveX, 0, moveY).normalized;
                break;
            case Axis.XY:
                moveDir = new Vector3(moveX, moveY).normalized;
                break;
        }
        
        if (moveX != 0 || moveY != 0) {
            // Not idle
        }

        if (axis == Axis.XZ) {
            //moveDir = UtilsClass.ApplyRotationToVectorXZ(moveDir, 30f);
        }

        Vector3 forward = transform.forward;
        forward.y = 0;
        transform.position += (forward * moveY + transform.right * moveX) * moveSpeed * Time.deltaTime;
        //transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

}
