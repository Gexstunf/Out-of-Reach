using UnityEngine;

namespace Environment.Scripts.DungeonGeneration.Utils {
    public class BoundsDrawerScript : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Color _defaultColor = Color.cyan;

        private bool _show;
        private Bounds _bounds;
        private int _id;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void SetBounds(Bounds newBounds, int id)
        {
            _bounds = newBounds;
            _id = id;
            _show = true;
        }

        // Update is called once per frame
        void OnDrawGizmos()
        {
            if (!_show) return;
            Gizmos.color = _defaultColor;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);
            if (_id % 2 == 0) 
                Gizmos.DrawCube(_bounds.center, Vector3.one);
            else 
                Gizmos.DrawSphere(_bounds.center, 0.5f);
        }
    }
}
