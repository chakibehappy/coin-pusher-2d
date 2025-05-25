using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    public float hudRefreshRate = 0.1f;

    private float timer;

    private void Update()
    {
        if (fpsText != null)
        {
            float current = 0f;
            current = (int)(1f / Time.unscaledDeltaTime);
            timer += Time.deltaTime;

            if (timer > hudRefreshRate)
            {
                fpsText.text = "FPS : " + current.ToString();
                timer = 0;
            }
        }
    }
}
