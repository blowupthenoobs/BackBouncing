using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float verticleOffset;

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = player.transform.position;
        newPos.z = -10;
        newPos.y += verticleOffset;
        transform.position = newPos;
    }
}
