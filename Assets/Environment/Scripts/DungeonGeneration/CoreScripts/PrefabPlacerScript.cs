using System.Collections.Generic;
using Environment.Scripts.DungeonGeneration.Data;
using Environment.Scripts.DungeonGeneration.Utils;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UIElements;

namespace Environment.Scripts.DungeonGeneration.CoreScripts {
    public class PrefabPlacerScript : MonoBehaviour {

        [Header("References")]
        [SerializeField] private PlacementValidatorScript validator;
        
        [Header("Settings")]
        [SerializeField, Range(0, 1f)] private float shrinkBoundsFactor = 0.05f;
        public bool usePhoton;

        private int _boundsDrawerAmount = 0;
        
        public StructureInstanceScript PlaceInitial(StructurePrefabScript structurePrefab, Vector3 startPosition, int drawerId = 0) {
            GameObject obj = usePhoton ? PhotonNetwork.Instantiate(structurePrefab.prefab.name, startPosition, Quaternion.identity) : Instantiate(structurePrefab.prefab, startPosition, Quaternion.identity);
            StructureInstanceScript instance = new StructureInstanceScript(obj, structurePrefab);
            instance.UpdateBounds(shrinkBoundsFactor);
            GiveBoundsDrawer(instance, drawerId);
            
            return instance;
        }

        public (bool success, StructureInstanceScript instance) TryPlacePrefab(
            StructurePrefabScript structurePrefab, 
            StructureSocketScript attachSocket, 
            List<StructureInstanceScript> existing, 
            bool ignoreOverlap = false
        ) {
            GameObject candidate = usePhoton ? PhotonNetwork.Instantiate(structurePrefab.prefab.name, Vector3.zero, Quaternion.identity) : Instantiate(structurePrefab.prefab);
            StructureInstanceScript newInstance = new StructureInstanceScript(candidate, structurePrefab);

            foreach (var entry in newInstance.GetEntries()) {
                Align(entry.transform, attachSocket.transform, newInstance, attachSocket);
                newInstance.UpdateBounds(shrinkBoundsFactor);
                
                if (!validator.Overlaps(newInstance, existing) || ignoreOverlap) {
                    attachSocket.SetConnected(true);
                    entry.SetConnected(true);
                    GiveBoundsDrawer(newInstance, _boundsDrawerAmount);
                    return (true, newInstance);
                }
            }
            
            //Debug.Log("Destroying instance, overlapped");
            Destroy(candidate);
            return (false, null);
        }

        private void Align(Transform entry, Transform targetExit, StructureInstanceScript newInstance, StructureSocketScript attachSocket) {
            //Debug.Log("Aligning instance to match target exit");
            AlignRotationMethod(entry, targetExit, newInstance);
            AlignPositionMethod(entry, targetExit, newInstance);  
        }

        #region Aligning logic
        
        private void AlignRotationMethod(Transform entry, Transform targetExit, StructureInstanceScript newInstance)
        {
            Transform root = newInstance.instance.transform;

            // make the entry socket face the opposite direction of the target socket
            Quaternion rot = Quaternion.FromToRotation(entry.forward, -targetExit.forward);
            root.rotation = rot * root.rotation;
            
            if (Vector3.Dot(newInstance.instance.transform.up, Vector3.down) > 0) { // this is for instances that are correctly aligned, but flipped
                // if upside-down, flip 180 around forward
                newInstance.instance.transform.Rotate(0, 0, 180, Space.Self);
            }
        }
        
        private void AlignRotationMethodUpright(Transform entry, Transform target, StructureInstanceScript newInstance)
        {
            Transform root = newInstance.instance.transform;

            // Align forward
            Vector3 forward = -target.forward;

            // Force "up" to match world up
            Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);

            // Apply
            root.rotation = targetRot;
        }

        
        private void AlignRotationMethodSocketAsPivot(Transform entry, Transform target, StructureInstanceScript newInstance) // allows different orientation
        {
            Transform root = newInstance.instance.transform;

            // Compute final world rotation: 
            // entry-local orientation → target-world orientation
            Quaternion delta = target.rotation * Quaternion.Inverse(entry.rotation);
            root.rotation = delta * root.rotation;
        }

        
        private void AlignPositionMethod(Transform entry, Transform targetExit, StructureInstanceScript newInstance) {
            Transform root = newInstance.instance.transform;

            // Recalculate entry position AFTER rotation
            Vector3 delta = (targetExit.position - entry.position);

            // Move the entire instance so entry → target
            root.position += delta;
        }
        
        #endregion
        
        private void GiveBoundsDrawer(StructureInstanceScript structureInstance, int id) {
            var drawer = structureInstance.instance.AddComponent<BoundsDrawerScript>();
            drawer.SetBounds(structureInstance.ShrunkBounds, id);
            _boundsDrawerAmount++;
        }
    }
}
