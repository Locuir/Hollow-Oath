
using UnityEngine;

public class QuestNPC : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;

    public void OnDialogueCompleted()
    {
        if (questManager != null)
            questManager.RegisterNPCConversation(this);
    }
}
