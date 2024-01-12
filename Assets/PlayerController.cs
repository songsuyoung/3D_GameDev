using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{

    Transform player;
    Animator playerAnimator;
    [SerializeField]
    float speed;
    [SerializeField]
    float jumpHeight = 3f;

    Vector3 dir;

    [SerializeField]
    Transform playerCamera;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        playerAnimator = player.GetComponent<Animator>();
        dir = Vector3.zero;
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
        moveDirection.y = 0; //y축회전을 수행하기 위해서 강제로 0으로 초기화해준다.

        // 카메라와 캐릭터 사이의 간격에 대해 회전
        if (moveDirection != Vector3.zero)
        {
            player.forward = Vector3.Slerp(player.forward, moveDirection, speed*Time.deltaTime);
        }

        // 이동
        player.transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        float offset = 0.5f + Input.GetAxis("Sprint") * 0.5f; //Shift 누를 시 스피드 추가 예정

        playerAnimator.SetFloat("Horizontal", moveDirection.x * offset);
        playerAnimator.SetFloat("Vertical", moveDirection.z * offset);

        //Jump 추가예정
    }


    void Attack()
    {

    }
}
