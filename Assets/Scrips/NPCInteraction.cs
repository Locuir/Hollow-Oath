
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private GameObject interactionPrompt;

    private bool playerInRange;

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (dialogueManager == null || dialogueData == null)
            {
                Debug.LogError("DialogueManager or DialogueData is missing!");
                return;
            }

            dialogueManager.SetCurrentNPC(this);
            dialogueManager.StartDialogue(dialogueData);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") &&
            !other.transform.root.CompareTag("Player"))
            return;

        playerInRange = true;

        if (dialogueManager == null || !dialogueManager.IsDialogueActive())
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") &&
            !other.transform.root.CompareTag("Player"))
            return;

        playerInRange = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (interactionPrompt != null && playerInRange)
            interactionPrompt.SetActive(true);
    }
}
