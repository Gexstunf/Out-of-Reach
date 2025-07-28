using System;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine
{
    public abstract class StateManagerScript<EState> : MonoBehaviour where EState : Enum
    {
        protected Dictionary<EState, BaseStateScript<EState>> States = new Dictionary<EState, BaseStateScript<EState>>();
        protected BaseStateScript<EState> CurrentState;

    
        private bool _isTransitioningToState = false;
    
        private void Start()
        {
            CurrentState.EnterState();
        }

        private void Update()
        {
            EState nexStateKey = CurrentState.GetNextState();
            if (!_isTransitioningToState && nexStateKey.Equals(CurrentState.StateKey))
            {
                CurrentState.UpdateState();
            }
            else
            {
                _isTransitioningToState = true;
                TransitionToState(CurrentState.StateKey);
            }
        }

        public void TransitionToState(EState stateKey)
        {
            CurrentState.ExitState();
            CurrentState = States[stateKey];
            CurrentState.EnterState();
        }
        private void OnTriggerEnter(Collider other)
        {
            CurrentState.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            CurrentState.OnTriggerExit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            CurrentState.OnTriggerStay(other);
        }
    }
}

