using Multiplayer.Inventory;
using UnityEngine;

namespace UI.Scripts.Commands {
    
    [CreateAssetMenu(fileName = "ConfirmCommand", menuName = "UI/Terminal/Commands/Confirm")]
    public class ConfirmCommandSO : CommandSO
    {
        
        public override void Execute(string[] arguments, TerminalControllerScript terminal) {
            ItemSO item = commandItemsPrefabs[0];
            terminal.AppendOutput("Bought!");
            
            Debug.Log("ITEM BOUGHT: " + item.displayName);
            Debug.Log("ITEM PREFAB: " + item.prefab);

            Instantiate(item.prefab);

            commandItemsPrefabs = null; // clean up
        }
    }
}
