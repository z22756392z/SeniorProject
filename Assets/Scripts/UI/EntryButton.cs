using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;

public class EntryButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public LocalizeStringEvent _stringEvent = default;
    public TextMeshProUGUI _text = default;

    private float fontSize;

    private void Awake()
    {
        fontSize = _text.fontSize;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _text.fontSize = fontSize - 20;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _text.fontSize = fontSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _stringEvent.StringReference.TableEntryReference = "OnHoverStart";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _stringEvent.StringReference.TableEntryReference = "Start";
    }
}
