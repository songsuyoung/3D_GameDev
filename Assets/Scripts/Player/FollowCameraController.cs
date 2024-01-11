using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FollowCameraController : MonoBehaviour
{

    [SerializeField]
    private Transform targetPlayer;
    [SerializeField]
    private Transform playerCamera;

    [SerializeField]
    private float sensitivity = 100f; //마우스 민감도
    [SerializeField]
    private float followSpeed = 10f;
    private float minDistance=2f, maxDistance=5f;
    private float limitedAngle = 70f; //상하이동시 angle 이상 넘어가지 않도록 함. 
    private float finalDistance = 0f;


    private float smoothness = 10f;
    Vector3 dirNormalize; //방향 단위 벡터
    float dirX,dirY;

    Vector3 finalDir;
    private void Start()
    {
        dirX = transform.localRotation.eulerAngles.x;
        dirY = transform.localRotation.eulerAngles.y;
        //초기화 
        dirNormalize = playerCamera.transform.localPosition.normalized;
        finalDistance = playerCamera.transform.localPosition.magnitude; //길이
    }

    private void Update()
    {
        //수직 이동시 x값 변경,마우스 반응 속도를 sensitivity로 정의
        dirX += (-Input.GetAxis("Mouse Y")) * sensitivity * Time.deltaTime;
        dirY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

        //제한된 각도를 넘지않도록, 정규화
        dirX = Mathf.Clamp(dirX, -limitedAngle, limitedAngle);
        //현재 방향을 쿼터니언 체계에서 오일러각 회전
        Quaternion quaternion = Quaternion.Euler(new Vector3(dirX,dirY,0));
        transform.rotation = quaternion;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPlayer.transform.position, followSpeed*Time.deltaTime);

        finalDir = transform.TransformPoint(dirNormalize*maxDistance);

        RaycastHit hit;
        if (Physics.Linecast(transform.position,finalDir,out hit))
        {
            //hit의 위치를 min - max distance보간
            finalDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            finalDistance = maxDistance;
        }

        //선형보간을 통해서 위치좌표 
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, dirNormalize * finalDistance, Time.deltaTime*smoothness); ;
    }

}
