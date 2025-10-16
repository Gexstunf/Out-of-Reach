using System;
using UnityEngine;

namespace Characters.Animation {
    public abstract class AnimationControllerManagerScript<T> : AnimControllerManagerBaseScript where T : Enum
    {

        public void SetBool(T state, bool value) => SetAnimatorBool(state, value);
        public void Trigger(T state) => SetAnimatorTrigger(state);
        public void SetFloat(T state, float value) => SetAnimatorFloat(state, value);

        // non-generic bridge (calls typed methods by parsing state name to T)
        public override void SetBoolByName(string stateName, bool value)
        {
            var state = (T) Enum.Parse(typeof(T), stateName);
            SetAnimatorBool(state, value);
        }

        public override void TriggerByName(string stateName)
        {
            var state = (T) Enum.Parse(typeof(T), stateName);
            SetAnimatorTrigger(state);
        }

        public override void SetFloatByName(string stateName, float value)
        {
            var state = (T) Enum.Parse(typeof(T), stateName);
            SetAnimatorFloat(state, value);
        }
        
        protected abstract void SetAnimatorBool(T state, bool value);
        protected abstract void SetAnimatorTrigger(T state);
        protected abstract void SetAnimatorFloat(T state, float value);
    }
}
