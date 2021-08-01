using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour, IState
{

    [Header("Player Stats")]
    public float speed;
    public float turnSmoothTime;

    [Header("Fill-ins")]
    public Transform cam;


    Animator animator;
    float turnSmoothVelocity;

    void Start() {
        animator = GetComponent<Animator>();
    }

    public void OnActivate()
    {
        
    }

    public void OnUpdate() {
        moveChar();
    }

    Vector3 moveSpeed = Vector3.zero;
    void moveChar()
    {
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if (inputDir.magnitude >= 0.1f) {

            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            moveSpeed = moveDir * speed * Time.deltaTime;
            transform.position += moveSpeed;
            
            
        }
        else {
            moveSpeed = Vector3.zero;
        }

        if (animator != null) {
            float currSpeed = Mathf.Abs(moveSpeed.x) + Mathf.Abs(moveSpeed.z);
            animator.SetFloat("Speed", currSpeed);
        }
        
    }


    public void OnEnd() {

    }
}
