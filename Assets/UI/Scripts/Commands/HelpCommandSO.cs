using UnityEngine;

namespace UI.Scripts.Commands {
    [CreateAssetMenu(fileName = "HelpCommand", menuName = "UI/Terminal/Commands/Help")]
    public class HelpCommandSO : CommandSO {
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            terminal.AppendOutput("Available Commands:", false);

            foreach (var cmd in terminal.AllAvailableCommands()) {
                terminal.AppendOutput($" - {cmd.commandName}: {cmd.commandDescription}");
            }
        }
    }
}