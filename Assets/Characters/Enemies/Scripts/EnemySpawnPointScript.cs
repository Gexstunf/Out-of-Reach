using UnityEngine;
using Photon.Pun;

public class EnemySpawnPointScript : MonoBehaviourPun
{
    public string plantEnemyName = "PlantEnemy";
    public string batEnemy = "BatEnemy";

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(plantEnemyName, transform.position, Quaternion.identity);
            PhotonNetwork.Instantiate(batEnemy, transform.position + Vector3.right * 1.5f, Quaternion.identity);
        }
    }
}
