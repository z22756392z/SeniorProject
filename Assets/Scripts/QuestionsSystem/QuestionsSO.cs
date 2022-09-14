using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "QuestionsSO", menuName = "Question/QuestionsSO")]
public class QuestionsSO : ScriptableObject
{
	[SerializeField] private QuestionsGroup[] _questionGroups;

	[SerializeField] private DialogueDataSO _winDialogue;
	[SerializeField] private DialogueDataSO _loseDialogue;

	[Header("Boardcasting on channels")]
	[SerializeField] private DialogueDataChannelSO _startDialogueEvent = default;
	[SerializeField] private VoidEventChannelSO _EndOfChoiceDialogue = default;

	[SerializeField] private ListLocalizedStringEventChannelSO _showAcpunturePoints = default;

	[SerializeField] private VoidEventChannelSO _questionAnswered = default;
	[SerializeField] private VoidEventChannelSO _questionFinish = default;
	[Header("Listening to channels")]
    [SerializeField] private VoidEventChannelSO _winDialogueEvent = default;
    [SerializeField] private VoidEventChannelSO _loseDialogueEvent = default;

	private QuestionsGroup _currentQuestionGroup;
    private DialogueDataSO _currentDialogue;

	public int CurrentQusetionGroupCount => _currentQuestionGroup.questions.Length;

    public void OnEnable()
    {
		_questionFinish.OnEventRaised += Reset;
    }

	public void OnDisable()
    {
		_questionFinish.OnEventRaised -= Reset;
    }

    public void SetCurrentQuestoinGroup(int value)
    {
		if(_questionGroups.Length - 1 >= value && value >= 0)
			_currentQuestionGroup = _questionGroups[value];
    }

    public void PlayDefaultQuest()
	{
		if (_currentDialogue == null) _currentQuestionGroup = _questionGroups[0];
		_currentDialogue = _currentQuestionGroup.GetQusetion();
		_winDialogueEvent.OnEventRaised += PlayWinDialogue;
		_loseDialogueEvent.OnEventRaised += PlayLoseDialogue;

		StartDialogue();
	}

    void StartDialogue()
    {
        _startDialogueEvent.RaiseEvent(_currentDialogue);
		List<Choice> choices = _currentDialogue.Lines[_currentDialogue.Lines.Count - 1].Choices;
		if (choices == null) return;
		List<LocalizedString> choiceContents = new List<LocalizedString>() ;
        foreach (var choice in choices)
        {
			choiceContents.Add(choice.Response);
        }
		_showAcpunturePoints.RaiseEvent(choiceContents);
	}

	void PlayWinDialogue()
	{
		if (_winDialogue != null)
		{
			_currentDialogue = _winDialogue;
			StartDialogue();
			_EndOfChoiceDialogue.OnEventRaised += EndChoiceDialogue;
		}
	}

	void PlayLoseDialogue()
	{
		if (_loseDialogue != null)
		{
			_currentDialogue = _loseDialogue;
			StartDialogue();
			_EndOfChoiceDialogue.OnEventRaised += EndChoiceDialogue;
		}
	}

	void EndChoiceDialogue()
	{
		_EndOfChoiceDialogue.OnEventRaised -= EndChoiceDialogue;
		_questionAnswered.RaiseEvent();
	}

	public void NextQuestion()
    {
		_currentDialogue = _currentQuestionGroup.GetQusetion();
		StartDialogue();
	}

	void Reset()
    {
		_winDialogueEvent.OnEventRaised -= PlayWinDialogue;
		_loseDialogueEvent.OnEventRaised -= PlayLoseDialogue;
	}
}

[Serializable]
public class QuestionsGroup
{
	[TextArea] public string description;
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

