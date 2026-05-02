using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Satisfying UI tick on press — runs before button callbacks so audio plays even when the next frame loads a scene.
/// </summary>
public class EchoesUiClickHook : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        EchoesAudioDirector.PlayUiClick();
    }
}
