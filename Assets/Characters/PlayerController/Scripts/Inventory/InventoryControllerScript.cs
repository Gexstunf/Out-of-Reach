using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Items.Scripts;
using UnityEngine;
using Photon.Pun;
using Multiplayer.Inventory;

public class InventoryControllerScript : MonoBehaviourPun
{
    [Header("References")]
    [SerializeField] private HandGrabberScript handGrabber;

    public PlayerInputScript input;
    public GameObject[] inventory;
    public ItemSO[] itemSOs;


    private PhotonView _photonView;

    public void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        handGrabber = GetComponent<HandGrabberScript>();
        input = GetComponent<PlayerInputScript>();
    }

    public void Update()
    {
        if (!photonView.IsMine) return;

        if (input.InventoryInteraction && handGrabber.currentItem != null)
        {
            // handGrabber.currentItem =   IGrabbableScript  ( a generic grabbable class )
            //ItemGrabbableScript itemGrabbable = GetComponent<>();

            ItemGrabbableScript itemGrabbable = handGrabber.currentItem as ItemGrabbableScript;

            if (!itemGrabbable) return;

            GameObject itemObject = itemGrabbable.gameObject;

            for (int i = 0; i < inventory.Length; i++) {
                // bring in logic
                if (input.InventoryIndex == i && inventory[i] == null)
                {
                    //save a reference to the inventory for later instantiation.
                    inventory[i] = itemObject;
                    DestroyObject(itemObject);
                }
            }
        }

        if (input.InventoryInteraction)
        {
            for (int i = 0; i < inventory.Length; i++)
            {

                if (input.InventoryIndex == i && inventory[i] != null)
                {
                    // bring out logic
                }
            }
        }
    }

    private void DestroyObject(GameObject obj)
    {
        if (obj == null) return;


        //DestroyObjectRPC();
        //  destroy
    }

    private void InstantiateObject(GameObject obj)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == obj)
            {
                inventory[i] = null;
            }
        }

        //InstantiateObjectRPC()  
        //  instantiate
    }

    private void InstantiateObjectRPC()
    {
        //photonView.RPC()    Instantiate this "obj" for all players.
    }
    private void DestroyObjectRPC()
    {
        //photonView.RPC()    Destroy this "obj" for all players.
    }
}
