
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogueData;

    public void StartConversation()
    {
        if (dialogueManager == null || dialogueData == null)
            return;

        dialogueManager.StartDialogue(dialogueData);
    }
}
