
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueIcon;
    [SerializeField] private Button continueButton;

    [Header("Settings")]
    [SerializeField, Min(0f)] private float typingSpeed = 0.03f;
    [SerializeField] private Behaviour playerMovement;

    private bool wasMovementEnabled;
    private DialogueData currentDialogue;
    private Coroutine typingCoroutine;
    private int currentLineIndex;
    private bool isTyping;
    private NPCInteraction currentNPC;

    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueIcon != null)
            continueIcon.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(AdvanceDialogue);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(AdvanceDialogue);
    }

    private void Update()
    {
        if (currentDialogue == null)
            return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (continueButton != null &&
                UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            AdvanceDialogue();
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines == null ||
            dialogue.lines.Length == 0 || currentDialogue != null)
            return;

        if (currentNPC != null)
            currentNPC.HideInteractionPrompt();

        if (playerMovement != null)
        {
            wasMovementEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;

        dialoguePanel.SetActive(true);
        ShowLine();
    }

    public void AdvanceDialogue()
    {
        if (currentDialogue == null)
            return;

        if (isTyping)
        {
            StopTyping();
            dialogueText.text = currentDialogue.lines[currentLineIndex].text;

            if (continueIcon != null)
                continueIcon.SetActive(true);

            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void ShowLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];

        characterName.text = line.speakerName;
        dialogueText.text = string.Empty;

        characterImage.sprite = line.portrait;
        characterImage.enabled = line.portrait != null;

        if (continueIcon != null)
            continueIcon.SetActive(false);

        if (typingSpeed <= 0f)
        {
            dialogueText.text = line.text;

            if (continueIcon != null)
                continueIcon.SetActive(true);

            return;
        }

        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        foreach (char character in text)
        {
            dialogueText.text += character;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        if (continueIcon != null)
            continueIcon.SetActive(true);
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    public void EndDialogue()
    {
        StopTyping();

        currentDialogue = null;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueIcon != null)
            continueIcon.SetActive(false);

        if (playerMovement != null && wasMovementEnabled)
            playerMovement.enabled = true;

        if (currentNPC != null)
        {
            currentNPC.ShowInteractionPrompt();
            currentNPC = null;
        }
    }

    public bool IsDialogueActive()
    {
        return currentDialogue != null;
    }

    public void SetCurrentNPC(NPCInteraction npc)
    {
        currentNPC = npc;
    }
}
