using System.Collections;  // 使用Queue
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DialogueManager : MonoBehaviour
{
	// UI
	//public Text nameText;
	public Text dialogueText;
	
	//public Animator animator; // 控制動畫效果(關閉對話、開啟對話)

	private Queue<string> sentences;  // 使用Queue讓對話新增到裡面(enqueue)

	// Use this for initialization
	void Start()
	{
		sentences = new Queue<string>();
	}

	public void StartDialogue(UI_Dialogue dialogue, int sentence) // 在DialogueTrigger.cs觸發
	{
		//animator.SetBool("IsOpen", true); // 控制UI動畫，開始對話

		//nameText.text = dialogue.name;

		sentences.Clear(); // 把對話先清除

		for(int i = sentence; i < dialogue.sentences.Length; i++)
        {
			sentences.Enqueue(dialogue.sentences[i]);
		}
		/*foreach (string sentence in dialogue.sentences) // 把對話丟到Queue裡面
		{
			sentences.Enqueue(sentence);
		}*/

		DisplayNextSentence();
	}

	public bool DisplayNextSentence()
	{
		if (sentences.Count == 0) // Queue裡面沒東西就結束對話(到了列隊的末尾)
		{
			EndDialogue();
			return true;
		}

		string sentence = sentences.Dequeue(); // 讓第一個句子離開Queue並存入sentence裡面
											   // dialogueText.text = sentence; UI顯示，下面可以讓UI逐字顯示
		StopAllCoroutines(); // 確保前一個動畫完成後才執行下一個動畫
		StartCoroutine(TypeSentence(sentence));
		return false;
	}

	IEnumerator TypeSentence(string sentence)  // 使用這個讓對話有打字效果
	{
		dialogueText.text = "";

		foreach (char letter in sentence.ToCharArray())
		{
			dialogueText.text += letter;
			yield return null; // 可以控制回傳時間，這裡不設置
							   // yield return new WaitForSeconds(0.02f);
		}
	}

	void EndDialogue()  // 控制UI動畫，結束對話，在DisplayNextSentence ()呼叫
	{
		//animator.SetBool("IsOpen", false);
	}

}
