using UnityEngine;

namespace Characters.Animation {
    public abstract class AnimControllerManagerBaseScript : MonoBehaviour
    {
        public abstract void SetBoolByName(string stateName, bool value);
        public abstract void TriggerByName(string stateName);
        public abstract void SetFloatByName(string stateName, float value);
    }
}
