using Multiplayer.TerrainGeneratorScripts.SpaceShips;
using UnityEngine;

public class IdentificatorScript : MonoBehaviour
{
    void Start()
    {
        ReloadManager.AddStructure(gameObject);
    }
}
