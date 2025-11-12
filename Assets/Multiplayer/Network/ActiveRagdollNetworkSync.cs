using Photon.Pun;
using UnityEngine;

namespace Characters.ActiveRagdollSystem.Network
{
    [RequireComponent(typeof(ActiveRagdollCoreScript), typeof(PhotonView))]
    public class ActiveRagdollNetworkSync : MonoBehaviourPun, IPunObservable
    {
        private ActiveRagdollCoreScript _core;
        private Quaternion[] _ghostRotations;
        private Vector3[] _ghostPositions;

        private void Awake()
        {
            _core = GetComponent<ActiveRagdollCoreScript>();
            _ghostRotations = new Quaternion[_core.boneMaps.Length];
            _ghostPositions = new Vector3[_core.boneMaps.Length];
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                for (int i = 0; i < _core.boneMaps.Length; i++)
                {
                    var bone = _core.boneMaps[i];
                    if (!bone.ghostBone) continue;

                    stream.SendNext(bone.ghostBone.localPosition);
                    stream.SendNext(bone.ghostBone.localRotation);
                }
            }
            else
            {
                for (int i = 0; i < _core.boneMaps.Length; i++)
                {
                    _ghostPositions[i] = (Vector3)stream.ReceiveNext();
                    _ghostRotations[i] = (Quaternion)stream.ReceiveNext();
                }
            }
        }

        private void LateUpdate()
        {
            if (photonView.IsMine) return;

            // Interpolar las posiciones/rotaciones ghost recibidas
            for (int i = 0; i < _core.boneMaps.Length; i++)
            {
                var bone = _core.boneMaps[i];
                if (!bone.ghostBone) continue;

                bone.ghostBone.localPosition = Vector3.Lerp(
                    bone.ghostBone.localPosition,
                    _ghostPositions[i],
                    Time.deltaTime * 15f
                );

                bone.ghostBone.localRotation = Quaternion.Slerp(
                    bone.ghostBone.localRotation,
                    _ghostRotations[i],
                    Time.deltaTime * 15f
                );
            }
        }
    }
}
