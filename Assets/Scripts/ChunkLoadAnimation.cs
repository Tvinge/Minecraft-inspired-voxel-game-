using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoadAnimation : MonoBehaviour
{
    float speed = 3f;
    Vector3 targetPos;

    float waitTimer;
    float timer;

    void Start()
    {
        waitTimer = Random.Range(0f, 3f);
        targetPos = transform.position;
        transform.position = new Vector3(transform.position.x, -VoxelData.ChunkH, transform.position.z);
    }

    void Update()
    {
        if (timer < waitTimer)//randomizes chunks poping up
        {
            timer += Time.deltaTime;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
            if ((targetPos.y - transform.position.y) < 0.05f)
            {
                transform.position = targetPos;
                Destroy(this);
            }
        }
    }
}
