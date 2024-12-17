using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Vector3 previousPosition;
    private Collider panelCollider;

    void Start()
    {
        // 처음 위치 저장
        previousPosition = transform.position;
        panelCollider = GetComponent<Collider>();
    }

    void Update()
    {
        // 현재 위치와 이전 위치를 비교하여 패널이 위로 움직이는지 아래로 움직이는지 판단
        Vector3 currentPosition = transform.position;

        if (currentPosition.y > previousPosition.y)
        {
            // 위로 이동 중이므로 패널 비활성화
            panelCollider.enabled = false;
        }
        else if (currentPosition.y < previousPosition.y)
        {
            // 아래로 이동 중이므로 패널 활성화
            panelCollider.enabled = true;
        }

        // 이전 위치를 현재 위치로 업데이트
        previousPosition = currentPosition;
    }
}
