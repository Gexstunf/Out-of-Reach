using UnityEngine;

namespace Environment.Scripts {
    public abstract class InteractableObjectSO : ScriptableObject
    {
        protected string InteractableObjectName;
        public abstract void Interact(Transform actor);
    }
}
