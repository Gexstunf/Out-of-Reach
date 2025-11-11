using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public class PhotonObjectManagerScript : MonoBehaviourPun
{
    public static PhotonObjectManagerScript Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // registrar objetos instanciados (para referencia o limpieza)
    private readonly Dictionary<int, GameObject> trackedObjects = new();


    /// <summary>
    /// Instancia un objeto sincronizado globalmente para todos los jugadores.
    /// </summary>
    public GameObject InstantiateObjectForAll(string prefabName, Vector3 pos, Quaternion rot)
    {
        GameObject obj = PhotonNetwork.Instantiate(prefabName, pos, rot);
        PhotonView view = obj.GetComponent<PhotonView>();

        if (view != null)
        {
            // asegúrate de que el nombre del objeto sea único
            obj.name = $"{prefabName}_ViewID_{view.ViewID}";
            trackedObjects[view.ViewID] = obj;
        }

        return obj;
    }

    /// <summary>
    /// Instancia un objeto solo para un jugador específico (localmente en su cliente).
    /// </summary>
    public void InstantiateObjectForTarget(string prefabName, Vector3 pos, Quaternion rot, Player targetPlayer)
    {
        photonView.RPC(nameof(RPC_InstantiateForTarget), targetPlayer, prefabName, pos, rot);
    }

    [PunRPC]
    private void RPC_InstantiateForTarget(string prefabName, Vector3 pos, Quaternion rot)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>(prefabName), pos, rot);
        // Estos objetos no están sincronizados con PhotonNetwork
        obj.name = prefabName + "_LocalClone";
    }
    /// <summary>
    /// Destruye un objeto sincronizado globalmente para todos los jugadores.
    /// </summary>
    public void DestroyObjectForAll(GameObject obj)
    {
        if (obj == null) return;

        PhotonView view = obj.GetComponent<PhotonView>();
        if (view != null)
        {
            trackedObjects.Remove(view.ViewID);
            PhotonNetwork.Destroy(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// Destruye un objeto solo en el cliente destino (no afecta a otros jugadores).
    /// </summary>
    public void DestroyObjectForTarget(GameObject obj, Player targetPlayer)
    {
        if (obj == null) return;

        PhotonView view = obj.GetComponent<PhotonView>();
        int viewID = view ? view.ViewID : -1;

        photonView.RPC(nameof(RPC_DestroyForTarget), targetPlayer, viewID);
    }

    [PunRPC]
    private void RPC_DestroyForTarget(int viewID)
    {
        if (trackedObjects.TryGetValue(viewID, out GameObject obj))
        {
            Destroy(obj);
            trackedObjects.Remove(viewID);
        }
    }

    /// <summary>
    /// Transfiere la propiedad (ownership) de un objeto a otro jugador.
    /// </summary>
    public void TransferOwnership(GameObject obj, Player newOwner)
    {
        PhotonView view = obj.GetComponent<PhotonView>();
        if (view != null && PhotonNetwork.IsMasterClient)
        {
            view.TransferOwnership(newOwner);
        }
    }

    /// <summary>
    /// Envía un RPC genérico desde este manager a un jugador o a todos.
    /// </summary>
    
    public void SendRPC(string rpcName, RpcTarget target, params object[] parameters)
    {
        photonView.RPC(rpcName, target, parameters);
    }

    public void ClearAllTrackedObjects()
    {
        trackedObjects.Clear();
    }
}
