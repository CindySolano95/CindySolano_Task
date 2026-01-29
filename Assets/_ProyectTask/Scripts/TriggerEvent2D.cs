using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent2D : MonoBehaviour
{
    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        OnEnter.Invoke();
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        OnExit.Invoke();
    }
}
