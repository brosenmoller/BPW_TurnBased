using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : MonoBehaviour
{
    public Dictionary<Type, State<T>> stateDictionary = new();
    private List<Transition> allTransitions = new();
    private List<Transition> activeTransitions = new();
    public State<T> currentState;
    
    public T Controller { get; private set; }

    public StateMachine(T owner, params State<T>[] states)
    {
        Controller = owner;

        foreach (State<T> state in states)
        {
            stateDictionary.Add(state.GetType(), state);
            state.Setup(this);
        }
    }

    public void ChangeState(Type newStateType)
    {
        if (!stateDictionary.ContainsKey(newStateType))
        {
            Debug.LogWarning($"{newStateType.Name} is not a state in the current statemachine ({nameof(T)})");
            return;
        }

        currentState?.OnExit();

        currentState = stateDictionary[newStateType];
        activeTransitions = allTransitions.FindAll(
            currentTransition => currentTransition.fromState == currentState.GetType() || currentTransition.fromState == null
        );
        currentState.OnEnter();
    }

    public void OnFixedUpdate()
    {
        foreach (Transition transition in activeTransitions)
        {
            if (transition.Evalutate())
            {
                ChangeState(transition.toState);
                return;
            }
        }
        currentState.OnUpdate();
    }

    public void AddTransition(Transition transition)
    {
        allTransitions.Add(transition);
    }
}

