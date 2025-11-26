using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Ikeiwa.BetterPlayerSync.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DebugPlayerDisplay : UdonSharpBehaviour
    {
        [SerializeField] private Transform syncCapsule;
        [SerializeField] private Transform avatarCapsule;
        
        private BetterPlayerSync _playerSync;
        
        void Start()
        {
            SendCustomEventDelayedSeconds(nameof(_StartDebug),1);
        }

        public void _StartDebug()
        {
            _playerSync = BetterPlayerSync.Find(Networking.GetOwner(gameObject));
            if(_playerSync.IsLocal) gameObject.SetActive(false);
        }

        public override void PostLateUpdate()
        {
            if (!_playerSync) return;
            
            syncCapsule.position = _playerSync.transform.position;
            syncCapsule.rotation = _playerSync.transform.rotation;
            syncCapsule.Translate(new Vector3(0,0.825f,0), Space.Self);

            avatarCapsule.position = _playerSync.Owner.GetPosition();
            avatarCapsule.rotation = _playerSync.Owner.GetRotation();
            avatarCapsule.Translate(new Vector3(0,0.825f,0), Space.Self);
        }
    }
}
