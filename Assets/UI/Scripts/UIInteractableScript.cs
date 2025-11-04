using UnityEngine;

namespace UI.Scripts {
    public class UIInteractableScript : MonoBehaviour
    {
        [Header("Settings")]
        public bool interactable;
        public string interactText;
        public Transform anchorTransform;
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (anchorTransform == null) {
                anchorTransform = transform;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
