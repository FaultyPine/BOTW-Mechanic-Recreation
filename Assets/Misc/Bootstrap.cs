using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDestroy() {
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {

    }
}
