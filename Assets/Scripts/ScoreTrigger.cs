using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScoreTrigger : MonoBehaviour
{
    public enum TriggerZone
    {
        Top,
        Middle,
        Bottom,
    }

    [SerializeField]
    private TriggerZone zone;

    private Collider triggerCollider;

    public TriggerZone Zone => zone;
    public Transform Hoop => transform.parent;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        BallActions ball = other.GetComponent<BallActions>();

        if (ball != null)
        {
            ScoreManager.Instance.TriggerEntered(this, ball);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BallActions ball = other.GetComponent<BallActions>();

        if (ball != null)
        {
            ScoreManager.Instance.TriggerExited(this, ball);
        }
    }

    public void SetTriggerEnabled(bool isEnabled)
    {
        triggerCollider.enabled = isEnabled;
    }
}
