using UnityEngine;

namespace UI.Scripts.Commands {
    
    [CreateAssetMenu(fileName = "DenyCommand", menuName = "UI/Terminal/Commands/Deny")]
    public class DenyCommandSO : CommandSO
    {
        
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            terminal.AppendOutput("Canceled operation.");
        }
    }
}