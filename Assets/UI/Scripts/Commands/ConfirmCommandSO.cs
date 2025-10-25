using Characters.EconomySystem;
using Multiplayer.Inventory;
using UnityEngine;

namespace UI.Scripts.Commands {
    
      public class ConfirmCommandSO : CommandSO {
          
            public GlobalBankSO globalBankSO;
            
            public override void Execute(string[] arguments, TerminalControllerScript terminal) {
                ItemSO item = commandItemsPrefabs[0];
                terminal.AppendOutput("");
                terminal.AppendOutput("Bought!", false);
                terminal.AppendOutput("");
                terminal.AppendOutput($"New balance: {globalBankSO.Balance}", playEndSound:true);
                
                Debug.Log("ITEM BOUGHT: " + item.displayName);
                Debug.Log("ITEM PREFAB: " + item.prefab);
                
                if (item.prefab) {
                    for (int i = 0; i < quantity; i++) {
                        Instantiate(item.prefab);
                    }
                    globalBankSO.Spend(item.value * quantity);
                }
                else {
                    Debug.LogWarning($"The: {item.displayName} item doesn't have a prefab in its ItemSO!");
                }
            }
    }
}
