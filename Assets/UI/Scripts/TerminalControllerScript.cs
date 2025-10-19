using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UI.Scripts {
    public class TerminalControllerScript : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI outputText;
        public TMP_InputField inputField;
        public string prefixText; 
        
        [Header("Cursor Settings")]
        public string cursorChar = "|";          // visual cursor        
        public float cursorBlinkRate = 0.5f;     // seconds per blink

        private bool _cursorVisible = true;
        private float _cursorTimer = 0f;

        
        [Header("Settings")] 
        public string userName = "unknown_user";
        public string dir = "/???/.";

        private List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;
        private Dictionary<string, System.Action<string[]>> _commands = new Dictionary<string, System.Action<string[]>>();

        private void Start()
        {
            Debug.Log("Starting TerminalControllerScript");

            _commands.Add("help", Cmd_Help);
            _commands.Add("clear", Cmd_Clear);
            _commands.Add("list", Cmd_List);
            _commands.Add("exit", Cmd_Exit);
            _commands.Add("buy", Cmd_Buy);
            
            inputField.text = "";
        }

        private void Update()
        {
            // Always ensure the input field is focused
            if (!inputField.isFocused)
                inputField.ActivateInputField();

            HandleInput();
            UpdateCursorBlink();
        }
        
        private void UpdateCursorBlink()
        {
            _cursorTimer += Time.deltaTime;
            if (_cursorTimer >= cursorBlinkRate)
            {
                _cursorTimer = 0f;
                _cursorVisible = !_cursorVisible;
                RefreshInputFieldCursor();
            }
        }
        
        private void RefreshInputFieldCursor()
        {
            // Store the current caret position (real user caret)
            int caretPos = inputField.caretPosition;

            string text = inputField.text;

            // Remove previous cursor if present
            text = RemoveCursor(text);


            // Insert visual cursor at the caret position
            if (_cursorVisible)
                text = text.Insert(caretPos, cursorChar);

            // Update displayed text
            inputField.text = text;

            // Restore caret to its original position (after the visual cursor)
            inputField.caretPosition = caretPos;
        }
        

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                string input = inputField.text;
                ProcessCommand(input);
                inputField.text = "";
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                AutoComplete();
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
            }
        }

        private void ProcessCommand(string input)
        {
            
            input = RemoveCursor(input);

            _commandHistory.Add(input);
            _historyIndex = _commandHistory.Count;

            AppendOutput($"{prefixText}{userName}{dir}: {input}", false);

            string[] parts = input.Split(' ');
            string cmd = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            if (_commands.ContainsKey(cmd))
                _commands[cmd].Invoke(args);
            else
                AppendOutput($"Unknown command: {cmd}", false);
        }


        private string RemoveCursor(string input) {
            if (input.Contains(cursorChar)) {
                return input = input.Replace(cursorChar, ""); // only remove visual cursor
            }
            
            return input;
        }

        private void AutoComplete()
        {
            string current = inputField.text.ToLower();
            var matches = _commands.Keys.Where(k => k.StartsWith(current)).ToList();

            if (matches.Count >= 1)
            {
                inputField.text = matches[0];
                inputField.caretPosition = inputField.text.Length; // move caret to end
            }
        }

        private void NavigateHistory(int direction)
        {
            _historyIndex = Mathf.Clamp(_historyIndex + direction, 0, _commandHistory.Count - 1);
            if (_commandHistory.Count > 0)
            {
                inputField.text = _commandHistory[_historyIndex];
                inputField.caretPosition = inputField.text.Length;
            }
        }

        private void AppendOutput(string text, bool indent = true)
        {
            if (indent) {
                outputText.text += "\n   " + text;
            }
            else {
                outputText.text += "\n" + text;
            }
        }

        private void Cmd_Help(string[] args) => AppendOutput("Available: " + string.Join(", ", _commands.Keys));
        private void Cmd_Clear(string[] args) => outputText.text = "";
        private void Cmd_List(string[] args) => AppendOutput("Items: pistol($100), ammo($20), medkit($50)");
        private void Cmd_Exit(string[] args) => AppendOutput("Closing terminal...");
        private void Cmd_Buy(string[] args) => AppendOutput(args.Length == 0 ? "Usage: buy <item>" : $"Attempting purchase: {args[0]}");
    }
}
