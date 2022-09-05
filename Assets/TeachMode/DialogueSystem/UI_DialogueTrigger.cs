using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DialogueTrigger : MonoBehaviour
{
	[SerializeField]
	protected int TriggerSentence = 99;
	public GameObject ChooseDialogue;
	public UI_Dialogue dialogue;

	protected int sentence = 0;  // Recorder sentence, is same to sentences[]'s index

	public void TriggerDialogue()
	{
		// 找到script，觸發裡面的函式
		FindObjectOfType<UI_DialogueManager>().StartDialogue(dialogue, sentence);
	}

	public virtual void NextDialogue()
	{
		sentence += 1;
		if (FindObjectOfType<UI_DialogueManager>().DisplayNextSentence())
		{
			sentence = 0;
			TriggerDialogue();  // repeat dialogue
		}

		if (TriggerSentence == sentence) ChooseDialogue.SetActive(true);
	}
}
