using TMPro;
using UnityEngine;

namespace UI.Scripts {
    public class WorldUIManagerScript : MonoBehaviour {
        public static WorldUIManagerScript Instance;

        [Header("UI Prefab")]
        public GameObject priceUIPrefab;
        private GameObject _currentUIInstance;
        private TextMeshProUGUI _priceText;
        private Transform _currentTarget;

        [Header("Settings")]
        public Vector3 offset = new Vector3(0, 1.5f, 0); 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // instantiate, hide
            _currentUIInstance = Instantiate(priceUIPrefab, Vector3.zero, Quaternion.identity);
            _priceText = _currentUIInstance.GetComponentInChildren<TextMeshProUGUI>();
            _currentUIInstance.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_currentTarget)
            {
                _currentUIInstance.transform.position = _currentTarget.position + offset;
            }
        }

        public void ShowPrice(Transform targetAnchor, float value)
        {
            _currentTarget = targetAnchor;
            _priceText.text = ("$" + value);
            _currentUIInstance.SetActive(true);
        }

        public void HidePrice()
        {
            _currentTarget = null;
            _currentUIInstance.SetActive(false);
        }
    }
}