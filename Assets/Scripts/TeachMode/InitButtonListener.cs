using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitButtonListener : MonoBehaviour
{
    public GameObject InitDialogue;

    void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        InitDialogue.GetComponent<UI_DialogueTrigger>().NextDialogue();
    }
}
