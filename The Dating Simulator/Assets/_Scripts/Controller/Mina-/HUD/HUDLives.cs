using Luci;
using TMPro;
using UnityEngine;

public class HUDLives : MonoBehaviour
{
    [SerializeField] private TMP_Text lifeText;
    MinaLifeSystem lifeSystem => MinaLifeSystem.Instance;

    private Color normalColor = new Color(0.094f, 0.094f, 0.094f); // RGB 24,24,24 -> #181818
    private Color dangerColor = Color.red;

    void Update()
    {
        lifeText.text = MinaAttributes.Instance.lives.ToString();

        if (MinaAttributes.Instance.lives <= 1)
        {
            // Flash red
            float alpha = Mathf.PingPong(Time.time * 4f, 1f); // speed 4f, adjust if needed
            Color flashingColor = new Color(dangerColor.r, dangerColor.g, dangerColor.b, alpha);
            lifeText.color = flashingColor;
        }
        else
        {
            // Reset to normal color
            lifeText.color = normalColor;
        }
    }
}
