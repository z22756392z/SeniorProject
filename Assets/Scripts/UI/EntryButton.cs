using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;

public class EntryButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public LocalizeStringEvent _stringEvent = default;
    public TextMeshProUGUI _text = default;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _text.fontSize = 70;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _text.fontSize = 90;
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
