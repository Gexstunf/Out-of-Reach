using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Scripts {
    public class WorldUIManagerScript : MonoBehaviour {
        public static WorldUIManagerScript Instance;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject _priceCanvasPrefab;
        [SerializeField] private GameObject _interactionCanvasPrefab;
        
        [Header("UI Prefab")]
        [SerializeField] private TextMeshProUGUI _interactableText;
        [SerializeField] private TextMeshProUGUI _priceText;
        
        [Header("Settings")] 
        public Vector3 offset = new Vector3(0, 1.5f, 0);
        
        private GameObject _priceUIInstance;
        private Transform _priceTarget;

        private GameObject _interactionUIInstance;
        private Transform _interactionTarget;

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start() {
            // instantiate, hide
            _priceUIInstance = Instantiate(_priceCanvasPrefab, Vector3.zero, Quaternion.identity);
            _interactionUIInstance = Instantiate(_interactionCanvasPrefab, Vector3.zero, Quaternion.identity);

            _priceText = _priceUIInstance.GetComponentInChildren<TextMeshProUGUI>();
            _interactableText = _interactionUIInstance.GetComponentInChildren<TextMeshProUGUI>();
            
            _interactionUIInstance.SetActive(false);
            _priceUIInstance.SetActive(false);
        }

        private void LateUpdate() {
            if (_priceTarget)
                _priceUIInstance.transform.position = _priceTarget.position + offset;

            if (_interactionTarget)
                _interactionUIInstance.transform.position = _interactionTarget.position + offset;
        }

        #region Public API

        public void ShowPrice(Transform targetAnchor, float value) {
            _priceTarget = targetAnchor;
            _priceText.text = ("$" + value);
            _priceUIInstance.SetActive(true);
        }

        public void HidePrice() {
            _priceTarget = null;
            _priceUIInstance.SetActive(false);
        }

        public void ShowInteractable(Transform target, string text)
        {
            _interactionTarget = target;
            _interactableText.text = text;
            _interactionUIInstance.SetActive(true);
        }

        public void HideInteractable()
        {
            _interactionTarget = null;
            _interactionUIInstance.SetActive(false);
        }

        #endregion
    }
}