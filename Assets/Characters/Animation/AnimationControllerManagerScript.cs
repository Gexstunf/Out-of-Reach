using System;
using UnityEngine;

namespace Characters.Animation {
    public abstract class AnimationControllerManagerScript<T> : MonoBehaviour where T : Enum
    {

        public void SetBool(T state, bool value)
        { 
            SetAnimatorBool(state, value);
        }

        public void Trigger(T state)
        {
            SetAnimatorTrigger(state);
        }

        public void SetFloat(T state, float value)
        {
            SetAnimatorFloat(state, value);
        }
        
        protected abstract void SetAnimatorBool(T state, bool value);
        protected abstract void SetAnimatorTrigger(T state);
        protected abstract void SetAnimatorFloat(T state, float value);
    }
}
