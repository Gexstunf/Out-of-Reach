using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public class StructureScript : MonoBehaviour
    {
        public IdentificatorScript idScript;
        public IdentificatorScript otherIdScript;
        void Start()
        {
            Collider myCol = GetComponent<Collider>();
            
            Collider[] others = Physics.OverlapBox(
                myCol.bounds.center,
                myCol.bounds.extents,
                transform.rotation
            );
            
            idScript = myCol.GetComponent<IdentificatorScript>();

            foreach (Collider other in others)
            {
                if (other != myCol)
                {
                    Vector3 dir;
                    float distance;
                    
                    otherIdScript = other.GetComponent<IdentificatorScript>();
                    
                    if (Physics.ComputePenetration(
                            myCol, transform.position, transform.rotation,
                            other, other.transform.position, other.transform.rotation, 
                            out dir, out distance))
                    {
                        
                    }
                }
            }
        }
    }
}