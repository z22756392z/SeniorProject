using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchButtonListener : MonoBehaviour
{
    public GameObject Object;
    public GameObject SwitchListenrObject;
    [SerializeField]
    private bool IsStart = false;
    
    public void SwitchDialogueTriggerListener()
    {
        if (!IsStart)
        {
            SwitchListenrObject.GetComponent<UI_DialogueTrigger>().TriggerDialogue();
            IsStart = true;
        }
        else
        {
            SwitchListenrObject.GetComponent<UI_DialogueTrigger>().TriggerDialogue();
            SwitchListenrObject.GetComponent<UI_DialogueTrigger>().NextDialogue();
        }
        
        Button btn = Object.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
        
        Debug.Log(btn);
    }

    private void OnClick()
    {
        SwitchListenrObject.GetComponent<UI_DialogueTrigger>().NextDialogue();
    }
}
