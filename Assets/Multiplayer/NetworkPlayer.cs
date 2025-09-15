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
            // Movimiento simple para testear
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * 5f);
        }
    }
}
