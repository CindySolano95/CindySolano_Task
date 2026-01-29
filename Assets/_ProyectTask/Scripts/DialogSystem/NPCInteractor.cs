using UnityEngine;

public class NPCInteractor : MonoBehaviour
{
    [Header("NPC Setup")]
    [SerializeField] private string npcId = "NPC_001";         // Unique per NPC
    [SerializeField] private NPCRequestChainSO requestChain;   // The chain configured for this NPC
    [SerializeField] private Sprite npcPortrait;               // Portrait shown in UI
    [SerializeField] private TalkPromptView talkPrompt;
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;  // Default key for the test

    private bool _playerInRange;

    // Called by TriggerEvent2D UnityEvents
    public void SetPlayerInRange(bool inRange)
    {
        _playerInRange = inRange;
    }

    private void Update()
    {
        if (!_playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            talkPrompt.Hide();
            NPCRequestManager.Instance.TalkToNPC(npcId, requestChain, npcPortrait);
        }
    }
}


