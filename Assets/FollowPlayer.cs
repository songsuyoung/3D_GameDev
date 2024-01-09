using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    [SerializeField]
    GameObject player;

    [SerializeField]
    float offset;
    const float speed = 5f;
    private void FixedUpdate()
    {
        Vector3 targetPos = new Vector3(
            player.transform.position.x,
            player.transform.position.y + offset,
            player.transform.position.z - offset
        );

        this.transform.position = Vector3.Lerp(this.transform.position,targetPos, speed * Time.deltaTime); ;
    }
}
