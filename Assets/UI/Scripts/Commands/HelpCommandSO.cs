using UnityEngine;

namespace UI.Scripts.Commands {
    [CreateAssetMenu(fileName = "HelpCommand", menuName = "UI/Terminal/Commands/Help")]
    public class HelpCommandSO : CommandSO {
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {

            var chosenStyle = terminal.GetStyleText(TerminalControllerScript.StyleText.EStyle.Slow);
            Debug.Log("Using this text style: " + chosenStyle.typeStyle);
            terminal.AppendOutput("Available Commands:", false, type:true, chosenStyle);

            foreach (var cmd in terminal.AllAvailableCommands()) {
                terminal.AppendOutput($" - {cmd.commandName}: {cmd.commandDescription}", type:true, style:chosenStyle);
            }
        }
    }
}