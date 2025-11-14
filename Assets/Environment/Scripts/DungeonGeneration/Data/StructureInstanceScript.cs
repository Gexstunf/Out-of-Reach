using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Environment.Scripts.DungeonGeneration.Data {
    public class StructureInstanceScript {
        public GameObject instance;
        public StructurePrefabScript definition;
        public Bounds Bounds { get; private set; }
        public Bounds ShrunkBounds { get; private set; }
        private List<StructureSocketScript> _sockets = new();

        public StructureInstanceScript(GameObject gameObj, StructurePrefabScript def) {
            instance = gameObj;
            definition = def;
            _sockets = gameObj.GetComponentsInChildren<StructureSocketScript>().ToList();
            UpdateBounds();
        }
        
        public List<StructureSocketScript> GetEntries() => _sockets.Where(s => s.SocketType == StructureSocketScript.StructureSocketType.Entry && !s.IsConnected).ToList();
        public List<StructureSocketScript> GetExits() => _sockets.Where(s => s.SocketType == StructureSocketScript.StructureSocketType.Exit && !s.IsConnected).ToList();
        public List<StructureSocketScript> GetUnconnectedExits() => GetExits();

        public void UpdateBounds(float shrinkAmount = 0) {
            Bounds = CalculateBounds();
            ShrunkBounds = Bounds;
            if (shrinkAmount != 0) ShrunkBounds = ShrinkBounds(shrinkAmount);
        }

        private Bounds CalculateBounds() {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) 
                return new Bounds(instance.transform.position, Vector3.zero);
            
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers.Skip(1)) 
                b.Encapsulate(r.bounds);
            return b;
        }

        private Bounds ShrinkBounds(float amount) {
            var b = Bounds;
            b.Expand(-amount);
            return b;
        }
    }
}
