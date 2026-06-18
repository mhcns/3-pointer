using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TwentyOneMinigame : MonoBehaviour
{
    private const string RecordKey = "TwentyOneRecord";

    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private BallActions ball;
    [SerializeField] private Transform startReference;
    [SerializeField] private Transform oppositeHoop;
    [SerializeField] private TMP_Text shootCountText;
    [SerializeField] private TMP_Text recordText;

    private InputAction startMinigameAction;
    private bool isActive;
    private bool hideShootCountAfterNextThrow;
    private int shootCount;

    private void Awake()
    {
        startMinigameAction = InputSystem.actions.FindAction("Player/Start Minigame");
        LoadRecord();
        shootCountText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        startMinigameAction.Enable();
        playerInteraction.BallThrown += OnBallThrown;
    }

    private void Start()
    {
        ScoreManager.Instance.ScoreChanged += OnScoreChanged;
    }

    private void OnDisable()
    {
        startMinigameAction.Disable();
        playerInteraction.BallThrown -= OnBallThrown;
        characterMovement.MovementLocked = false;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged -= OnScoreChanged;
        }
    }

    private void Update()
    {
        if (startMinigameAction.WasPressedThisFrame())
        {
            StartMinigame();
        }

        characterMovement.MovementLocked = isActive && playerInteraction.HasBall;
    }

    private void StartMinigame()
    {
        if (isActive)
        {
            return;
        }

        isActive = true;
        hideShootCountAfterNextThrow = false;
        shootCount = 0;

        ScoreManager.Instance.ResetScore();
        UpdateShootCountText();
        shootCountText.gameObject.SetActive(true);
        oppositeHoop.gameObject.SetActive(false);

        characterMovement.Teleport(startReference);
        playerInteraction.ForceGrabBall(ball);
        characterMovement.MovementLocked = true;
    }

    private void OnBallThrown()
    {
        if (isActive)
        {
            shootCount++;
            UpdateShootCountText();
            return;
        }

        if (hideShootCountAfterNextThrow)
        {
            hideShootCountAfterNextThrow = false;
            shootCountText.gameObject.SetActive(false);
        }
    }

    private void OnScoreChanged(int currentScore)
    {
        if (isActive && currentScore >= 21)
        {
            FinishMinigame();
        }
    }

    private void FinishMinigame()
    {
        isActive = false;
        hideShootCountAfterNextThrow = true;
        characterMovement.MovementLocked = false;
        oppositeHoop.gameObject.SetActive(true);

        if (!PlayerPrefs.HasKey(RecordKey) || shootCount < PlayerPrefs.GetInt(RecordKey))
        {
            PlayerPrefs.SetInt(RecordKey, shootCount);
            PlayerPrefs.Save();
        }

        ShowRecord(PlayerPrefs.GetInt(RecordKey));
    }

    private void LoadRecord()
    {
        if (!PlayerPrefs.HasKey(RecordKey))
        {
            recordText.gameObject.SetActive(false);
            return;
        }

        ShowRecord(PlayerPrefs.GetInt(RecordKey));
    }

    private void ShowRecord(int record)
    {
        recordText.text = $"Record: {record} shots";
        recordText.gameObject.SetActive(true);
    }

    private void UpdateShootCountText()
    {
        shootCountText.text = $"Shots: {shootCount}";
    }
}
