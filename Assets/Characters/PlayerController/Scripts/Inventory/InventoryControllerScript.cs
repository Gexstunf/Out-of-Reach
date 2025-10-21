using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Items.Scripts;
using UnityEngine;
using Photon.Pun;
using Multiplayer.Inventory;
using System.Collections.Generic;

public class InventoryControllerScript : MonoBehaviourPun
{
    [Header("References")]
    [SerializeField] private HandGrabberScript _handGrabber;
    [SerializeField] private PhotonObjectManagerScript _photonObjManager;

    public PlayerInputScript input;
    public GameObject[] inventory;
    public Dictionary<GameObject, ItemSO> itemSOs;


    private PhotonView _photonView;

    public void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _handGrabber = GetComponent<HandGrabberScript>();
        input = GetComponent<PlayerInputScript>();
    }

    public void Update()
    {
        if (!photonView.IsMine) return;

        if (input.InventoryInteraction && _handGrabber.currentItem != null)
        {
            // handGrabber.currentItem =   IGrabbableScript  ( a generic grabbable class )
            //ItemGrabbableScript itemGrabbable = GetComponent<>();

            ItemGrabbableScript itemGrabbable = _handGrabber.currentItem as ItemGrabbableScript;

            if (!itemGrabbable) return;

            GameObject itemObject = itemGrabbable.gameObject;

            for (int i = 0; i < inventory.Length; i++) {
                // bring in logic
                if (input.InventoryIndex == i && inventory[i] == null)
                {
                    //save a reference to the inventory for later instantiation.
                    inventory[i] = itemObject;
                    itemSOs.Add(itemObject, itemGrabbable.data);
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
        _photonObjManager.DestroyObjectForAll(obj);
    }

    private void InstantiateObject(GameObject obj)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == obj)
            {
                inventory[i] = null;
                if (!itemSOs[obj]) return;
                _photonObjManager.InstantiateObjectForAll(
                    itemSOs[obj].prefab.name,
                    transform.InverseTransformPoint(transform.localPosition),
                    transform.rotation
                );
            }
        }
    }
}
