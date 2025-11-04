using UnityEngine;

namespace UI.Scripts {
    [CreateAssetMenu(fileName = "ExitCommand", menuName = "UI/Terminal/Commands/Exit")]
    public class ExitCommandSO : CommandSO {
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            terminal.AppendOutput("Closing terminal...", false);
            terminal.CloseTerminal(); 
        }

        public override void Reset() {
            throw new System.NotImplementedException();
        }
    }
}