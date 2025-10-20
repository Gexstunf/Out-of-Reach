namespace UI.Scripts.Commands {
    using UnityEngine;

    [CreateAssetMenu(fileName = "WhoAmI_Command", menuName = "UI/Terminal/Commands/WhoAmI")]
    public class WhoAmICommandSO : CommandSO
    {
        public override void Execute(string[] args, TerminalControllerScript terminal)
        {
            terminal.AppendOutput(terminal.userName, false);
        }
    }
}