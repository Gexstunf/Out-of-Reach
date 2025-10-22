using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Scripts {
    public class TerminalControllerScript : MonoBehaviour
    {
        #region Variables
        
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
        public bool keyBeep = false;
        public AudioClip typingSoundClip;
        public AudioSource audioSource;
        public bool goSlower = false;
        public StyleText[] Styles;
        public List<CommandSO> allCommandSOs;
        [SerializeField] private bool _hasToSpecialCommand;
        
        public List<string> CommandHistory { get; private set; } = new List<string>();
        private int _historyIndex = -1;
        
        private Dictionary<string, CommandSO> _availableCommands = new Dictionary<string, CommandSO>();
        [SerializeField] private Dictionary<string, CommandSO> _specialCommands = new Dictionary<string, CommandSO>();
        
        private List<IEnumerator> _typingCoroutines = new List<IEnumerator>();
        private bool _isTyping = false;
        
        private StyleText _defaultStyle;
        
        #endregion
        
        [System.Serializable]
        public class StyleText {
            public float TypeSpeed { get; private set; } = 0.1f;
            public bool Bold = false;
            public bool Italic = false;
            public bool Underline = false;
            public EStyle typeStyle = EStyle.Normal;
            
            protected float SlowTypeSpeed = 0.2f;
            protected float NormalTypeSpeed = 0.05f;
            protected float FastTypeSpeed = 0.01f;

            public enum EStyle {
                Normal,
                Fast,
                Slow,
            }

            public string ApplyStyle(string text) {
                
                string result = text;
                
                if (Bold) result = $"<b>{result}</b>";
                if (Italic) result = $"<i>{result}</i>";

                switch (typeStyle) {
                    case EStyle.Normal:
                        TypeSpeed = NormalTypeSpeed;
                        break;
                    case EStyle.Fast:
                        TypeSpeed = FastTypeSpeed;
                        break;
                    case EStyle.Slow:
                        TypeSpeed = SlowTypeSpeed;
                        break;
                }
                    
                return result;
            }
        }

        #region Starting logic
        private void Start()
        {
            UnityEngine.Debug.Log("Starting TerminalControllerScript");

            if (allCommandSOs != null) {
                foreach (var cmd in allCommandSOs) {
                    _availableCommands.Add(cmd.commandName, cmd);
                    UnityEngine.Debug.Log(_availableCommands[cmd.commandName].commandName);
                }
            }
            
            audioSource = GetComponent<AudioSource>();
            _defaultStyle = GetStyleText(StyleText.EStyle.Normal);
            inputField.text = "";
            
            if (keyBeep && audioSource) audioSource.PlayOneShot(typingSoundClip);
        }

        #endregion
        
        private void Update()
        {
            // Always ensure the input field is focused
            if (!inputField.isFocused)
                inputField.ActivateInputField();

            ProcessTypingList();
            HandleInput();
            UpdateCursorBlink();
        }
        
        
        #region Main logic
        
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
            // some formatting
            input = RemoveCursor(input);
            input = input.ToLower();
            
            // here we get some helpful vars (deconstruct input)
            string[] parts = input.Split(' ');
            string cmd = parts[0];
            string autoCompletedCmd = AutoComplete(true, _hasToSpecialCommand, cmd);

            string[] args = parts.Skip(1).ToArray();
            string argString = string.Join(" ", args);
            string processedInput = $"{autoCompletedCmd} {argString}";
            bool success = false;
            
            CommandHistory.Add(processedInput);
            _historyIndex = CommandHistory.Count;

            AppendOutput($"\n{prefixText}{dir}: {autoCompletedCmd} {argString}", false, true, GetStyleText(StyleText.EStyle.Fast));


            if (_hasToSpecialCommand) {
                success = ExecuteSpecialCommand(cmd, args); 
                UnityEngine.Debug.Log(success ? "Successfully executed special command." : "Failed to execute special command.");
            }
            else {
                success = ExecuteNormalCommand(cmd, args);  
            }


            if (!success) {
                UnityEngine.Debug.Log($"input string: {input}");
                string state = _hasToSpecialCommand ? "Invalid" : "Unknown";
                AppendOutput($"{state} command: {autoCompletedCmd}", false);
            }
        }

        private void ProcessTypingList() {
            if (_typingCoroutines.Count > 0 && !_isTyping) {
                StartCoroutine(_typingCoroutines[0]);
            }
        }
        
        private string AutoComplete(bool onlyWantValue = false, bool specialCommands = false, string str = null) {
            string current = str ?? inputField.text.ToLower();
            var matches = _availableCommands.Keys.Where(k => k.StartsWith(current)).ToList();

            if (specialCommands) {
                matches = _specialCommands.Keys.Where(k => k.StartsWith(current)).ToList();
            }

            if (matches.Count >= 1)
            {
                inputField.text = matches[0];

                if (onlyWantValue) {
                    return matches[0];
                }
                
                inputField.caretPosition = inputField.text.Length; // move caret to end
            }
            return "";
        }
        
        private IEnumerator TypeText(string text, StyleText style) {
            float speed = 0.1f;
            string styledText = text; 
            _isTyping = true;

            if (style != null) {
                styledText = style.ApplyStyle(text);
                speed = style.TypeSpeed;
            }
            else {
                UnityEngine.Debug.Log("Couldnt apply style, due to style = null.");
            }
            
            speed = goSlower? speed + 1f : speed;
            
            foreach (char c in styledText)
            {
                outputText.text += c;
                if (keyBeep && c != " "[0]) audioSource.PlayOneShot(typingSoundClip);
                yield return new WaitForSeconds(speed);
            }
            
            _isTyping = false;
            outputText.text += "\n ";
            _typingCoroutines.RemoveAt(0);
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
                UnityEngine.Debug.Log("Executing normal command...");
                return true;
            }
            UnityEngine.Debug.Log("Could not find normal command...");
            return false;
        }

        private bool ExecuteSpecialCommand(string cmd, string[] args) {

            var processedCmd = AutoComplete(true, true, cmd);
            UnityEngine.Debug.Log("Autocompleted CMD: " + processedCmd + "  from: " + cmd);
            if (_specialCommands.ContainsKey(cmd) || _specialCommands.ContainsKey(processedCmd)) {
                _specialCommands[processedCmd].Execute(args, this);
                UnityEngine.Debug.Log("Executing special command...");
                CleanUpSpecialCommands();
                return true;
            }
            UnityEngine.Debug.Log("Could not find special command...");
            return false;
        }
        
        #endregion
        
        public void AppendOutput(string text, bool indent = true, bool type = true, StyleText style = null)
        {
            
            if (indent) {
                text = text.Insert(0, "   ");
            }
            
            if (type) {
                var txtStyle = style ?? _defaultStyle;
                AddTextToTypingList(text, txtStyle);
            }
            else {
                outputText.text += text + "\n ";
            }
        }

        #region Helpers

        private void AddTextToTypingList(string text, StyleText style) {
            var newCoroutine = TypeText(text, style);
            _typingCoroutines.Add(newCoroutine);
        }

        public void AddSpecialCommand(string commandName, CommandSO command) {
            _specialCommands.Add(commandName, command);
        }
        
        public void SetRequiredCommandState(bool shouldDoSpecialCommand) {
            _hasToSpecialCommand = shouldDoSpecialCommand;
        }
        
        private void CleanUpSpecialCommands() {
            foreach (var specialCmd in _specialCommands) {
                Debug.Log("Clearing: " + specialCmd.Key + ": " + specialCmd.Value);
                CommandSO cmd = specialCmd.Value;
                cmd.commandItemsPrefabs.Clear(); // we clear the list of items for the special command
            }
            _specialCommands.Clear(); // we clear the list of special commands
            _hasToSpecialCommand = false;
        }

        public void ClearOutput() {
            outputText.text = "";
        }
        
        private string RemoveCursor(string input) {
            if (input.Contains(cursorChar)) {
                return input.Replace(cursorChar, ""); // only remove visual cursor
            }
            
            return input;
        }
        
        public List<CommandSO> AllAvailableCommands() {
            return allCommandSOs;
        }

        public StyleText GetStyleText(StyleText.EStyle wantedStyle) {
            StyleText defaultStyle = new StyleText();;
            
            // if it finds a style matching, use it, else, use default
            if (Styles != null && Styles.Length > 0)
            {
                foreach (var style in Styles)
                {
                    if (style.typeStyle == wantedStyle)
                    {
                        return style;
                    }
                }
            }
            UnityEngine.Debug.Log("Returning default text style...");
            return defaultStyle;
        }
        #endregion
    }
}
