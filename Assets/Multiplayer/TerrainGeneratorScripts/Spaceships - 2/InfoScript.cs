using System;
using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        [SerializeField] BranchDivisionScript branchDivision;
        public string estructuraID;
        private void Awake()
        {
            Collider myCol = GetComponent<Collider>();
            Collider[] others = Physics.OverlapBox(
                myCol.bounds.center,
                myCol.bounds.extents,
                transform.rotation
            );
            
            branchDivision = FindAnyObjectByType<BranchDivisionScript>();
            if (gameObject.CompareTag("Hallway"))
            {
                if (!branchDivision.Rama.ContainsKey(estructuraID) && estructuraID != "")
                {
                    branchDivision.Rama.Add(estructuraID, this);
                }
                else
                {
                    estructuraID = Guid.NewGuid().ToString();
                    branchDivision.Rama.Add(estructuraID, this);
                }
                foreach (Collider other in others)
                {
                    if (other != myCol)
                    {
                        Vector3 dir;
                        float distance;
                    
                        if (Physics.ComputePenetration(
                                myCol, transform.position, transform.rotation,
                                other, other.transform.position, other.transform.rotation,
                                out dir, out distance)) 
                        {
                            if (distance > 0.5f)
                            {
                                branchDivision.AccionRama(true);
                            }
                        }
                    }
                }
            }
            else if (gameObject.CompareTag("Intersection"))
            {
                if (!branchDivision.EntreRama.ContainsKey(estructuraID) && estructuraID != "")
                {
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                else
                {
                    estructuraID = Guid.NewGuid().ToString();
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                foreach (Collider other in others)
                {
                    if (other != myCol)
                    {
                        Vector3 dir;
                        float distance;
                    
                        if (Physics.ComputePenetration(
                                myCol, transform.position, transform.rotation,
                                other, other.transform.position, other.transform.rotation,
                                out dir, out distance)) 
                        {
                            if (distance > 0.5f)
                            {
                                branchDivision.AccionRama(true);
                            }
                            else
                            {
                                branchDivision.AccionRama(false);
                            }
                        }
                    }
                }
            }
            else if (gameObject.CompareTag("Room"))
            {
                branchDivision.AccionRama(false);
                if (!branchDivision.EntreRama.ContainsKey(estructuraID) && estructuraID != "")
                {
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                else
                {
                    estructuraID = Guid.NewGuid().ToString();
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                foreach (Collider other in others)
                {
                    if (other != myCol)
                    {
                        Vector3 dir;
                        float distance;
                    
                        if (Physics.ComputePenetration(
                                myCol, transform.position, transform.rotation,
                                other, other.transform.position, other.transform.rotation,
                                out dir, out distance)) 
                        {
                            if (distance > 0.5f)
                            {
                                branchDivision.AccionRama(true);
                            }
                            else
                            {
                                branchDivision.AccionRama(false);
                            }
                        }
                    }
                }
            }
        }
    }
}
