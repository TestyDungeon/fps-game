using TMPro;
using UnityEngine;

public class PromptUI : MonoBehaviour
{
    [SerializeField] TMP_Text prompt;     // just under the crosshair, while looking at something
    [SerializeField] TMP_Text message;    // feedback line, disappears after a moment
    [SerializeField] float messageSeconds = 2f;

    float hideMessageAt;

    public void SetPrompt(string text)
    {
        bool has = !string.IsNullOrEmpty(text);
        prompt.gameObject.SetActive(has);
        if (has) prompt.text = text;
    }

    public void ShowMessage(string text)
    {
        message.text = text;
        message.gameObject.SetActive(true);
        hideMessageAt = Time.unscaledTime + messageSeconds;
    }

    void Update()
    {
        if (message.gameObject.activeSelf && Time.unscaledTime >= hideMessageAt)
            message.gameObject.SetActive(false);
    }
}