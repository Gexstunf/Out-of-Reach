using TMPro;
using UnityEngine;

namespace UI.Scripts {
    public class TerminalCursorScript : MonoBehaviour
    {
// Assign these in the Unity Inspector
        public TMP_InputField inputField;
        public RectTransform fakeCursorRect; // Your separate UI Image/RectTransform
        public float blinkRate = 0.5f; // Time in seconds for one half-cycle

        private float _timeSinceLastBlink = 0f;

        void Start()
        {
            // Ensure the fake cursor starts visible (or hidden, depending on preference)
            fakeCursorRect.gameObject.SetActive(true); 
        }

        void Update()
        {
            // 1. Position the Cursor
            PositionCursor();
            // 2. Handle Blinking
            UpdateBlinking();
        }

        //---------------------------------------------------------
        // 1. CURSOR POSITIONING LOGIC
        //---------------------------------------------------------
        private void PositionCursor()
        {
            // Get the current index where the user's cursor is.
            int caretPosition = inputField.caretPosition;
            TMP_Text textComponent = inputField.textComponent;

            // Hide the cursor if the text field is not focused
            if (!inputField.isFocused)
            {
                fakeCursorRect.gameObject.SetActive(false);
                return;
            }

            // --- Handle Empty/Initial State ---
            if (textComponent.textInfo.characterInfo.Length == 0 || string.IsNullOrEmpty(textComponent.text))
            {
                // Text is empty: position at the start point (usually text component's content origin)
                // You may need to manually adjust this offset based on padding and alignment.
                Vector3 startPos = textComponent.rectTransform.rect.min;
                fakeCursorRect.localPosition = new Vector3(startPos.x, startPos.y, 0f);
            
                // Set the cursor height to match the font size if needed (optional)
                // fakeCursorRect.sizeDelta = new Vector2(fakeCursorRect.sizeDelta.x, textComponent.fontSize);
                return;
            }

            // --- Handle Text Content State ---
        
            // Clamp the index to prevent out-of-bounds access. The index is always (caretPosition - 1)
            // because we want the character *before* the caret index to determine position.
            int charIndex = Mathf.Clamp(caretPosition - 1, 0, textComponent.textInfo.characterInfo.Length - 1);
        
            // Retrieve the geometry information for the character we need to reference.
            TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[charIndex];

            Vector3 targetWorldPosition;

            if (caretPosition == 0)
            {
                // Case 1: Before the first character (caret at index 0)
                // Use the bottom-left corner of the first character.
                targetWorldPosition = charInfo.bottomLeft;
            }
            else
            {
                // Case 2: In the middle or at the end of text
                // Use the bottom-right corner of the character *before* the caret.
                targetWorldPosition = charInfo.bottomRight;
            }

            // Convert the World Space coordinate (from charInfo) to Local Space 
            // (for the RectTransform of the fake cursor).
            Vector3 targetLocalPosition = textComponent.rectTransform.InverseTransformPoint(targetWorldPosition);

            // Adjust the Y position of the cursor to be centered vertically with the text 
            // (usually half the height of the character up from the baseline/bottomRight)
            float charHeight = charInfo.ascender - charInfo.descender;
            targetLocalPosition.y += charHeight / 2f;

            // Apply Position
            fakeCursorRect.localPosition = targetLocalPosition;
        }

        //---------------------------------------------------------
        // 2. CURSOR BLINKING LOGIC
        //---------------------------------------------------------
        private void UpdateBlinking()
        {
            // Only blink if the input field is focused
            if (!inputField.isFocused) return;
        
            _timeSinceLastBlink += Time.deltaTime;

            if (_timeSinceLastBlink >= blinkRate)
            {
                // Toggle the visibility state
                fakeCursorRect.gameObject.SetActive(!fakeCursorRect.gameObject.activeSelf);
                _timeSinceLastBlink = 0f;
            }
        }
    }
}
