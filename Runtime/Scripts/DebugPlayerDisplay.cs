using UdonSharp;
using UnityEngine;

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
            _playerSync = BetterPlayerSync.Find();
            if(_playerSync.IsLocal) gameObject.SetActive(false);
        }

        public override void PostLateUpdate()
        {
            if (!_playerSync) return;
            
            syncCapsule.position = _playerSync.transform.position;
            syncCapsule.rotation = _playerSync.transform.rotation;

            avatarCapsule.position = _playerSync.Owner.GetPosition();
            avatarCapsule.rotation = _playerSync.Owner.GetRotation();
        }
    }
}
