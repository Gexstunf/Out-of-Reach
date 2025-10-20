using UnityEngine;

namespace UI.Scripts.Commands {
    [CreateAssetMenu(fileName = "ClearCommand", menuName = "UI/Terminal/Commands/Clear")]
    public class ClearCommandSO : CommandSO {
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            terminal.ClearOutput();
        }
    }
}