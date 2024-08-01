using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector3 rawInputForward;
    private Vector3 rawInputTurn;
    private Vector3 realMovement;
    private Quaternion turnToForward;
    private Vector3 smoothInputMovement;
    public Rigidbody rb;
    public float movementSmoothingSpeed = 1f;
    public float rotationSpeed = 10f;
    //public Camera playerCamera;

    private bool isOnGround = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CalculateMovementInputSmoothing();
    }

    public void PlayerMove(InputAction.CallbackContext context)
    {
        Vector2 inputMovement = context.ReadValue<Vector2>();

        rawInputForward = new Vector3(0, 0, inputMovement.y);
        rawInputTurn = new Vector3(inputMovement.x, 0, 0);
        //if (inputMovement.x > 0.01f || inputMovement.x < -0.01f)
        //{
        //    turnToForward = Quaternion.FromToRotation(Vector3.forward, rawInputTurn); //* transform.rotation;// 0 0 1�� ȸ����Ű�� ���ʹϾ�
        //    //transform.rotation *= turnToForward;
        //}
        Quaternion moveToForward = Quaternion.FromToRotation(Vector3.forward, transform.forward);
        realMovement = moveToForward * rawInputForward;
    }

    //public void PlayerTurn(InputAction.CallbackContext context)
    //{
    //    double press = context.ReadValue<double>();
    //    int a = 0;
    //}

    void CalculateMovementInputSmoothing()
    {
        if (rawInputTurn.magnitude > 0.01f)
        {
            Vector3 v = rb.velocity;
            Quaternion turnQuat;
            if (rawInputTurn.x > 0.01f) // ����
            {
                turnQuat = Quaternion.FromToRotation(Vector3.forward, new Vector3(Time.unscaledDeltaTime, 0, 1 - Time.unscaledDeltaTime));
                turnToForward = turnQuat * rb.rotation;
                rb.rotation = /*turnToForward * Time.timeScale;*/ Quaternion.Slerp(rb.rotation, turnToForward, rotationSpeed * Time.deltaTime);
                //rb.velocity = turnQuat * rb.velocity;
                Vector3 v3 = turnQuat * rb.velocity;
                rb.velocity = Vector3.Slerp(rb.velocity, v3, rotationSpeed * Time.deltaTime);
            }
            else if (rawInputTurn.x < -0.01f) // ����
            {
                turnQuat = Quaternion.FromToRotation(Vector3.forward, new Vector3(-Time.unscaledDeltaTime, 0, 1 - Time.unscaledDeltaTime));
                turnToForward = turnQuat * rb.rotation;
                rb.rotation = /*turnToForward * Time.timeScale;*/ Quaternion.Slerp(rb.rotation, turnToForward, rotationSpeed * Time.deltaTime);
                //rb.velocity = turnQuat * rb.velocity;
                Vector3 v3 = turnQuat * rb.velocity;
                rb.velocity = Vector3.Slerp(rb.velocity, v3, rotationSpeed * Time.deltaTime);
            }
        }

        //smoothInputMovement = Vector3.Lerp(smoothInputMovement, realMovement, Time.deltaTime * movementSmoothingSpeed);
        //transform.position += smoothInputMovement;
        if (isOnGround)
            rb.AddForce(realMovement.normalized);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isOnGround = true;
    }

    private void OnCollisionStay(Collision collision)
    {

        if (collision.gameObject.CompareTag("Slope"))
        {
            isOnGround = true;
            Vector3 normal = collision.GetContact(0).normal;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;

            // ��ü�� ������ ������ �°� ȸ��
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isOnGround = false;
    }
}
