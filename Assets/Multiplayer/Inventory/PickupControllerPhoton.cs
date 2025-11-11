using Multiplayer.Inventory;
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
            Debug.LogError("[PickupControllerPhoton] No se encontró la cámara del player local.");

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
        if (!photonView.IsMine) return;
        if (playerCamera == null || inventory == null) return;

        // Detectar clic izquierdo para pickup
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
            {
                var pv = hit.collider.GetComponentInParent<PhotonView>();
                var netItem = hit.collider.GetComponentInParent<NetworkedItem>();

                if (pv != null && netItem != null)
                {
                    Debug.Log($"[Pickup] Click detectado sobre '{netItem.name}' con ViewID {pv.ViewID}");
                    Debug.Log($"[Pickup] Solicitud de pickup enviada para item '{netItem.name}'");

                    //inventory.RequestPickupOnClosest(pv);
                }
            }
        }

        /*
        // Detectar "E" para abrir mochila (en mano o del mundo)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inventory == null) return;

            // Mochila equipada en slot 4 y activa
            if (inventory.backpackObj != null && inventory.slots[3] != null && inventory.activeSlot == 3)
            {
                Debug.Log("[Pickup] Abrir mochila equipada en mano");
                inventory.OpenBackpack();
                return;
            }

            // Buscar mochila cercana en el mundo
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f, inventory.itemLayer);
            foreach (var hit in hits)
            {
                var netItemWorld = hit.GetComponentInParent<NetworkedItem>();
                if (netItemWorld != null && netItemWorld.itemData != null && netItemWorld.itemData.itemType == ItemType.Backpack)
                {
                    var bd = netItemWorld.GetComponent<BackpackData>();
                    if (bd != null)
                    {
                        Debug.Log($"[Pickup] Abrir mochila en el mundo: {netItemWorld.name}");
                        var ui = FindFirstObjectByType<PlayerUIManager>();
                        if (ui != null)
                        {
                            ui.ShowBackpackInventory(bd, inventory);
                        }
                    }
                    break;
                }
            }
        }
        */
    }
}
