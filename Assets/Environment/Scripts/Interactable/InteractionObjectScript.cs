using System.Collections;
using UnityEngine;

namespace Environment.Scripts.Interactable {
    public abstract class InteractionObjectScript : MonoBehaviour
    {
        public abstract void StartInteraction();
        public abstract IEnumerator QuitInteraction();
    }
}
