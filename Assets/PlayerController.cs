using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    GameObject player;
    Animator playerAnimator;
    Rigidbody playerRigidbody;
    [SerializeField]
    float speed = 10f;
    [SerializeField]
    float jumpHeight = 3f;

    Vector3 dir= Vector3.zero;
   
    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerAnimator = player.GetComponent<Animator>();
        playerRigidbody= player.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Move();
    }

    void Move()
    {
        //가로로 A-D
        dir.x = Input.GetAxis("Horizontal");
        //세로로, W-S키
        dir.z = Input.GetAxis("Vertical");
        dir.Normalize(); //단위벡터로 만들어서, 대각선 이동 시 속도가 증가하는 현상을 없애기 위함.

        //현재 위치 + 방향 * 거리(속도 * 시간)
        player.transform.position += dir * speed * Time.deltaTime;
        player.transform.LookAt(player.transform.position + dir); //회전

        float offset = 0.5f + Input.GetAxis("Sprint") * 0.5f; //Shift 누를 시 스피드 추가 예정

        playerAnimator.SetFloat("Horizontal", dir.x * offset);
        playerAnimator.SetFloat("Vertical", dir.z * offset);

        //Jump 추가예정
    }

    void Attack()
    {

    }
}
