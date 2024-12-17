using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float walkSpeed = 10f; // 걷기 속도
    public float dashSpeed = 40f; // 대쉬 속도
    public float dashDuration = 0.1f; // 대쉬 지속 시간
    public float rotationSpeed = 10f; // 회전 속도
    private Animator animator; // 애니메이터 컴포넌트
    private Rigidbody rb; // Rigidbody 컴포넌트
    private Vector3 inputVector; // 입력 벡터
    private bool isDashing; // 대쉬 여부
    private float dashTime; // 대쉬 시작 시간

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 입력받은 방향을 벡터로 저장
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.z = Input.GetAxisRaw("Vertical");
        // 이동 벡터 정규화
        inputVector = inputVector.normalized; // 크기 정규화

        // 대쉬 입력 체크
        if (Input.GetKeyDown(KeyCode.LeftShift) && inputVector.magnitude > 0.1f && !isDashing)
        {
            StartDash();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // 대쉬 중일 때
            if (Time.time < dashTime + dashDuration)
            {
                rb.MovePosition(transform.position + inputVector * dashSpeed * Time.fixedDeltaTime);
            }
            else
            {
                isDashing = false; // 대쉬 종료
            }
        }
        else
        {
            // 이동 벡터의 크기가 0보다 클 때
            if (inputVector.magnitude > 0.1f)
            {
                // 이동 방향으로 플레이어 회전
                Quaternion toRotation = Quaternion.LookRotation(inputVector, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }

            // Rigidbody를 이용한 이동
            rb.MovePosition(transform.position + inputVector * walkSpeed * Time.fixedDeltaTime);
        }
    }

    private void LateUpdate()
    {
        animator.SetFloat("speed", inputVector.magnitude);
        animator.SetBool("isDash", isDashing);
    }

    private void StartDash()
    {
        isDashing = true;
        dashTime = Time.time; // 대쉬 시작 시간 기록
    }
}
