
using UnityEngine;

[CreateAssetMenu(fileName = "New Blip Profile", menuName = "Dialogue/Blip Profile")]
public class DialogueVoiceProfile : ScriptableObject
{
    public AudioClip[] blips;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    [Min(1)]
    public int charactersPerBlip = 2;
}
