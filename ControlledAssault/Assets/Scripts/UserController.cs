using UnityEngine;
using System.Collections;

public class UserController : UnitController {
    private GameObject playerCamera;
    private float speed;
    private float upDownRoatation = 0.0f;
    //private bool jump = false;
    public UserController(GameObject cam, float speed)
    {
        this.playerCamera = cam;
        this.speed = speed;
    }
    public void ControlUnit(Unit unit)
    {
        unit.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Vector3 moveVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 direction = unit.transform.forward.normalized;
            //unit.transform.position += direction * speed * Time.deltaTime;
            moveVector += direction * speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 direction = -unit.transform.forward.normalized;
            //unit.transform.position += direction * speed * Time.deltaTime;
            moveVector += direction * speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 direction = -unit.transform.right;
            //unit.transform.position += direction * speed * Time.deltaTime;
            moveVector += direction * speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 direction = unit.transform.right;
            //unit.transform.position += direction * speed * Time.deltaTime;
            moveVector += direction * speed;
        }
        if (Input.GetMouseButtonDown(0))
        {
            unit.Swing();
        }
        /*if (Input.GetKey(KeyCode.Space))
        {
            if (!jump)
            {
                unit.GetComponent<Rigidbody>().AddForce(new Vector3(0.0f, 10000.0f, 0.0f));
                jump = true;
            }
        }
        if (jump && unit.gameObject.transform.position.y == 0.4f)
        {
            jump = false;
        }*/
        unit.GetComponent<Rigidbody>().velocity += Vector3.Normalize(moveVector) * speed;
        playerCamera.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y + 0.1f, unit.transform.position.z);
        unit.transform.RotateAround(unit.transform.position, Vector3.up, Input.GetAxis("Mouse X") * 2.5f);
        playerCamera.transform.RotateAround(playerCamera.transform.position, Vector3.up, Input.GetAxis("Mouse X") * 2.5f);
        if (!(Mathf.Abs(upDownRoatation + (Input.GetAxis("Mouse Y") * -2.5f)) > 45.0f))
        {
            playerCamera.transform.RotateAround(playerCamera.transform.position, playerCamera.transform.right, Input.GetAxis("Mouse Y") * -2.5f);
            upDownRoatation += (Input.GetAxis("Mouse Y") * -2.5f);
        }
        unit.twoDImage.transform.localPosition = new Vector3(unit.transform.position.x, unit.transform.position.z, 0);
    }
}
