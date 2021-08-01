using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    public StateMachine SM { get; private set; }

    void Awake() {
        SM = GetComponent<StateMachine>();
    }
    void Start()
    {
        SM.SetState<Movement>();
    }

    void Update()
    {
       if (Input.GetKeyDown(KeyCode.Space)) {
            SM.SetState<StasisDetection>();
        }
    }

    

}
