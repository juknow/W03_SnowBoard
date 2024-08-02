using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    float basicspeed = 1.0f;
    float tilt;
    public float turnStrength = 0.3f;

    Vector3 velocity;
    Vector3 localVel;
    float curDir = 0f;

    Vector3 curNormal = Vector3.up;//(0,1,0)
    float distGround, distGroundL, distGroundR;
    float boardDeltaY;

    Vector3 normalGround, posGround;

    Vector3 localRot;

    Rigidbody rg;
    public Transform R, L;


    RaycastHit hit;// 레이캐스트 충돌 변수 저장  

    float magnitude;

    Vector3 ang;

    private void Start()
    {
        curDir = transform.rotation.eulerAngles.y; // 플레이어의 y축 회전값 저장. 즉 진행 방향을 저장. 플레이여 a,d로 회전하는 방향 저장.
        rg = GetComponent<Rigidbody>();
    }


    void Update()
    {
        tilt = Input.GetAxis("Horizontal"); //  a ,d 입력값 -1~1. -1 과 1사이로 나와야한다.
        //Debug.Log(tilt);

        // Draw raycast from L
        Debug.DrawRay(L.position, -curNormal * 10, Color.red);
        if (Physics.Raycast(L.position, -curNormal, out hit)) // L에서 래이캐스트를 지면 방향 (0,-1,0)으로 쏴서 충돌이 발생하면 hit 변수에 충돌 정보를 저장
        {
            //Debug.Log("L HIT :" + hit);
            posGround = hit.point; // 레이 캐스트가 충돌한 지점을 포스 그라운드에 저장
            distGroundL = hit.distance; //L에서 레이 캐스트 충돌 지점 까지의 거리
            normalGround = hit.normal;// 충돌한 지점의 법선 벡터. (0,1/2 , 루트3/2) 이런식으로...
        }
        // Draw raycast from R
        Debug.DrawRay(R.position, -curNormal * 10, Color.blue);
        if (Physics.Raycast(R.position, -curNormal, out hit)) // R에서 래이캐스트를 지면 방향 (0,-1,0)으로 쏴서 충돌이 발생하면 hit 변수에 충돌 정보를 저장
        {
            //Debug.Log("R HIT :" + hit);
            posGround = (posGround + hit.point) / 2f; // 아까 L 충돌 지점 정보 저장했던 포스 그라운드에 R 충돌 지점을 더하고 2로 놔눠 L,R충돌지점 사이를 저장한다.
            if (hit.point.y > posGround.y) // Rhit 포인트의 y축값 즉 높이가 posground 보다 높으면 => R hit 이 Lhit보다 높으면 => 플레이어가 L방향으로 기울어져있으면
                posGround.y = hit.point.y; // pos그라운드의 높이는 R hit의 높이랑 같아진다.
            distGroundR = hit.distance; //R에서 레이 캐스트 충돌 지점 까지의 거리
        }
        distGround = (distGroundL + distGroundR) / 2f; // L 과 R 충돌 지점 거리 평균
        SnowTrail();
    }

    void SnowTrail()
    {
        if (distGround < 0.2f) // 지면과의 평균 높이가 0.2f 보다 낮으면
        {
            localRot = transform.localRotation.eulerAngles; // 현재 오브젝트의 로컬 회전값을 오일러각도로 변환하여 저장.
            Debug.Log("현재 로컬 값 : " + localRot);
            localRot.z = (distGroundR - distGroundL) * 100; // 오일러 각도로 변환한 로컬 회전값의 z축을 L R 충돌 거리의 차이에 100을 곱함. R이 더 길면 양수 L이 더 길면 음수 => L방향으로 플레이어가 기울면 양수 ,R방향으로 플레이어가 기울면 음수 => 물체의 좌우 기울기를 반영한 회전값 => 물체의 좌우 지면 거리 차이에 따른 기울기 값 
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(localRot), Time.deltaTime * 10); // 현재의 로컬 회전값을 목표회전값으로 초당 10 deltaTime 만큼 회전 => z축의 기울기를 바꾼다. => a,d 입력하면 그 방향으로 보드 기울듯이 함
            Debug.Log("바꾼 로컬 값 : " + transform.localRotation);
        }
    }
    void FixedUpdate()
    {
        boardDeltaY = 0;
        boardDeltaY += (float)(tilt * (1 + velocity.magnitude) / 10f); // tilt 값에 따라 누적 더하거나 뺌
        //Debug.Log(boardDeltaY);
        ang = transform.eulerAngles; // 현재 물체의 회전값을 오일러 각도로 변환하여 저장.
        //Debug.Log(ang);
        ang.y += boardDeltaY;
        transform.eulerAngles = // 현재 물체의 y축 회전값에 tilt값을 더함.
        velocity = rg.velocity; // 현재 물체의 velocity 저장
        localVel = transform.InverseTransformDirection(velocity); // 월드 좌표계의 velocity 벡터를 로컬 좌표계로 변환
        localVel.x -= localVel.x * turnStrength; // 로컬 좌표계의 x값을 tilt만큼 바꿈. 이건 로테이션을 바꾼 값이 아닌 그냥 a,d 누른 만큼 좌우로 이동하냐 만 보는 것임.

        // Apply basicspeed as a force in the forward direction
        Vector3 forwardForce = transform.forward * basicspeed;
        rg.AddForce(forwardForce);



        //Simulate friction by increasing the drag depending of the speed
        magnitude = velocity.magnitude; // velocity  벡터의 크기 계산;
        if (magnitude < 3) // 속도 크기가 3보다 작으면
            rg.drag = 0; // 물체의 drag는 0. drag는 저항력
        else // 속도가 3보다 크다면
            rg.drag = magnitude / 20000000f; // 드래그를 조금 더 줌

        rg.angularVelocity = Vector3.zero; // 물체의 회전속도를 0으로 설정. 물체가 지맘대로 회전하는 것을 막음.
        if (distGround > 0.2f)
        {
            //Air 
        }
        else
        {
            //On the ground/snow
            rg.velocity = transform.TransformDirection(localVel);
        }
    }
}
