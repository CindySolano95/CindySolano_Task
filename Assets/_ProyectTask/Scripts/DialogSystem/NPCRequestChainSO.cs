using UnityEngine;

[CreateAssetMenu(menuName = "NPC/Request Chain", fileName = "NPCRequestChain_")]
public class NPCRequestChainSO : ScriptableObject
{
    [Header("NPC Identity")]
    public string npcId;   // Must be unique per NPC in the scene (e.g., "NPC_Woman")
    public string npcName; // Optional

    [Header("Starting Request")]
    public NPCRequestSO firstRequest;
}
