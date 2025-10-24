using Multiplayer.Inventory;
using UnityEngine;

namespace UI.Scripts.Commands {
    
    [CreateAssetMenu(fileName = "ConfirmCommand", menuName = "UI/Terminal/Commands/Confirm")]
    public class ConfirmCommandSO : CommandSO
    {
        
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            ItemSO item = commandItemsPrefabs[0];
            terminal.AppendOutput("");
            terminal.AppendOutput("Bought!", playEndSound:true);
            
            Debug.Log("ITEM BOUGHT: " + item.displayName);
            Debug.Log("ITEM PREFAB: " + item.prefab);
            
            if (item.prefab) {
                Instantiate(item.prefab);
            }
            else {
                Debug.LogWarning($"The: {item.displayName} item doesn't have a prefab in its ItemSO!");
            }
        }
    }
}
