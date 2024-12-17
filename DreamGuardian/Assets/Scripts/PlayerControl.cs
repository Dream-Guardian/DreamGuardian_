using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float walkSpeed = 10f; // �ȱ� �ӵ�
    public float dashSpeed = 40f; // �뽬 �ӵ�
    public float dashDuration = 0.1f; // �뽬 ���� �ð�
    public float rotationSpeed = 10f; // ȸ�� �ӵ�
    private Animator animator; // �ִϸ����� ������Ʈ
    private Rigidbody rb; // Rigidbody ������Ʈ
    private Vector3 inputVector; // �Է� ����
    private bool isDashing; // �뽬 ����
    private float dashTime; // �뽬 ���� �ð�

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // �Է¹��� ������ ���ͷ� ����
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.z = Input.GetAxisRaw("Vertical");
        // �̵� ���� ����ȭ
        inputVector = inputVector.normalized; // ũ�� ����ȭ

        // �뽬 �Է� üũ
        if (Input.GetKeyDown(KeyCode.LeftShift) && inputVector.magnitude > 0.1f && !isDashing)
        {
            StartDash();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // �뽬 ���� ��
            if (Time.time < dashTime + dashDuration)
            {
                rb.MovePosition(transform.position + inputVector * dashSpeed * Time.fixedDeltaTime);
            }
            else
            {
                isDashing = false; // �뽬 ����
            }
        }
        else
        {
            // �̵� ������ ũ�Ⱑ 0���� Ŭ ��
            if (inputVector.magnitude > 0.1f)
            {
                // �̵� �������� �÷��̾� ȸ��
                Quaternion toRotation = Quaternion.LookRotation(inputVector, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }

            // Rigidbody�� �̿��� �̵�
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
        dashTime = Time.time; // �뽬 ���� �ð� ���
    }
}
