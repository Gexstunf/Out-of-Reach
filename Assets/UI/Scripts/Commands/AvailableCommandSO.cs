using UnityEngine;

namespace UI.Scripts.Commands {
    
    [CreateAssetMenu(fileName = "AvailableCommand", menuName = "UI/Terminal/Commands/Available")]
    public class AvailableCommandSO : CommandSO
    {
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            foreach (var item in commandItemsPrefabs) {
                var objName = item.displayName != "" ? item.displayName : "?";
                terminal.AppendOutput($"- {objName}: ${item.value}");
            }
        }
    }
}
