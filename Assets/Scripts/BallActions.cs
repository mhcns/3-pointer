using UnityEngine;

public class BallActions : MonoBehaviour
{
    [SerializeField]
    private float moveToHandSpeed = 8f;

    [SerializeField]
    private float interactionDistance = 3f;

    private Collider ballCollider;
    private Rigidbody ballRigidbody;
    private Transform grabbedBallReference;
    private bool isGrabbed;

    private void Awake()
    {
        ballCollider = GetComponent<Collider>();
        ballRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isGrabbed && grabbedBallReference != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                grabbedBallReference.position,
                moveToHandSpeed * Time.deltaTime
            );
        }
        else if (isGrabbed)
        {
            isGrabbed = false;
        }
    }

    public bool PlayerIsAimingAtBall(Camera playerCamera)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            return hit.collider.transform == transform;
        }

        return false;
    }

    public void Grab(Transform handReference)
    {
        isGrabbed = true;
        grabbedBallReference = handReference;
        ballCollider.enabled = false;
        ballRigidbody.isKinematic = true;
        ballRigidbody.useGravity = false;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
    }

    public void Throw(Vector3 direction, float force)
    {
        isGrabbed = false;
        grabbedBallReference = null;
        ballCollider.enabled = true;
        ballRigidbody.isKinematic = false;
        ballRigidbody.useGravity = true;
        ballRigidbody.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        if (playerInteraction == null)
        {
            return;
        }

        playerInteraction.SetNearbyBall(this);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        if (isGrabbed || playerInteraction == null)
        {
            return;
        }

        playerInteraction.ClearNearbyBall(this);
    }
}
