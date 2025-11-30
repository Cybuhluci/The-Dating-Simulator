using UnityEngine;
using Luci;

public class TenRing : Entity
{
    protected override void OnCollect(GameObject collector)
    {
        base.OnCollect(collector);

        //MinaRingSystem.Instance.AddRing(10);
    }
}
