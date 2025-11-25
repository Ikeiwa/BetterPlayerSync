using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Ikeiwa.BetterPlayerSync.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BetterSyncTeleporter : UdonSharpBehaviour
    {
        [SerializeField] private Transform target;
    
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if(!player.isLocal) return;
        
            BetterPlayerSync.Find(player).Teleport(target.position, target.rotation);
        }
    }
}
