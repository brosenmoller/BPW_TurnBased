using UnityEngine;

public abstract class State<T> where T : MonoBehaviour
{
    protected StateMachine<T> stateOwner;

    public void Setup(StateMachine<T> stateMachine)
    {
        stateOwner = stateMachine;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}
