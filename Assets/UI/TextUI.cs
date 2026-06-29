using UnityEngine;
using TMPro;

public class TextUI : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(tmp.alpha != 0)
            tmp.alpha = Mathf.MoveTowards(tmp.alpha, 0, 0.3f * Time.fixedDeltaTime);
    }

    public void SetText(string text_)
    {
        tmp.text = text_;
        tmp.alpha = 1;
    } 
}
