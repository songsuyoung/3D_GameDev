using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{

    Transform player;
    Animator playerAnimator;
    Rigidbody playerRigidbody;
    [SerializeField]
    float speed = 10f;
    [SerializeField]
    float jumpHeight = 3f;
    [SerializeField]
    float rotateSpeed = 5f;
    Vector3 dir= Vector3.zero;

    [SerializeField]
    Transform playerCamera;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        playerAnimator = player.GetComponent<Animator>();
        playerRigidbody= player.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
    }
    void Move()
    {

        //가로로 A-D
        dir.x = Input.GetAxis("Horizontal");
        //세로로, W-S키
        dir.z = Input.GetAxis("Vertical");
        //카메라가 바라보는 방향으로 플레이어 방향을 이동시킨다.
        // 카메라의 forward 방향을 기반으로 이동 방향 계산
        Vector3 cameraForward = playerCamera.forward; //카메라가 바라보는 앞 방향
        Vector3 cameraRight = playerCamera.right; //원하는 방향(즉 (x,0,0) + (0,0,z) 정규화 진행)
        Vector3 moveDirection = (cameraForward * dir.z + cameraRight * dir.x).normalized;

        // 이동
        player.transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // 카메라와 캐릭터 사이의 간격에 대해 회전
        if (moveDirection != Vector3.zero)
        {
            //회전에 대해 다시생각해

        }
       
        float offset = 0.5f + Input.GetAxis("Sprint") * 0.5f; //Shift 누를 시 스피드 추가 예정

        playerAnimator.SetFloat("Horizontal", moveDirection.x * offset);
        playerAnimator.SetFloat("Vertical", moveDirection.z * offset);

        //Jump 추가예정
    }


    void Attack()
    {

    }
}
