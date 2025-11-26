using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Ikeiwa.BetterPlayerSync.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BetterPlayerSync : UdonSharpBehaviour
    {
        [SerializeField] private int physFps = 30;
        [SerializeField] private bool autoActivate = false;
        [SerializeField] private bool disableOnRespawn = false;
    
        [UdonSynced] private Vector3 _playerPosition;
        [UdonSynced] private Quaternion _playerRotation;
        [UdonSynced] private Vector3 _playerVelocity;
        [UdonSynced] private float _playerGravity;
        [UdonSynced] private bool _enabled;

        private VRCPlayerApi _owner;
        private bool _isLocal;
        private VRCStation _station;
        private float _physDuration;
        private CharacterController _characterController;

        public VRCPlayerApi Owner => _owner;
        public bool IsLocal => _isLocal;
        public bool Enabled => _enabled;

        #region Unity Events

        void Start()
        {
            _station = GetComponent<VRCStation>();
            _characterController = GetComponent<CharacterController>();
            _owner = Networking.GetOwner(gameObject);

            if (_owner.isLocal)
            {
                _isLocal = true;
                _station.PlayerMobility = VRCStation.Mobility.Mobile;
                if(autoActivate)
                    SendCustomEventDelayedSeconds(nameof(_AutoActivate),2);
            }
        }
    
        private void FixedUpdate()
        {
            if (_isLocal || !_enabled) return;
        
            Vector3 desiredMove = _playerVelocity;

            float gravityContribution = _playerGravity * Time.fixedDeltaTime * Physics.gravity.y;
            if (_characterController.isGrounded)
                gravityContribution = 0;

            _playerVelocity = new Vector3(desiredMove.x, desiredMove.y + gravityContribution,
                desiredMove.z);

            _characterController.Move(_playerVelocity * Time.fixedDeltaTime);
        
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _playerRotation, Time.deltaTime*360);
        }

        #endregion

        #region VRC Events

        public override void OnDeserialization(DeserializationResult result)
        {
            double latency = result.receiveTime - result.sendTime;
            double updateTime = (Time.realtimeSinceStartup - result.sendTime)-latency;

            _characterController.enabled = false;
            transform.position = _playerPosition;
            _characterController.enabled = true;
            _characterController.Move(_playerVelocity * (float)updateTime);
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (disableOnRespawn)
                SetSyncState(false);
            else
                SendCustomEventDelayedFrames(nameof(_AutoActivate),5);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player != _owner) return;
            _enabled = false;
        }

        #endregion

        #region Local Methods

        public void _AutoActivate()
        {
            SetSyncState(true);
        }

        public void _PhysUpdate()
        {
            _playerPosition = _owner.GetPosition();
            _playerRotation = _owner.GetRotation();
            _playerVelocity = _owner.GetVelocity();
            _playerGravity = _owner.GetGravityStrength();
        
            transform.position = _playerPosition;
            RequestSerialization();
        
            if(_enabled)
                SendCustomEventDelayedSeconds(nameof(_PhysUpdate), _physDuration);
        }

        private void SetSyncState(bool activate)
        {
            if (!_isLocal) return;
        
            _enabled = activate;
            if (_enabled)
            {
                _physDuration = 1.0f/physFps;
                _PhysUpdate();
                _station.transform.position = _owner.GetPosition();
                _station.transform.rotation = _owner.GetRotation();
                _station.UseStation(_owner);
            }
            else
                _station.ExitStation(_owner);
            
            RequestSerialization();
        }

        #endregion

        #region public API

        public static void ActivateSync()
        {
            Find(Networking.LocalPlayer).SetSyncState(true);
        }

        public static void DeActivateSync()
        {
            Find(Networking.LocalPlayer).SetSyncState(false);
        }

        public void Teleport(Vector3 position, Quaternion rotation, VRC_SceneDescriptor.SpawnOrientation teleportOrientation = VRC_SceneDescriptor.SpawnOrientation.Default)
        {
            if (!_isLocal) return;
        
            _owner.TeleportTo(position, rotation, teleportOrientation, false);
            transform.position = position;
            transform.rotation = rotation;
            _station.UseStation(_owner);
        }
    
        public static BetterPlayerSync Find(VRCPlayerApi player = null)
        {
            if(player == null)
                player = Networking.LocalPlayer;
        
            if (!Utilities.IsValid(player))
                return null;
        
            var objects = Networking.GetPlayerObjects(player);
            for (int i = 0; i < objects.Length; i++)
            {
                if (!Utilities.IsValid(objects[i])) continue;
                BetterPlayerSync foundScript = objects[i].GetComponentInChildren<BetterPlayerSync>();
                if (Utilities.IsValid(foundScript)) return foundScript;
            }
            return null;
        }

        #endregion
    
    }
}
