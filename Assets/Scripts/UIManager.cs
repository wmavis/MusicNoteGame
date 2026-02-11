using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the UI elements including buttons, score, and feedback
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button[] noteButtons; // Buttons for C, D, E, F, G, A, B

    [Header("Feedback Settings")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private float feedbackDisplayTime = 1.5f;

    private GameManager gameManager;
    private float feedbackTimer = 0f;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        // Setup note buttons
        SetupNoteButtons();
        
        // Initialize UI
        UpdateScore(0);
        ClearFeedback();
    }

    void Update()
    {
        // Handle feedback timer
        if (feedbackTimer > 0)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0)
            {
                ClearFeedback();
            }
        }
    }

    private void SetupNoteButtons()
    {
        // Assign click handlers to each note button
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
            
            // Set button text
            TextMeshProUGUI buttonText = noteButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = noteName.ToString();
            }
        }
    }

    private void OnNoteButtonClicked(MusicNote.NoteName selectedNote)
    {
        if (gameManager != null)
        {
            gameManager.CheckAnswer(selectedNote);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void ShowCorrectFeedback(string fullNoteName = "")
    {
        if (feedbackText != null)
        {
            if (!string.IsNullOrEmpty(fullNoteName))
            {
                feedbackText.text = $"Correct! ✓ ({fullNoteName})";
            }
            else
            {
                feedbackText.text = "Correct! ✓";
            }
            feedbackText.color = correctColor;
            feedbackTimer = feedbackDisplayTime;
        }
    }

    public void ShowIncorrectFeedback(string correctAnswer, string fullNoteName = "")
    {
        if (feedbackText != null)
        {
            if (!string.IsNullOrEmpty(fullNoteName))
            {
                feedbackText.text = $"Incorrect! The answer was {correctAnswer} ({fullNoteName})";
            }
            else
            {
                feedbackText.text = $"Incorrect! The answer was {correctAnswer}";
            }
            feedbackText.color = incorrectColor;
            feedbackTimer = feedbackDisplayTime;
        }
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    public void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in noteButtons)
        {
            button.interactable = interactable;
        }
    }
}
