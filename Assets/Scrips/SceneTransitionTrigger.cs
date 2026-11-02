
using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private string targetSceneName;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player"))
            return;

        if (questManager.CurrentQuestIndex != 2)
            return;

        triggered = true;

        questManager.CompleteCampQuest();
        sceneTransition.LoadScene(targetSceneName);
    }
}
