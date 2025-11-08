using System.Collections;
using Characters.PlayerController.Scripts;
using UnityEngine;

namespace Environment.Scripts.Interactable {
    public abstract class InteractionObjectScript : MonoBehaviour
    {
        public abstract void StartInteraction(InteractableControllerScript controller);
        public abstract IEnumerator QuitInteraction();
    }
}
