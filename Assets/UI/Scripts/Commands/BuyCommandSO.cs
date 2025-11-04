using System.Linq;
using Characters.EconomySystem;
using Multiplayer.Inventory;
using UnityEngine;

namespace UI.Scripts.Commands {
    [CreateAssetMenu(fileName = "BuyCommand", menuName = "UI/Terminal/Commands/Buy")]
    public class BuyCommandSO : CommandSO {
        public GlobalBankSO globalBank;
        
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            if (arguments.Length == 0) {
                terminal.AppendOutput("Usage: buy <item> <quantity>", false);
            }
            else {

                string objName = arguments[0];

                if (arguments.Length > 1) {
                    var num = int.Parse(arguments[1]);
                    quantity = Mathf.Abs(num);
                }
                

                foreach (var item in commandItemsPrefabs) {
                    if (item.displayName.ToLower() == objName && quantity > 0) {

                        if (CanBuy(item)) {
                            var chosenStyle = terminal.GetStyleText(TerminalControllerScript.StyleText.EStyle.Normal);
                            terminal.AppendOutput($"purchase: {quantity} {objName} for ${item.value * quantity}?", false, type:true, style:chosenStyle);
                            terminal.SetRequiredCommandState(true);
                            terminal.AppendOutput($"type:", false, type:true);
                            terminal.AppendOutput("");
                        }
                        else {
                            terminal.AppendOutput($"You are poor.", false);
                            terminal.AppendOutput("");
                            terminal.AppendOutput($"price: {item.value * quantity}$");
                            terminal.AppendOutput($"balance: {globalBank.Balance}$");
                            return;
                        }


                        // Adding special options (deny-confirm)
                        foreach (var specialCmd in specialCommands) {
                            Debug.Log("Adding special Command to the terminal: " + specialCmd.commandName);
                            terminal.AppendOutput($"- {specialCmd.commandName}", type:true);
                            specialCmd.commandItemsPrefabs.Insert(0, item);
                            specialCmd.quantity = quantity;
                            terminal.AddSpecialCommand(specialCmd.commandName, specialCmd);
                        }
                        
                        return;
                    }
                }
                
                terminal.AppendOutput("Invalid item purchase.");
            }
        }

        public override void Reset() {
            throw new System.NotImplementedException();
        }

        private bool CanBuy(ItemSO item) {
            var price = quantity * item.value;
            if (price <= globalBank.Balance) {
                //Debug.Log($"Can buy, price: {price}, balance: {globalBank.Balance}");
                return true;
            }
            return false;
        }
    }
}
