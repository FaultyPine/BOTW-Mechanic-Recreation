using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void OnActivate();
    void OnUpdate();
    void OnEnd();
}


public class StateMachine : MonoBehaviour
{
    IState currState;
    IState prevState;

    void Update() {
        if (currState != null) {
            currState.OnUpdate();
        }
    }

    public void SetState<T>() where T : IState {
        if (currState != null) {
            currState.OnEnd();
        }
        prevState = currState;

        currState = GetComponent<T>();
        currState.OnActivate();
    }

    public IState getCurrState() { return currState; }
    public IState getPrevState() { return prevState; }

}