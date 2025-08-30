using Photon.Pun;
using UnityEngine;

public class PickupControllerPhoton : MonoBehaviourPun
{
    public float pickupRange = 3f;
    public LayerMask itemLayer;
    public PlayerInventoryPhoton inventory;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
            {
                var pv = hit.collider.GetComponentInParent<PhotonView>();
                var netItem = hit.collider.GetComponentInParent<NetworkedItem>();
                if (pv != null && netItem != null)
                {
                    inventory.RequestPickupOnClosest(pv);
                }
            }
        }
    }
}
