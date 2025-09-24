using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public interface IGrabbableScript {
        Transform GrabHandle { get; } // nullable
        void Grab(Rigidbody handRb, Vector3 worldPoint);
        void Release();
        bool IsItem { get; }
    }
}
