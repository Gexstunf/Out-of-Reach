using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalUtils;
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
        public AudioClip endOfLineSoundClip;
        public AudioSource audioSource;
        public bool goSlower = false;
        public StyleText[] Styles;
        public List<CommandSO> allCommandSOs;
        [SerializeField] private bool _hasToSpecialCommand;
        
        public List<string> CommandHistory { get; private set; } = new List<string>();
        private int _historyIndex = -1;
        
        private Dictionary<string, CommandSO> _availableCommands = new Dictionary<string, CommandSO>();
        [SerializeField] private Dictionary<string, CommandSO> _specialCommands = new Dictionary<string, CommandSO>();
        
        private List<IEnumerator> _queueCoroutines = new List<IEnumerator>();
        private bool _isBusy = false;
        private bool _isOpen = false;

        
        private StyleText _defaultStyle;
        private StyleText _fastStyle;
        
        private LoggerSO _logger;
        
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

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
            _defaultStyle = GetStyleText(StyleText.EStyle.Normal);
            _fastStyle = GetStyleText(StyleText.EStyle.Fast);
        }
        
        private void Start() {
            _logger = LoggerSO.Instance;

            if (allCommandSOs != null) {
                foreach (var cmd in allCommandSOs) {
                    _availableCommands.Add(cmd.commandName, cmd);
                    //UnityEngine.Debug.Log(_availableCommands[cmd.commandName].commandName);
                }
            }

            var fastTextStyle = GetStyleText(StyleText.EStyle.Fast);
            ClearOutput(); // we clear at the start

            AppendOutput("Booting terminal . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . OK", false, playEndSound:true);
            AppendOutput("  ");
            AppendOutput("System Status . . . . . . OK", playEndSound:true);
            AppendOutput("Connection Status . . . . OK", playEndSound:true);
            AppendOutput("Secure Status . . . . OK", playEndSound:true);
            AppendOutput("  ");
            AppendOutput("  ");
            AppendOutput("Finished.", style: fastTextStyle);
            AppendOutput("                       "); // a sort of "waiting"
            
            AddMethodToIEnumeratorList(ClearOutputIEnumerator());
            
            AppendOutput("Type 'HELP' for instructions", false);
            
            _logger.LogMinor("Started TerminalControllerScript");
            inputField.text = "";
            
            if (keyBeep && audioSource) audioSource.PlayOneShot(typingSoundClip);
        }

        #endregion
        
        private void Update()
        {
            // Always ensure the input field is focused
            if (!inputField.isFocused && _isOpen) {
                inputField.ActivateInputField();
                //_logger.Log("Focusing InputField, isFocused now: " + inputField.isFocused);
            }

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

            if (input.All(char.IsWhiteSpace)) {
                return;
            }
            
            Debug.Log($"Input: {input}");
            
            // here we get some helpful vars (deconstruct input)
            string[] parts = input.Split(' ');
            string cmd = parts[0];
            string autoCompletedCmd = AutoComplete(true, _hasToSpecialCommand, cmd);

            string[] args = parts.Skip(1).ToArray();
            string argString = string.Join(" ", args);
            string processedInput = $"{autoCompletedCmd} {argString}";
            bool success;
            
            Debug.Log($"cmd: {cmd} and automcompleted: {autoCompletedCmd}");
            CommandHistory.Add(processedInput);  
            _historyIndex = CommandHistory.Count;
            
            AppendOutput($"\n{prefixText}{dir}: {processedInput}", false);


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
            if (_queueCoroutines.Count > 0 && !_isBusy) {
                StartCoroutine(_queueCoroutines[0]);
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
        
        private IEnumerator TypeText(string text, StyleText style, bool playEndSound) {
            float speed = 0.1f;
            string styledText = text; 
            _isBusy = true;

            if (style != null) {
                styledText = style.ApplyStyle(text);
                speed = style.TypeSpeed;
            }
            else {
                UnityEngine.Debug.Log("Couldnt apply style, due to style = null.");
            }
            
            speed = goSlower? speed + 1f : speed;
            
            for (int i = 0; i < styledText.Length; i++)
            {
                char c = styledText[i];
                outputText.text += c;
                                                                                                        
                if (keyBeep && c != " "[0]) audioSource.PlayOneShot(typingSoundClip);
                if (i == styledText.Length - 1 && endOfLineSoundClip && playEndSound) audioSource.PlayOneShot(endOfLineSoundClip);
                
                yield return new WaitForSeconds(speed);
            }
            
            _isBusy = false;
            outputText.text += "\n ";
            _queueCoroutines.RemoveAt(0);
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

        public void CloseTerminal() {
            _isOpen = false;
        }

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
        
        public void AppendOutput(string text, bool indent = true, bool type = true, StyleText style = null, bool playEndSound = false)
        {
            
            if (indent) {
                text = text.Insert(0, "   ");
            }
            
            if (type) {
                var txtStyle = style ?? _defaultStyle;
                AddMethodToIEnumeratorList(TypeText(text, txtStyle, playEndSound));
            }
            else {
                outputText.text += text + "\n ";
            }
        }

        #region Helpers
        
        private void AddMethodToIEnumeratorList(IEnumerator method) {
            var newCoroutine = method;
            _queueCoroutines.Add(newCoroutine);
        }
        
        public void AddSpecialCommand(string commandName, CommandSO command) {
            _specialCommands.Add(commandName, command);
        }
        
        public void SetRequiredCommandState(bool shouldDoSpecialCommand) {
            _hasToSpecialCommand = shouldDoSpecialCommand;
        }
        
        private void CleanUpSpecialCommands() {
            foreach (var specialCmd in _specialCommands) {
                _logger.LogMinor("Clearing: " + specialCmd.Key + ": " + specialCmd.Value);
                CommandSO cmd = specialCmd.Value;
                cmd.commandItemsPrefabs.Clear(); // we clear the list of items for the special command
            }
            _specialCommands.Clear(); // we clear the list of special commands
            _hasToSpecialCommand = false;
        }

        public void ClearOutput() {
            outputText.text = "";
        }

        public IEnumerator ClearOutputIEnumerator() {
            _isBusy = true;
            outputText.text = "";
            _isBusy = false;
            _queueCoroutines.RemoveAt(0);
            yield break;
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
            _logger.LogMinor("Returning default text style...");
            return defaultStyle;
        }

        public void OpenTerminal() {
            _isOpen = true;
        }
        
        #endregion
    }
}
