using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnesis : MonoBehaviour, IState
{

    PlayerController playerController;
    public GameObject magnesisNoodlesHolder;
    public LayerMask layerMask;

    RaycastHit hit;

    void Awake() {
        playerController = GetComponent<PlayerController>();
    }

    public void OnActivate() {
        Debug.Log("Magnesis activate");
        //magnesisNoodlesHolder.SetActive(true);
        //magnesisNoodlesHolder.transform.position = transform.position;
        Vector3 playerForward = playerController.MainCamera.transform.forward;
        DrawArrow.ForDebugDuration(transform.position, playerForward, Color.red, 1.0f);
        if (Physics.Raycast(transform.position, playerForward, out hit, Mathf.Infinity, layerMask)) {
            Debug.Log(hit.collider.gameObject.name);
        }
        CheckStateEnd();
    }

    public void OnUpdate() {


        
        CheckStateEnd();
    }

    void CheckStateEnd() {
        if (!Input.GetMouseButton(1)) {
            playerController.SM.SetState<Movement>();
        }
    }

    public void OnEnd() {
        Debug.Log("Magnesis end");
        //magnesisNoodlesHolder.SetActive(false);
    }
}
