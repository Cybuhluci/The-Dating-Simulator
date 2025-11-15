using Luci;
using UnityEngine;

namespace RagdollEngine
{
    [RequireComponent(typeof(BalloonEntity))]
    public class BalloonHomingTarget : HomingTarget
    {
        [SerializeField] float bounciness;

        BalloonEntity balloonEntity;

        void Awake()
        {
            balloonEntity = GetComponent<BalloonEntity>();
        }

        //public override void OnTarget(PlayerBehaviourTree playerBehaviourTree)
        //{
        //    playerBehaviourTree.RB.linearVelocity = new Vector3(playerBehaviourTree.RB.linearVelocity.x,
        //        bounciness,
        //        playerBehaviourTree.RB.linearVelocity.z);

        //    balloonEntity.Break(playerBehaviourTree);
        //}
    }
}
