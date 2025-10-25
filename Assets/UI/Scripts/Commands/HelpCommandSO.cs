using UnityEngine;

namespace UI.Scripts.Commands {
    [CreateAssetMenu(fileName = "HelpCommand", menuName = "UI/Terminal/Commands/Help")]
    public class HelpCommandSO : CommandSO {
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {

            var chosenStyle = terminal.GetStyleText(TerminalControllerScript.StyleText.EStyle.Fast);
            Debug.Log("Using this text style: " + chosenStyle.typeStyle);
            terminal.AppendOutput("Available Commands:", false, type:true, chosenStyle);
            terminal.AppendOutput("");

            var allCommands = terminal.AllAvailableCommands();
            foreach (var cmd in allCommands ) {
                if (cmd == allCommands[^1]) {
                    terminal.AppendOutput($" - {cmd.commandName}: {cmd.commandDescription}", type:true, style:chosenStyle, playEndSound:true);
                }
                else {
                    terminal.AppendOutput($" - {cmd.commandName}: {cmd.commandDescription}", type:true, style:chosenStyle);
                }
            }
        }

        public override void Reset() {
            throw new System.NotImplementedException();
        }
    }
}