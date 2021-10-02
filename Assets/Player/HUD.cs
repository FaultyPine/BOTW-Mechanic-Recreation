using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

    public Image AbilityHUDIcon;

    public Sprite StasisHUDIcon;
    public Sprite MagnesisHUDIcon;

    void Awake() {
        
    }

    // not ideal impl of something like this, but I don't plan on having more than a few abilities here so doing it like this is easiest
    public void UpdateHUDIcon(int abilityState) {
        switch (abilityState) {
            case 0:
                AbilityHUDIcon.sprite = StasisHUDIcon;
                break;
            case 1:
                AbilityHUDIcon.sprite = MagnesisHUDIcon;
                break;
            default:
                break;
        }
    }
}
