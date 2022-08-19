using UnityEngine;
using UnityEngine.Localization;

public enum ActorID
{
	D,//default ?? currently don't know
	U,//User ?? same currently don't know user name
}

/// <summary>
/// Scriptable Object that represents an "Actor", that is the protagonist of a Dialogue
/// </summary>
[CreateAssetMenu(fileName = "newActor", menuName = "Dialogues/Actor")]
public class ActorSO : ScriptableObject
{
	[SerializeField] private ActorID _actorId = default;
	[SerializeField] private LocalizedString _actorName = default;

	public ActorID ActorId { get => _actorId; }
	public LocalizedString ActorName { get => _actorName; }
}
