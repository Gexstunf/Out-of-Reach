using System.Collections.Generic;
using Environment.Scripts.DungeonGeneration.Data;
using Environment.Scripts.DungeonGeneration.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Environment.Scripts.DungeonGeneration.CoreScripts {
    public class PrefabPlacerScript : MonoBehaviour {
        [SerializeField] private PlacementValidatorScript validator;

        private int boundsDrawerAmount = 0;
        
        public StructureInstanceScript PlaceInitial(StructurePrefabScript structurePrefab, Vector3 startPosition) {
            GameObject inst = Instantiate(structurePrefab.prefab, startPosition, Quaternion.identity);
            return new StructureInstanceScript(inst, structurePrefab);
        }

        public (bool success, StructureInstanceScript instance) TryPlacePrefab(
            StructurePrefabScript structurePrefab, StructureSocketScript attachSocket, List<StructureInstanceScript> existing
            ) {
            GameObject candidate = Instantiate(structurePrefab.prefab);
            StructureInstanceScript newInstance = new StructureInstanceScript(candidate, structurePrefab);
            newInstance.UpdateBounds();
            var drawer = newInstance.instance.AddComponent<BoundsDrawerScript>();
            boundsDrawerAmount++;
            drawer.SetBounds(newInstance.Bounds, boundsDrawerAmount);

            foreach (var entry in newInstance.GetEntries()) {
                Align(entry.transform, attachSocket.transform, newInstance, attachSocket);
                if (!validator.Overlaps(newInstance, existing)) {
                    attachSocket.SetConnected(true);
                    entry.SetConnected(true);
                    return (true, newInstance);
                }
            }
            
            Debug.Log("Destroying instance, overlapped");
            //Destroy(candidate);
            return (false, null);
        }

        private void Align(Transform entry, Transform targetExit, StructureInstanceScript newInstance, StructureSocketScript attachSocket) {
            Debug.Log("Aligning instance to match target exit");
            SimpleAlignRotationMethod(entry, targetExit, newInstance);
            SimpleAlignPositionMethod(entry, targetExit, newInstance);
        } 
        
        private void SimpleAlignRotationMethod(Transform entry, Transform targetExit, StructureInstanceScript newInstance)
        {
            Transform root = newInstance.instance.transform;

            // Make the entry socket face the opposite direction of the target socket
            Quaternion rot = Quaternion.FromToRotation(entry.forward, -targetExit.forward);
            root.rotation = rot * root.rotation;
        }

        
        private void SimpleAlignPositionMethod(Transform entry, Transform targetExit, StructureInstanceScript newInstance) {
            // var bounds = newInstance.Bounds;
            // var instance = newInstance.instance;
            //
            // Vector3 offset = bounds.center - bounds.extents;
            // Vector3 forwardOffset = offset + attachSocket.transform.forward;
            // instance.transform.root.position = attachSocket.transform.root.position;
            // instance.transform.root.position += forwardOffset;
            
            Transform root = newInstance.instance.transform;

            // Recalculate entry position AFTER rotation
            Vector3 delta = targetExit.position - entry.position;

            // Move the entire instance so entry → target
            root.position += delta;
        }
    }
}
