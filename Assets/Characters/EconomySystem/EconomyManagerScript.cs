using System;
using Items.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.EconomySystem {
    public class EconomyManagerScript : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private GlobalBankSO globalBankSo;
        private static EconomyManagerScript Instance { get; set; }
        

        private void Awake() {
            if (Instance != null && Instance != this) {
                Debug.LogWarning("Duplicate EconomyManager detected, destroying the new one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // DontDestroyOnLoad(gameObject);
        }
        
        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Item")) {
                ItemGrabbableScript item = other.GetComponent<ItemGrabbableScript>();
                if (item != null) {
                    Debug.Log("Selling item: " + item.name);
                    
                    Debug.Log("Value item: " + item.data.value);

                    globalBankSo.Add(item.data.value);
                    Destroy(item.gameObject);
                }
            }
        }
    }
}
