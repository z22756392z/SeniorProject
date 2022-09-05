using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TriggerAnimator : UI_DialogueTrigger
{
	public GameObject Character;

	// 人工調整，下面可以使用scriptable存
	public int saluteSentence = 4;  
	public int endDialogue = 5;
	bool restart = false;

	public override void NextDialogue()
	{
		base.NextDialogue();

		Animator animator = Character.GetComponent<Animator>();
		if (sentence == saluteSentence) animator.SetTrigger("salute");
		if (sentence == endDialogue)
		{
			animator.SetTrigger("endDialogue");
			restart = true;
		}
		if (sentence == 0 && restart) animator.SetTrigger("restart");
	}
}
