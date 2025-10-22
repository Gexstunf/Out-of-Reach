using System.Linq;
using UnityEngine;

namespace UI.Scripts.Commands {
    [CreateAssetMenu(fileName = "BuyCommand", menuName = "UI/Terminal/Commands/Buy")]
    public class BuyCommandSO : CommandSO {
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            if (arguments.Length == 0) {
                terminal.AppendOutput("Usage: buy <item> <quantity>", false);
            }
            else {

                int quantity = 1;
                string objName = arguments[0];

                if (arguments.Length > 1) {
                    var num = int.Parse(arguments[1]);
                    quantity = Mathf.Abs(num);
                }
                

                foreach (var item in commandItemsPrefabs) {
                    if (item.displayName.ToLower() == objName && quantity > 0) {
                        var chosenStyle = terminal.GetStyleText(TerminalControllerScript.StyleText.EStyle.Normal);
                        terminal.AppendOutput($"purchase: {quantity} {objName} for ${item.value * quantity}?", false, type:true, style:chosenStyle);
                        terminal.SetRequiredCommandState(true);
                        terminal.AppendOutput($"type:", false, type:true);
                        // Adding special options (deny-confirm)
                        foreach (var specialCmd in specialCommands) {
                            Debug.Log("Adding special Command to the terminal: " + specialCmd.commandName);
                            terminal.AppendOutput($"- {specialCmd.commandName}", type:true);
                            specialCmd.commandItemsPrefabs.Insert(0, item);
                            terminal.AddSpecialCommand(specialCmd.commandName, specialCmd);
                        }
                        
                        return;
                    }
                }
                
                terminal.AppendOutput("Invalid item purchase.");
            }
        }
    }
}
