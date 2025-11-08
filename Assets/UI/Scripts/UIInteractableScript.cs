using UnityEngine;

namespace UI.Scripts {
    public class UIInteractableScript : MonoBehaviour
    {
        [Header("Settings")]
        public bool interactable;
        [SerializeField] private string interactText;
        public Transform anchorTransform;

        public string DisplayText => interactText;
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (anchorTransform == null) {
                anchorTransform = transform;
            }
        }

        public void SetText(string text) {
            interactText = text;
        }
    }
}
