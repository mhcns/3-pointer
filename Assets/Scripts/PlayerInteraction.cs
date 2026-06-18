using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private Transform grabbedBallReference;

    [SerializeField]
    private Image throwChargeImage;

    [SerializeField]
    private float chargeDuration = 1.5f;

    [SerializeField]
    private float minimumThrowForce = 5f;

    [SerializeField]
    private float maximumThrowForce = 15f;

    private InputAction interactAction;
    private InputAction attackAction;
    private BallActions nearbyBall;
    private BallActions grabbedBall;
    private float charge;

    public bool HasBall => grabbedBall != null;
    public event Action BallThrown;

    private void Awake()
    {
        interactAction = InputSystem.actions.FindAction("Player/Interact");
        attackAction = InputSystem.actions.FindAction("Player/Attack");

        if (throwChargeImage != null)
        {
            throwChargeImage.fillAmount = 0f;
            throwChargeImage.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        interactAction.Enable();
        attackAction.Enable();
    }

    private void OnDisable()
    {
        interactAction.Disable();
        attackAction.Disable();
    }

    private void Update()
    {
        if (grabbedBall == null)
        {
            TryGrabBall();
            return;
        }

        ChargeAndThrow();
    }

    public void SetNearbyBall(BallActions ball)
    {
        nearbyBall = ball;
    }

    public void ClearNearbyBall(BallActions ball)
    {
        if (nearbyBall == ball)
        {
            nearbyBall = null;
        }
    }

    public void ForceGrabBall(BallActions ball)
    {
        if (grabbedBall != null)
        {
            return;
        }

        nearbyBall = null;
        grabbedBall = ball;
        grabbedBall.Grab(grabbedBallReference);
    }

    private void TryGrabBall()
    {
        if (nearbyBall == null || !interactAction.WasPressedThisFrame()
        //|| !nearbyBall.PlayerIsAimingAtBall(playerCamera)
        )
        {
            return;
        }

        grabbedBall = nearbyBall;
        nearbyBall = null;
        grabbedBall.Grab(grabbedBallReference);
    }

    private void ChargeAndThrow()
    {
        if (attackAction.WasPressedThisFrame())
        {
            charge = 0f;
            SetChargeImageVisible(true);
        }

        if (attackAction.IsPressed())
        {
            charge = Mathf.Clamp01(charge + Time.deltaTime / Mathf.Max(chargeDuration, 0.01f));

            if (throwChargeImage != null)
            {
                throwChargeImage.fillAmount = charge;
            }
        }

        if (!attackAction.WasReleasedThisFrame())
        {
            return;
        }

        float throwForce = Mathf.Lerp(minimumThrowForce, maximumThrowForce, charge);
        grabbedBall.Throw(
            playerCamera.transform.forward + (Vector3.up * 0.3f),
            throwForce,
            transform.position
        );
        grabbedBall = null;
        charge = 0f;
        SetChargeImageVisible(false);
        BallThrown?.Invoke();
    }

    private void SetChargeImageVisible(bool isVisible)
    {
        if (throwChargeImage != null)
        {
            throwChargeImage.gameObject.SetActive(isVisible);
        }
    }
}
