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

                if (arguments.Length == 2) {
                    quantity = int.Parse(arguments[1]);
                }
                

                foreach (var item in commandItemsPrefabs) {
                    if (item.displayName.ToLower() == objName && quantity > 0) {
                        terminal.AppendOutput($"purchase: {quantity} {objName} for ${item.value * quantity}?", false);
                        terminal.SetRequiredCommandState(true);
                        terminal.AppendOutput($"type:", false);
                        
                        // Adding special options (deny-confirm)
                        foreach (var specialCmd in specialCommands) {
                            specialCmd.commandItemsPrefabs.Add(item);
                            terminal.AppendOutput($"- {specialCmd.commandName}");
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
