using UnityEngine;

public class BallActions : MonoBehaviour
{
    [SerializeField]
    private float moveToHandSpeed = 8f;

    [SerializeField]
    private PhysicsMaterial groundMaterial;

    [Header("Bounce Sound")]
    [SerializeField]
    private AudioClip bounceSound;

    [SerializeField]
    private float minimumImpactSpeed = 1f;

    [SerializeField]
    private float maximumImpactSpeed = 12f;

    [SerializeField]
    [Range(0f, 1f)]
    private float minimumBounceVolume = 0.1f;

    [SerializeField]
    private float bounceSoundCooldown = 0.08f;

    private Collider ballCollider;
    private Rigidbody ballRigidbody;
    private AudioSource audioSource;
    private Transform grabbedBallReference;
    private bool isGrabbed;
    private float lastBounceSoundTime;

    public bool HasValidThrow { get; private set; }
    public Vector3 ThrowOrigin { get; private set; }

    private void Awake()
    {
        ballCollider = GetComponent<Collider>();
        ballRigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
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

    public void Grab(Transform handReference)
    {
        ResetScoring();
        isGrabbed = true;
        grabbedBallReference = handReference;
        ballCollider.enabled = false;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.isKinematic = true;
        ballRigidbody.useGravity = false;
    }

    public void Throw(Vector3 direction, float force, Vector3 throwOrigin)
    {
        ThrowOrigin = throwOrigin;
        HasValidThrow = true;
        isGrabbed = false;
        grabbedBallReference = null;
        ballCollider.enabled = true;
        ballRigidbody.isKinematic = false;
        ballRigidbody.useGravity = true;
        ballRigidbody.AddForce(direction * force, ForceMode.Impulse);
        Vector3 torqueDirection = Vector3.Cross(Vector3.up, direction).normalized;
        ballRigidbody.AddTorque(torqueDirection * force * -0.05f, ForceMode.Impulse);
    }

    public void ConsumeThrow()
    {
        HasValidThrow = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.sharedMaterial == groundMaterial)
        {
            ResetScoring();
        }

        if (collision.collider.GetComponentInParent<PlayerInteraction>() == null)
        {
            PlayBounceSound(collision.relativeVelocity.magnitude);
        }
    }

    private void PlayBounceSound(float impactSpeed)
    {
        if (
            bounceSound == null
            || impactSpeed < minimumImpactSpeed
            || Time.time < lastBounceSoundTime + bounceSoundCooldown
        )
        {
            return;
        }

        float impactStrength = Mathf.InverseLerp(
            minimumImpactSpeed,
            maximumImpactSpeed,
            impactSpeed
        );
        float volume = Mathf.Lerp(minimumBounceVolume, 1f, impactStrength);

        audioSource.PlayOneShot(bounceSound, volume);
        lastBounceSoundTime = Time.time;
    }

    private void ResetScoring()
    {
        HasValidThrow = false;
        ScoreManager.Instance?.ResetAllHoops();
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
