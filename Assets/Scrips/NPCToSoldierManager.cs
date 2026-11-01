
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NPCToSoldierManager : MonoBehaviour
{
    [Header("NPCs")]
    [SerializeField] private GameObject[] npcs;

    [Header("Soldier")]
    [SerializeField] private GameObject soldierPrefab;

    [Header("Events")]
    [SerializeField] private UnityEvent onTransformationComplete;
    [Header("References")]
    [SerializeField] private Transform player;

    private bool hasTransformed;

    private class SpawnData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Transform parent;
        public Vector3 scale;
    }
    private void AssignPlayerReference(GameObject soldier)
    {
        MonoBehaviour[] components =
            soldier.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour component in components)
        {
            if (component is IPlayerTarget targetReceiver)
            {
                targetReceiver.SetPlayerTarget(player);
            }
        }
    }
    public void TransformNPCsToSoldiers()
    {
        if (hasTransformed)
            return;

        if (soldierPrefab == null)
        {
            Debug.LogError("Soldier Prefab is missing!");
            return;
        }

        hasTransformed = true;

        List<SpawnData> spawnPoints = new List<SpawnData>();

        foreach (GameObject npc in npcs)
        {
            if (npc == null)
                continue;

            spawnPoints.Add(new SpawnData
            {
                position = npc.transform.position,
                rotation = npc.transform.rotation,
                parent = npc.transform.parent,
                scale = npc.transform.localScale
            });

            npc.SetActive(false);
        }

        List<GameObject> soldiers = new List<GameObject>();

        GameObject stagingObject = new GameObject("SoldierStaging");
        stagingObject.SetActive(false);

        foreach (SpawnData data in spawnPoints)
        {

            GameObject soldier = Instantiate(soldierPrefab,data.position,data.rotation,stagingObject.transform);
            AssignPlayerReference(soldier);
            soldier.SetActive(false);

            soldier.transform.SetParent(data.parent, true);

            Vector3 scale = soldier.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(data.scale.x);
            soldier.transform.localScale = scale;

            soldiers.Add(soldier);
        }

        Destroy(stagingObject);

        foreach (GameObject soldier in soldiers)
        {
            soldier.SetActive(true);
        }

        foreach (GameObject npc in npcs)
        {
            if (npc != null)
                Destroy(npc);
        }

        onTransformationComplete?.Invoke();

        Debug.Log("All NPCs have been transformed into soldiers!");
    }
}
