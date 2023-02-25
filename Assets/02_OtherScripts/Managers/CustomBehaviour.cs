using UnityEngine;

[System.Serializable]
public abstract class CustomBehaviour
{
    public virtual void OnStart() { }
    public virtual void OnAwake() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }
}
