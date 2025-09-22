using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public interface IGrabbableScript 
    {
        void Grab(Rigidbody handRb, Vector3 grabPoint);
        void Release(Rigidbody handRb);
    }
}
