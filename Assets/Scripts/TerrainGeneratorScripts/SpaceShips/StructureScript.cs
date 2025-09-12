using UnityEngine;

namespace TerrainGeneratorScripts.SpaceShips
{
    public class StructureScript : MonoBehaviour
    {
        void Start()
        {
            Collider myCol = GetComponent<Collider>();

            // Buscamos todos los colliders cerca de este objeto
            Collider[] others = Physics.OverlapBox(
                myCol.bounds.center,
                myCol.bounds.extents,
                transform.rotation
            );

            foreach (Collider other in others)
            {
                if (other != myCol) // ignorar mi propio collider
                {
                    Vector3 dir;
                    float distance;

                    // Si hay penetración real entre colliders
                    if (Physics.ComputePenetration(
                            myCol, transform.position, transform.rotation,
                            other, other.transform.position, other.transform.rotation,
                            out dir, out distance))
                    {
                        if (other.CompareTag("Indestructible"))
                        {
                            if (distance > 0.1f) // margen para no contar solo el "roce"
                            {
                                Destroy(gameObject);
                                Debug.Log("Se destruyó " + gameObject.name + "porque chocó con " + other.name);
                                return; // me destruyo, no sigo chequeando
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