using System;
using UnityEngine;

namespace Characters.Utils {
    public class PseudoParentScript : MonoBehaviour {
        [Header("Object to follow & Unparent")] 
        [SerializeField] private GameObject pseudoChild;

        private void Awake() {
            pseudoChild.transform.SetParent(null);
        }

        private void Update() {
            transform.position = pseudoChild.transform.position;
            transform.rotation = pseudoChild.transform.rotation;
        }
    }
}
