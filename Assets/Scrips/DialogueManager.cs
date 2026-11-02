
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueIcon;
    [SerializeField] private Button continueButton;

    [Header("Blip Audio")]
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private DialogueVoiceProfile blipProfile;

    [Header("Settings")]
    [SerializeField, Min(0f)] private float typingSpeed = 0.03f;
    [SerializeField] private Behaviour playerMovement;

    [Header("Dialogue Events")]
    [SerializeField] private DialogueData finalSoldierDialogue;
    [SerializeField] private UnityEvent onFinalSoldierDialogueCompleted;
    [SerializeField] private DialogueData butcherDialogue;
    [SerializeField] private UnityEvent onButcherDialogueCompleted;
    [SerializeField] private DialogueData prisonerDialogue;
    [SerializeField] private UnityEvent onPrisonerDialogueCompleted;

    private bool wasMovementEnabled;
    private DialogueData currentDialogue;
    private Coroutine typingCoroutine;
    private int currentLineIndex;
    private bool isTyping;
    private NPCInteraction currentNPC;
    private int characterCounter;

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

        characterCounter = 0;

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

            PlayBlip(character);

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        if (continueIcon != null)
            continueIcon.SetActive(true);
    }

    private void PlayBlip(char character)
    {
        if (blipProfile == null || voiceAudioSource == null)
            return;

        if (blipProfile.blips == null ||
            blipProfile.blips.Length == 0)
            return;

        if (!char.IsLetterOrDigit(character))
            return;

        characterCounter++;

        if (characterCounter < blipProfile.charactersPerBlip)
            return;

        characterCounter = 0;

        AudioClip clip = blipProfile.blips[
            Random.Range(0, blipProfile.blips.Length)
        ];

        if (clip == null)
            return;

        voiceAudioSource.pitch = Random.Range(
            Mathf.Min(blipProfile.pitchMin, blipProfile.pitchMax),
            Mathf.Max(blipProfile.pitchMin, blipProfile.pitchMax)
        );

        voiceAudioSource.PlayOneShot(clip, blipProfile.volume);
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

        DialogueData completedDialogue = currentDialogue;

        currentDialogue = null;
        characterCounter = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueIcon != null)
            continueIcon.SetActive(false);

        if (playerMovement != null && wasMovementEnabled)
            playerMovement.enabled = true;
        if (currentNPC != null)
        {
            QuestNPC questNPC = currentNPC.GetComponent<QuestNPC>();

            if (questNPC != null)
            {
                Debug.Log("NPC Dialogue Completed: " + currentNPC.name);
                questNPC.OnDialogueCompleted();
            }
            else
            {
                Debug.LogWarning("QuestNPC component not found on: " + currentNPC.name);
            }

            currentNPC.ShowInteractionPrompt();
            currentNPC = null;
        }
        if (completedDialogue == finalSoldierDialogue)
        {
            onFinalSoldierDialogueCompleted?.Invoke();
        }
        if (completedDialogue == butcherDialogue)
        {
            Debug.Log("Butcher Dialogue Completed!");
            onButcherDialogueCompleted?.Invoke();
        }
        else
        {
            Debug.LogWarning("Completed dialogue is not the assigned Butcher Dialogue!");
        }
        if (completedDialogue == prisonerDialogue)
        {
            onPrisonerDialogueCompleted?.Invoke();
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
