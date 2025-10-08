using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public class StructureScript : MonoBehaviour
    {
        void Start()
        {
            Collider myCol = GetComponent<Collider>();
            
            Collider[] others = Physics.OverlapBox(
                myCol.bounds.center,
                myCol.bounds.extents,
                transform.rotation
            );

            foreach (Collider other in others)
            {
                if (other != myCol)
                {
                    Vector3 dir;
                    float distance;
                    
                    if (Physics.ComputePenetration(
                            myCol, transform.position, transform.rotation,
                            other, other.transform.position, other.transform.rotation,
                            out dir, out distance)) {
                        if (other.CompareTag("Indestructible"))
                        {
                            if (distance > 0.1f)
                            {
                                Destroy(gameObject);
                                return;
                            }
                        }
                    }
                    else
                    {
                        gameObject.tag = "Indestructible";
                    }
                }
            }
        }
    }
}