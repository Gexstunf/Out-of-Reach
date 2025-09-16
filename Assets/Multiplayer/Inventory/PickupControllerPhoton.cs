using Photon.Pun;
using UnityEngine;
using UI;

public class PickupControllerPhoton : MonoBehaviourPun
{
    [Header("Configuración")]
    public float pickupRange = 3f;
    public LayerMask itemLayer;
    public PlayerInventoryPhoton inventory;

    private Camera playerCamera;

    void Start()
    {
        // Solo el jugador local necesita la cámara
        if (!photonView.IsMine) return;

        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError("[PickupControllerPhoton] No se encontró la cámara del player local.");
        }

        // Asegúrate de tener la referencia al inventario
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventoryPhoton>();
            if (inventory == null)
                Debug.LogError("[PickupControllerPhoton] No se encontró PlayerInventoryPhoton en el jugador.");
        }
    }

    void Update()
    {
        // Solo el jugador local puede hacer pickup
        if (!photonView.IsMine) return;
        if (playerCamera == null || inventory == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
            {
                var pv = hit.collider.GetComponentInParent<PhotonView>();
                var netItem = hit.collider.GetComponentInParent<NetworkedItem>();

                if (pv != null && netItem != null)
                {
                    // Solicita pickup al MasterClient
                    inventory.RequestPickupOnClosest(pv);
                    Debug.Log($"[Pickup] Click detectado sobre '{netItem.name}'");
                }
            }
        }
    }
}
