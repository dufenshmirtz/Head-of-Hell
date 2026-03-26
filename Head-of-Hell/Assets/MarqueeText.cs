using TMPro;
using UnityEngine;

public class MarqueeTMP : MonoBehaviour
{
    public float speed = 40f;
    public float startDelay = 1f;
    public float endPause = 0.75f;

    private RectTransform textRect;
    private RectTransform maskRect;
    private TMP_Text tmp;

    private float leftLimit;
    private float startX;
    private float timer;
    private bool shouldScroll;
    private bool waitingAtStart = true;
    private bool waitingAtEnd = false;

    void OnEnable()
    {
        textRect = GetComponent<RectTransform>();
        tmp = GetComponent<TMP_Text>();

        if (transform.parent == null)
        {
            shouldScroll = false;
            return;
        }

        maskRect = transform.parent.GetComponent<RectTransform>();

        Canvas.ForceUpdateCanvases();
        tmp.ForceMeshUpdate();

        float textWidth = tmp.preferredWidth;
        float maskWidth = maskRect.rect.width;

        startX = 0f;
        leftLimit = -(textWidth - maskWidth);

        if (textWidth <= maskWidth)
        {
            shouldScroll = false;
            textRect.anchoredPosition = new Vector2(startX, textRect.anchoredPosition.y);
            return;
        }

        shouldScroll = true;
        timer = 0f;
        waitingAtStart = true;
        waitingAtEnd = false;
        textRect.anchoredPosition = new Vector2(startX, textRect.anchoredPosition.y);
    }

    void Update()
    {
        if (!shouldScroll) return;

        if (waitingAtStart)
        {
            timer += Time.deltaTime;
            if (timer >= startDelay)
            {
                timer = 0f;
                waitingAtStart = false;
            }
            return;
        }

        if (waitingAtEnd)
        {
            timer += Time.deltaTime;
            if (timer >= endPause)
            {
                timer = 0f;
                waitingAtEnd = false;
                waitingAtStart = true;
                textRect.anchoredPosition = new Vector2(startX, textRect.anchoredPosition.y);
            }
            return;
        }

        Vector2 pos = textRect.anchoredPosition;
        pos.x -= speed * Time.deltaTime;

        if (pos.x <= leftLimit)
        {
            pos.x = leftLimit;
            waitingAtEnd = true;
            timer = 0f;
        }

        textRect.anchoredPosition = pos;
    }
}