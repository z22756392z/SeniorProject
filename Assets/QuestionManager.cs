using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private QuestionsGroup _questionsGroup;

	[SerializeField] private DialogueDataSO _winDialogue;
	[SerializeField] private DialogueDataSO _loseDialogue;

	[SerializeField] private DialogueDataChannelSO _startDialogueEvent = default;

    [Header("Listening to channels")]
    [SerializeField] private VoidEventChannelSO _winDialogueEvent = default;
    [SerializeField] private VoidEventChannelSO _loseDialogueEvent = default;
    [SerializeField] private IntEventChannelSO _endDialogueEvent = default;
	[SerializeField] private VoidEventChannelSO _endOfANSEvent = default;

    private DialogueDataSO _currentDialogue;
	public void PlayDefaultQuest()
	{
		_currentDialogue = _questionsGroup.GetQusetion();
		StartDialogue();
	}

    void StartDialogue()
    {
        _startDialogueEvent.RaiseEvent(_currentDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
        _winDialogueEvent.OnEventRaised += PlayWinDialogue;
        _loseDialogueEvent.OnEventRaised += PlayLoseDialogue;
    }

	void EndDialogue(int dialogueType)
	{
		_endDialogueEvent.OnEventRaised -= EndDialogue;
		_winDialogueEvent.OnEventRaised -= PlayWinDialogue;
		_loseDialogueEvent.OnEventRaised -= PlayLoseDialogue;
	}

	void PlayLoseDialogue()
	{
		if (_loseDialogue != null)
		{
			_currentDialogue = _loseDialogue;
			StartDialogue();
			_endOfANSEvent.OnEventRaised += NextQuestion;
		}
	}

	void PlayWinDialogue()
	{
		if (_winDialogue != null)
		{
			_currentDialogue = _winDialogue;
			StartDialogue();
			_endOfANSEvent.OnEventRaised += NextQuestion;
		}
	}

	void NextQuestion()
    {
		_currentDialogue = _questionsGroup.GetQusetion();
		StartDialogue();
		_endOfANSEvent.OnEventRaised -= NextQuestion;
	}
}

[Serializable]
public class QuestionsGroup
{
	public SequenceMode sequenceMode = SequenceMode.RandomNoImmediateRepeat;
	public DialogueDataSO[] questions;

	private int _nextQuestionToPlay = -1;
	private int _lastQuestionPlayed = -1;

	/// <summary>
	/// Chooses the next clip in the sequence, either following the order or randomly.
	/// </summary>
	/// <returns>A reference to an AudioClip</returns>
	public DialogueDataSO GetQusetion()
	{
		// Fast out if there is only one clip to play
		if (questions.Length == 1)
			return questions[0];

		if (_nextQuestionToPlay == -1)
		{
			// Index needs to be initialised: 0 if Sequential, random if otherwise
			_nextQuestionToPlay = (sequenceMode == SequenceMode.Sequential) ? 0 : UnityEngine.Random.Range(0, questions.Length);
		}
		else
		{
			// Select next clip index based on the appropriate SequenceMode
			switch (sequenceMode)
			{
				case SequenceMode.Random:
					_nextQuestionToPlay = UnityEngine.Random.Range(0, questions.Length);
					break;

				case SequenceMode.RandomNoImmediateRepeat:
					do
					{
						_nextQuestionToPlay = UnityEngine.Random.Range(0, questions.Length);
					} while (_nextQuestionToPlay == _lastQuestionPlayed);
					break;

				case SequenceMode.Sequential:
					_nextQuestionToPlay = (int)Mathf.Repeat(++_nextQuestionToPlay, questions.Length);
					break;
			}
		}

		_lastQuestionPlayed = _nextQuestionToPlay;

		return questions[_nextQuestionToPlay];
	}

	public enum SequenceMode
	{
		Random,
		RandomNoImmediateRepeat,
		Sequential,
	}
}

