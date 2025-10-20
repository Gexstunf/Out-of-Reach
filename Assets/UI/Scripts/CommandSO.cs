using System.Collections.Generic;
using Multiplayer.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Scripts {
    public abstract class CommandSO : ScriptableObject {
        public string commandName;
        public string commandDescription;
        public List<ItemSO> commandItemsPrefabs;
        public CommandSO[] specialCommands;
        
        public abstract void Execute(string[] arguments, TerminalControllerScript terminal);
    }
}
