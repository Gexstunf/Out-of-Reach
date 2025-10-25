using UnityEngine;

namespace UI.Scripts.Commands {
    
    [CreateAssetMenu(fileName = "AvailableCommand", menuName = "UI/Terminal/Commands/Available")]
    public class AvailableCommandSO : CommandSO
    {
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            foreach (var item in commandItemsPrefabs) {
                var objName = item.displayName != "" ? item.displayName : "?";
                var chosenStyle = terminal.GetStyleText(TerminalControllerScript.StyleText.EStyle.Normal);

                if (item == commandItemsPrefabs[^1]) 
                    terminal.AppendOutput($"- {objName}: ${item.value}", type:true, style:chosenStyle, playEndSound:true); 
                else 
                    terminal.AppendOutput($"- {objName}: ${item.value}", type:true, style:chosenStyle );
            }
        }

        public override void Reset() {
            throw new System.NotImplementedException();
        }
    }
}
