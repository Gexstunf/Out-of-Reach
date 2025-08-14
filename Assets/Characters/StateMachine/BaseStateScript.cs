using System;
using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine
{
    public abstract class BaseStateScript<EState> where EState : Enum
    {
        public BaseStateScript(EState key)
        {
            StateKey = key;
        }
        
        public EState StateKey { get; private set; }
        
        public abstract void EnterState();
        public abstract void ExitState();
        public abstract void UpdateState();
        public abstract EState GetNextState();
        public abstract void OnTriggerEnter(Collider other);
        public abstract void OnTriggerExit(Collider other);
        public abstract void OnTriggerStay(Collider other);
    }
}
