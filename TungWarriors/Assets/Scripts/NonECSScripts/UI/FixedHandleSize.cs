using UnityEngine;
using UnityEngine.UI;

public class FixedHandleSize : MonoBehaviour
{
    public ScrollRect scrollRect;
    [Tooltip("Держите значение чуть больше 0 (например, 0.1), чтобы ползунок можно было нажать!")]
    [Range(0.01f, 1f)]
    public float fixedSize = 0.1f;

    void OnEnable()
    {
        Canvas.willRenderCanvases += EnforceScrollbarSize;
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= EnforceScrollbarSize;
    }

    private void EnforceScrollbarSize()
    {
        if (scrollRect.verticalScrollbar && scrollRect.verticalScrollbar.size != fixedSize)
        {
            scrollRect.verticalScrollbar.size = fixedSize;
        }

        if (scrollRect.horizontalScrollbar && scrollRect.horizontalScrollbar.size != fixedSize)
        {
            scrollRect.horizontalScrollbar.size = fixedSize;
        }
    }
}
