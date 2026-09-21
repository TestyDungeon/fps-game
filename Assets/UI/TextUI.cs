using UnityEngine;
using TMPro;

public class TextUI : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private float lastTime = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(tmp.alpha != 0 && (Time.time - lastTime) > 2)
            tmp.alpha = Mathf.MoveTowards(tmp.alpha, 0, 1f * Time.fixedDeltaTime);
    }

    public void SetText(string text_)
    {
        tmp.text = text_;
        tmp.alpha = 1;
        lastTime = Time.time;
    } 

    public void SetSize(float size)
    {
        tmp.fontSize = size;
    } 
}
