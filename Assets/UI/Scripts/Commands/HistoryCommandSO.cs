using UnityEngine;

namespace UI.Scripts.Commands {
    [CreateAssetMenu(fileName = "HistoryCommand", menuName = "UI/Terminal/Commands/History")]
    public class HistoryCommandSO : CommandSO
    {

        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            if (terminal.CommandHistory == null || terminal.CommandHistory.Count == 0) {
                terminal.AppendOutput("No history available.", false);
                return;
            }
            terminal.AppendOutput("History:", false);

            for (int i = 0; i < terminal.CommandHistory.Count; i++)
            {
                if (i == terminal.CommandHistory.Count - 1) {
                    Debug.Log("Not showing last command");
                    return;
                }
                
                terminal.AppendOutput($"{i + 1}. {terminal.CommandHistory[i]}");
            }
        }
    }
}
