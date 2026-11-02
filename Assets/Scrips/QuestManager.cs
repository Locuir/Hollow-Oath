
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    [Header("Quests")]
    [SerializeField] private QuestData[] quests;

    [Header("Village Quest")]
    [SerializeField] private QuestNPC[] requiredNPCs;

    [Header("UI")]
    [SerializeField] private TMP_Text questTitleText;
    [SerializeField] private TMP_Text objectiveText;

    private int currentQuestIndex = -1;
    public int CurrentQuestIndex => currentQuestIndex;
    [SerializeField] private int startingQuestIndex;
    private HashSet<QuestNPC> talkedNPCs = new HashSet<QuestNPC>();

    private void Start()
    {
        StartQuest(startingQuestIndex);
    }

    public void StartQuest(int index)
    {
        if (quests == null || index < 0 || index >= quests.Length)
        {
            currentQuestIndex = -1;

            questTitleText.text = "";
            objectiveText.text = "All Quests Completed";

            return;
        }

        currentQuestIndex = index;

        QuestData quest = quests[currentQuestIndex];

        questTitleText.text = quest.questTitle;
        objectiveText.text = quest.objective;

        if (currentQuestIndex == 1)
            UpdateVillageQuestUI();

        Debug.Log($"Started Quest: {quest.questTitle}");
    }
    public void CompleteSoldiersQuest()
    {
        Debug.Log("Soldiers Quest Completion Event Called!");

        if (currentQuestIndex != 4)
        {
            Debug.LogWarning("Wrong Quest Index: " + currentQuestIndex);
            return;
        }

        CompleteCurrentQuest();
    }
    public void CompleteCurrentQuest()
    {
        if (currentQuestIndex < 0)
            return;

        StartQuest(currentQuestIndex + 1);
    }
    public void CompletePrisonerQuest()
    {
        Debug.Log("Prisoner Quest Completion Event Called!");

        if (currentQuestIndex != 3)
        {
            Debug.LogWarning("Wrong Quest Index: " + currentQuestIndex);
            return;
        }

        CompleteCurrentQuest();
    }
    public void CompleteButcherQuest()
    {
        Debug.Log("Butcher Quest Completion Event Called!");

        if (currentQuestIndex != 0)
        {
            Debug.LogWarning("Wrong Quest Index: " + currentQuestIndex);
            return;
        }

        CompleteCurrentQuest();
    }
    public void CompleteCampQuest()
    {
        if (currentQuestIndex != 2)
            return;

        Debug.Log("Quest Completed: Find the King's Camp");

        CompleteCurrentQuest();
    }
    public void RegisterNPCConversation(QuestNPC npc)
    {
        Debug.Log("Registering NPC: " + npc.name);

        if (currentQuestIndex != 1)
        {
            Debug.Log("Village Quest is not active!");
            return;
        }

        if (requiredNPCs == null ||
            !System.Array.Exists(requiredNPCs, x => x == npc))
        {
            Debug.LogWarning(npc.name + " is not in Required NPCs!");
            return;
        }

        if (!talkedNPCs.Add(npc))
        {
            Debug.Log("NPC already counted: " + npc.name);
            return;
        }

        Debug.Log("Village Progress: " + talkedNPCs.Count + "/" + requiredNPCs.Length);

        UpdateVillageQuestUI();

        if (talkedNPCs.Count >= requiredNPCs.Length)
            CompleteCurrentQuest();
    }
    private void UpdateVillageQuestUI()
    {
        objectiveText.text =
            $"{quests[currentQuestIndex].objective} ({talkedNPCs.Count}/{requiredNPCs.Length})";
    }
}
