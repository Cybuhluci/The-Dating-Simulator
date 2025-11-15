using Luci;
using UnityEngine;

public class Ring : Entity
{
    public enum RingType { Normal, Super}
    public RingType ringType;

    public AudioClip normSound, superSound;
    
    public GameObject DefaultState, NormalRing, SuperRing;

    public void Awake()
    {
        if (ringType == RingType.Normal)
        {
            DefaultState.SetActive(false);
            NormalRing.SetActive(true);
            collectSound = normSound;
        }
        else if (ringType == RingType.Super)
        {
            DefaultState.SetActive(false);
            SuperRing.SetActive(true);
            collectSound = superSound;
        }
    }

    protected override void OnCollect(GameObject collector)
    {
        base.OnCollect(collector);

        if (ringType == RingType.Normal)
        {
            //MinaRingSystem.Instance.AddRing(1);
        }
        else if (ringType == RingType.Super)
        {
            //MinaRingSystem.Instance.AddRing(10);
        }
    }
}
