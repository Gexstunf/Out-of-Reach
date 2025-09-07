using Characters.LifeSupportSystem.PlayerLifeSupport;
using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPun
{
    private PlayerLifeSupportContextScript context;

    void Start()
    {
        if (photonView.IsMine)
        {
            // Buscar el HUD local
            PlayerUIManager ui = FindFirstObjectByType<PlayerUIManager>();

            if (ui != null)
            {
                context = GetComponent<PlayerLifeSupportContextScript>();
                if (context != null)
                {
                    // Enlazar el UI Manager al contexto
                    typeof(PlayerLifeSupportContextScript)
                        .GetField("_uiManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(context, ui);
                }
            }
        }
        else
        {
            // Desactivar cámara y controles de los jugadores remotos
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }
    }
}
