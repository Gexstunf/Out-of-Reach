using Photon.Pun;
using UnityEngine;

public class NetworkPlayer : MonoBehaviourPun
{
    void Start()
    {
        if (!photonView.IsMine)
        {
            // Esto no es el jugador local → desactiva cámara o input
            GetComponentInChildren<Camera>().enabled = false;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            
        }
    }
}
