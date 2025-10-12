using UnityEngine;

namespace Characters.Enemies.Scripts {
    public class EnemySpawnPointScript : MonoBehaviour {

        public GameObject enemyPrefab;
        void Start()
        {
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }
    }
}
