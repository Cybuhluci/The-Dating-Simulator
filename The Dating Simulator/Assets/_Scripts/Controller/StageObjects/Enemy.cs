using Luci;
using UnityEngine;

public class Enemy : StageObject
{
    public enum EnemyType { Air, Ground, Water }

    [SerializeField] EnemyType enemyType;
    [SerializeField] float bounceHeight;
    [SerializeField] int EnemyScore;

    public float BounceHeight => bounceHeight;
    public EnemyType Type => enemyType;

    public void DestroySelf()
    {
        //plays destruction animation or whatever.
        if (enemyType != EnemyType.Air)
        {
            //SonicScoreSystem.Instance.GainScore(EnemyScore);
        }

        //SonicScoreSystem.Instance.EnemiesDestroyed += 1;
        Destroy(gameObject);
    }
}
