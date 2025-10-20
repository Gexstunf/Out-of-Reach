using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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
        public string userName = "subject_5";
        public string dir = "Z:/project_61/exp_4";
        
        public List<CommandSO> allCommandSOs;

        [SerializeField] private bool _hasToSpecialCommand;
        
        public List<string> CommandHistory { get; private set; } = new List<string>();
        private int _historyIndex = -1;
        
        private Dictionary<string, CommandSO> _availableCommands = new Dictionary<string, CommandSO>();
        [SerializeField] private Dictionary<string, CommandSO> _specialCommands = new Dictionary<string, CommandSO>();


        private void Start()
        {
            Debug.Log("Starting TerminalControllerScript");

            if (allCommandSOs != null) {
                foreach (var cmd in allCommandSOs) {
                    _availableCommands.Add(cmd.commandName, cmd);
                    Debug.Log(_availableCommands[cmd.commandName].commandName);
                }
            }
            
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

            CommandHistory.Add(input);
            _historyIndex = CommandHistory.Count;

            AppendOutput($"\n{prefixText}{dir}: {input}", false);
            
            input = input.ToLower();
            string[] parts = input.Split(' ');
            
            string cmd = parts[0];
            string[] args = parts.Skip(1).ToArray();

            bool success;

            if (_hasToSpecialCommand) {
                success = ExecuteSpecialCommand(cmd, args); 
                Debug.Log(success ? "Successfully executed special command." : "Failed to execute special command.");
            }
            else {
                success = ExecuteNormalCommand(cmd, args);  
            }
            

            if (!success) {
                Debug.Log($"input string: {input}");
                string state = _hasToSpecialCommand ? "Invalid" : "Unknown";
                AppendOutput($"{state} command: {cmd}", false);
                return;
            }
        }

        private string RemoveCursor(string input) {
            if (input.Contains(cursorChar)) {
                return input.Replace(cursorChar, ""); // only remove visual cursor
            }
            
            return input;
        }

        private void AutoComplete()
        {
            string current = inputField.text.ToLower();
            var matches = _availableCommands.Keys.Where(k => k.StartsWith(current)).ToList();

            if (matches.Count >= 1)
            {
                inputField.text = matches[0];
                inputField.caretPosition = inputField.text.Length; // move caret to end
            }
        }

        private void NavigateHistory(int direction)
        {
            _historyIndex = Mathf.Clamp(_historyIndex + direction, 0, CommandHistory.Count - 1);
            if (CommandHistory.Count > 0)
            {
                inputField.text = CommandHistory[_historyIndex];
                inputField.caretPosition = inputField.text.Length;
            }
        }

        public void CloseTerminal() { }

        private bool ExecuteNormalCommand(string cmd, string[] args ) {
            if (_availableCommands.ContainsKey(cmd)) {
                _availableCommands[cmd].Execute(args, this);
                Debug.Log("Executing normal command...");
                return true;
            }
            Debug.Log("Could not find normal command...");
            return false;
        }

        private bool ExecuteSpecialCommand(string cmd, string[] args) {

            if (_specialCommands.ContainsKey(cmd)) {
                _specialCommands[cmd].Execute(args, this);
                Debug.Log("Executing special command...");
                CleanUpSpecialCommands();
                return true;
            }
            Debug.Log("Could not find special command...");
            return false;
        }

        private void CleanUpSpecialCommands() {
            foreach (var specialCmd in _specialCommands) {
                specialCmd.Value.commandItemsPrefabs = null;
            }
            _specialCommands.Clear();
            _hasToSpecialCommand = false;
        }

        public void AppendOutput(string text, bool indent = true)
        {
            if (indent) {
                outputText.text += "\n   " + text;
            }
            else {
                outputText.text += "\n" + text;
            }
        }

        public void AddSpecialCommand(string commandName, CommandSO command) {
            _specialCommands.Add(commandName, command);
        }

        public void ClearOutput() {
            outputText.text = "";
        }

        public void SetRequiredCommandState(bool shouldDoSpecialCommand) {
            _hasToSpecialCommand = shouldDoSpecialCommand;
        }
        
        public List<CommandSO> AllAvailableCommands() {
            return allCommandSOs;
        }
    }
}
