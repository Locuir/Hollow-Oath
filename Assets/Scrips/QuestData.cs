using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questTitle;
    [TextArea(2, 3)]
    public string objective;
}