using Luci;
using UnityEngine;

namespace RagdollEngine
{
    public class BoostCapsuleHomingTarget : HomingTarget
    {
        [SerializeField] float bounciness;

        //public override void OnTarget(PlayerBehaviourTree playerBehaviourTree)
        //{
        //    playerBehaviourTree.RB.linearVelocity = new Vector3(playerBehaviourTree.RB.linearVelocity.x,
        //        bounciness,
        //        playerBehaviourTree.RB.linearVelocity.z);
        //}
    }
}
