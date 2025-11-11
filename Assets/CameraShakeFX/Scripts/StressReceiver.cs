using UnityEngine;

namespace CameraShakeFX.Scripts {
    public class StressReceiver : MonoBehaviour 
    {
        private float _trauma;
        private Vector3 _lastPosOffset;
        private Vector3 _lastRotOffset;

        [Tooltip("Exponent for calculating the shake factor. Useful for creating different effect fade outs")]
        public float TraumaExponent = 1;

        [Tooltip("Maximum angular shake (Euler angles in degrees)")]
        public Vector3 MaximumAngularShake = Vector3.one * 5;

        [Tooltip("Maximum translation shake")]
        public Vector3 MaximumTranslationShake = Vector3.one * 0.75f;

        [Tooltip("Speed multiplier for Perlin noise evolution")]
        public float NoiseFrequency = 25f;

        private float _seed;

        private void Awake()
        {
            // Random seed ensures different shake patterns per object
            _seed = Random.value * 100f;
        }

        private void LateUpdate()
        {
            // Undo previous offsets first — makes this additive and camera-movement-safe
            transform.localPosition -= _lastPosOffset;
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles - _lastRotOffset);

            float shake = Mathf.Pow(_trauma, TraumaExponent);

            if (shake > 0)
            {
                // Compute new smooth offsets with Perlin noise
                _lastPosOffset = new Vector3(
                    (Mathf.PerlinNoise(_seed, Time.time * NoiseFrequency) * 2 - 1) * MaximumTranslationShake.x,
                    (Mathf.PerlinNoise(_seed + 1, Time.time * NoiseFrequency) * 2 - 1) * MaximumTranslationShake.y,
                    (Mathf.PerlinNoise(_seed + 2, Time.time * NoiseFrequency) * 2 - 1) * MaximumTranslationShake.z
                ) * shake;

                _lastRotOffset = new Vector3(
                    (Mathf.PerlinNoise(_seed + 3, Time.time * NoiseFrequency) * 2 - 1) * MaximumAngularShake.x,
                    (Mathf.PerlinNoise(_seed + 4, Time.time * NoiseFrequency) * 2 - 1) * MaximumAngularShake.y,
                    (Mathf.PerlinNoise(_seed + 5, Time.time * NoiseFrequency) * 2 - 1) * MaximumAngularShake.z
                ) * shake;

                // Apply the new offsets additively
                transform.localPosition += _lastPosOffset;
                transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + _lastRotOffset);

                // Gradually reduce trauma
                _trauma = Mathf.Clamp01(_trauma - Time.deltaTime);
            }
            else
            {
                // No shake — reset offsets to zero
                _lastPosOffset = Vector3.zero;
                _lastRotOffset = Vector3.zero;
            }
        }

        /// <summary>
        /// Adds stress (trauma) to the object, triggering a shake.
        /// </summary>
        /// <param name="stress">[0,1] intensity of the shake</param>
        public void InduceStress(float stress)
        {
            _trauma = Mathf.Clamp01(_trauma + stress);
        }
    }
}
