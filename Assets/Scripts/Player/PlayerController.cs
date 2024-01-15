using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{

    Transform player;
    Animator playerAnimator;
    //캐릭터 컨트롤러의 경우, 중력이 적용되지 않음. 그래서 직접적으로 중력을 적용
    CharacterController characterController;
    
    [SerializeField]
    float speed;
    [SerializeField]
    float jumpPower;
    [SerializeField]
    Transform playerCamera;


    Vector3 moveDirection;
    Vector3 dir;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        playerAnimator = player.GetComponent<Animator>();
        characterController = player.GetComponent<CharacterController>();
        dir = Vector3.zero;
        moveDirection=Vector3.zero;
    }

    private void Update()
    {
        Move();
    }

    void Move()
    {
        if (characterController.isGrounded)
        {
            // WASD 입력을 받아 이동 방향을 계산 =>즉각적인 반응을 위해서 GetAxisRaw
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            dir = new Vector3(horizontalInput, 0f, verticalInput).normalized;

            // 카메라가 바라보는 방향과 플레이어의 전방 방향을 일치시키기 위해 카메라의 forward 벡터를 사용
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;

            // 카메라가 바라보는 방향의 프로젝션을 제거하여 이동 방향을 얻음 => y축 회전
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            moveDirection = (cameraForward.normalized * dir.z + cameraRight.normalized * dir.x).normalized;

            if (moveDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                //부드러운 이동보다 즉각적인 회전을 수행하기 위해서 Quaternion.RotateTowards ->Quaternion.Slerp로 변경
                //player.rotation = Quaternion.Slerp(player.rotation, toRotation, 60f * Time.deltaTime);
                player.rotation = toRotation;
            }

            float offset = 0.5f + Input.GetAxis("Sprint") * 0.5f; //Shift 누를 시 스피드 추가 예정

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpPower;
                //playerAnimator.SetTrigger("isJump");
            }
            
            playerAnimator.SetFloat("Horizontal", moveDirection.x * offset);
            playerAnimator.SetFloat("Vertical", moveDirection.z * offset);
        }

        moveDirection.y += Physics.gravity.y * Time.deltaTime;
        // 이동
        characterController.Move(moveDirection * speed * Time.deltaTime);

        //점프
    }
    
    void Attack()
    {

    }
}
