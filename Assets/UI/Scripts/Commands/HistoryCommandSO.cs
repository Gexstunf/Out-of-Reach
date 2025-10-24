using UnityEngine;

namespace UI.Scripts.Commands {
    [CreateAssetMenu(fileName = "HistoryCommand", menuName = "UI/Terminal/Commands/History")]
    public class HistoryCommandSO : CommandSO
    {

        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            if (terminal.CommandHistory == null || terminal.CommandHistory.Count == 0) {
                terminal.AppendOutput("No history available.", false, playEndSound:true);
                return;
            }
            terminal.AppendOutput("History:", false);
            terminal.AppendOutput("");

            for (int i = 0; i < terminal.CommandHistory.Count; i++)
            {
                if (i == terminal.CommandHistory.Count - 1) {
                    Debug.Log("Not showing last command"); 
                    // this is not actual last command ( due to this one being counted in, though it was just made )
                    return;
                }

                // play end sound for -actual- last command
                if (i != terminal.CommandHistory.Count - 2) {
                    terminal.AppendOutput($"{i + 1}. {terminal.CommandHistory[i]}");
                }
                else {
                    terminal.AppendOutput($"{i + 1}. {terminal.CommandHistory[i]}", playEndSound:true);
                }
            }
        }
    }
}
