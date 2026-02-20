using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Manages the UI elements including buttons, score, lives, feedback, and game over
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Button[] noteButtons; // Buttons for C, D, E, F, G, A, B
    [SerializeField] private Toggle showNoteNamesToggle;
    [SerializeField] private Toggle debugModeToggle;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private Button restartButton;

    [Header("Feedback Settings")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private float feedbackDisplayTime = 1.5f;

    private GameManager gameManager;
    private StaffRenderer staffRenderer;
    private float feedbackTimer = 0f;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        staffRenderer = FindFirstObjectByType<StaffRenderer>();

        SetupNoteButtons();
        SetupShowNoteNamesToggle();
        SetupDebugModeToggle();
        SetupRestartButton();

        UpdateScore(0);
        ClearFeedback();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        // Handle feedback timer
        if (feedbackTimer > 0)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0)
                ClearFeedback();
        }

        // Handle keyboard input for note answers
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.cKey.wasPressedThisFrame) OnNoteButtonClicked(MusicNote.NoteName.C);
            else if (keyboard.dKey.wasPressedThisFrame) OnNoteButtonClicked(MusicNote.NoteName.D);
            else if (keyboard.eKey.wasPressedThisFrame) OnNoteButtonClicked(MusicNote.NoteName.E);
            else if (keyboard.fKey.wasPressedThisFrame) OnNoteButtonClicked(MusicNote.NoteName.F);
            else if (keyboard.gKey.wasPressedThisFrame) OnNoteButtonClicked(MusicNote.NoteName.G);
            else if (keyboard.aKey.wasPressedThisFrame) OnNoteButtonClicked(MusicNote.NoteName.A);
            else if (keyboard.bKey.wasPressedThisFrame) OnNoteButtonClicked(MusicNote.NoteName.B);
        }
    }

    private void SetupNoteButtons()
    {
        MusicNote.NoteName[] noteNames = new MusicNote.NoteName[]
        {
            MusicNote.NoteName.C,
            MusicNote.NoteName.D,
            MusicNote.NoteName.E,
            MusicNote.NoteName.F,
            MusicNote.NoteName.G,
            MusicNote.NoteName.A,
            MusicNote.NoteName.B
        };

        for (int i = 0; i < noteButtons.Length && i < noteNames.Length; i++)
        {
            MusicNote.NoteName noteName = noteNames[i];
            noteButtons[i].onClick.AddListener(() => OnNoteButtonClicked(noteName));

            TextMeshProUGUI buttonText = noteButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = noteName.ToString();
        }
    }

    private void SetupRestartButton()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(() => gameManager?.RestartGame());
    }

    private void SetupShowNoteNamesToggle()
    {
        if (showNoteNamesToggle != null && staffRenderer != null)
        {
            showNoteNamesToggle.isOn = staffRenderer.GetShowNoteNames();
            showNoteNamesToggle.onValueChanged.AddListener(OnShowNoteNamesToggled);
        }
    }

    private void OnShowNoteNamesToggled(bool isOn)
    {
        staffRenderer?.SetShowNoteNames(isOn);
    }

    private void SetupDebugModeToggle()
    {
        if (debugModeToggle != null && staffRenderer != null)
        {
            debugModeToggle.isOn = staffRenderer.GetDebugMode();
            debugModeToggle.onValueChanged.AddListener(OnDebugModeToggled);
        }
    }

    private void OnDebugModeToggled(bool isOn)
    {
        staffRenderer?.SetDebugMode(isOn);
        gameManager?.SetDebugMode(isOn);
    }

    private void OnNoteButtonClicked(MusicNote.NoteName selectedNote)
    {
        gameManager?.CheckAnswer(selectedNote);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void UpdateLives(int lives, int maxLives)
    {
        if (livesText != null)
            livesText.text = $"Lives: {lives}/{maxLives}";
    }

    public void ShowCorrectFeedback(string fullNoteName = "")
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.IsNullOrEmpty(fullNoteName)
                ? "Correct!"
                : $"Correct! ({fullNoteName})";
            feedbackText.color = correctColor;
            feedbackTimer = feedbackDisplayTime;
        }
    }

    public void ShowIncorrectFeedback(string correctAnswer, string fullNoteName = "")
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.IsNullOrEmpty(fullNoteName)
                ? $"Incorrect! The answer was {correctAnswer}"
                : $"Incorrect! The answer was {correctAnswer} ({fullNoteName})";
            feedbackText.color = incorrectColor;
            feedbackTimer = feedbackDisplayTime;
        }
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Final Score: {finalScore}";
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }

    public void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in noteButtons)
            button.interactable = interactable;
    }
}
