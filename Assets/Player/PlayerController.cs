using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    public StateMachine SM { get; private set; }
    public Camera MainCamera { get; private set; }
    public Animator animator { get; private set; }
    HUD playerAbilityHUD;

    /*
        0 -> stasis
        1 -> magnesis
    */ 
    int numAbilities = 2;

    int abilityState = 0;

    void Awake() {
        playerAbilityHUD = GetComponent<HUD>();
        SM = GetComponent<StateMachine>();
        MainCamera = Camera.main;
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        SM.SetState<Movement>();
    }

    void Update()
    {
        switch (abilityState) {
            case 0: // stasis

                if (Input.GetMouseButtonDown(1)) { // right click
                    SM.SetState<Stasis>();
                }
                else if (Input.GetMouseButtonDown(0)) { // left click
                    SM.SetState<Attack>();
                }

                break;

            case 1: // magnesis

                if (Input.GetMouseButtonDown(1)) {
                    SM.SetState<Magnesis>();
                }

                break;

            default:
                Debug.Log("Invalid abilityState");
                break;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            abilityState = (abilityState + 1) % numAbilities;
            Debug.Log("Changed ability. New abilityState = " + abilityState);
            playerAbilityHUD.UpdateHUDIcon(abilityState);
        }
       
    }

    

}
