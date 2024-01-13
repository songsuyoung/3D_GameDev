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
    float jumpHeight = 3f;

    Vector3 dir;
    private float gravity = -9.81f;//중력계수 
    [SerializeField]
    Transform playerCamera;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        playerAnimator = player.GetComponent<Animator>();
        characterController = player.GetComponent<CharacterController>();
        dir = Vector3.zero;
    }

    private void Update()
    {
        Move();
    }

    void Move()
    {   //가로로 A-D
        dir.x = Input.GetAxisRaw("Horizontal");
        //세로로, W-S키
        dir.z = Input.GetAxisRaw("Vertical");
        // 카메라가 바라보는 방향으로 플레이어 방향을 이동시킨다.

        Vector3 forward = player.TransformDirection(Vector3.forward); //플레이어의 현재 z좌표를 월드에서
        Vector3 right = player.TransformDirection(Vector3.right); //플레이어의 현재 x좌표(월드)
        
        Vector3 moveDirection = (forward * dir.z + right * dir.x).normalized;

        if(characterController.isGrounded==false)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }
        // 이동
        characterController.Move(moveDirection * speed * Time.deltaTime);

        float offset = 0.5f + Input.GetAxis("Sprint") * 0.5f; // Shift 누를 시 스피드 추가 예정

        playerAnimator.SetFloat("Horizontal", moveDirection.x * offset);
        playerAnimator.SetFloat("Vertical", moveDirection.z * offset);

        Debug.Log(moveDirection);

        // 회전
        if (moveDirection != Vector3.zero)
        {
            Vector3 toRotation = Vector3.Scale(playerCamera.forward, new Vector3(1f, 0, 1f));
            player.rotation = Quaternion.Slerp(player.rotation, Quaternion.LookRotation(toRotation), speed * Time.deltaTime);
        }

    }

    void Attack()
    {

    }
}
