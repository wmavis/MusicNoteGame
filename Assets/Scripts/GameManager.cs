using UnityEngine;
using System.Collections;

/// <summary>
/// Main game controller that manages game flow and logic
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float delayBeforeNextNote = 2f;

    [Header("References")]
    [SerializeField] private StaffRenderer staffRenderer;
    [SerializeField] private UIManager uiManager;

    private MusicNote currentNote;
    private int score = 0;
    private bool waitingForAnswer = false;
    private bool debugMode = false;
    public static readonly int firstNoteIndex = 9; // C2
    public static readonly int lastNoteIndex = 37; // C6

    void Start()
    {
        // Find references if not assigned
        if (staffRenderer == null)
            staffRenderer = FindFirstObjectByType<StaffRenderer>();
        
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        // Start the game
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        // Wait a moment before starting
        yield return new WaitForSeconds(1f);

        while (true)
        {
            // Pause game loop if in debug mode
            if (debugMode)
            {
                yield return null;
                continue;
            }

            // Generate and display a new note
            GenerateNewNote();

            // Wait for player to answer
            waitingForAnswer = true;
            while (waitingForAnswer)
            {
                // Check if debug mode was enabled while waiting
                if (debugMode)
                {
                    waitingForAnswer = false;
                    break;
                }
                yield return null;
            }

            // Wait before showing next note
            yield return new WaitForSeconds(delayBeforeNextNote);
        }
    }

    void GenerateNewNote()
    {
        // Generate a random note
        currentNote = MusicNote.GenerateRandomNote(firstNoteIndex, lastNoteIndex);
        
        // Display it on the staff
        if (staffRenderer != null)
        {
            staffRenderer.ClearNotes();
            staffRenderer.DisplayNote(currentNote, currentNote.BassClef);
        }

        Debug.Log($"New note generated: {currentNote.GetFullName()} (pos: " + currentNote.StaffPosition + ")");
    }

    public void CheckAnswer(MusicNote.NoteName selectedNote)
    {
        if (!waitingForAnswer)
            return;

        // Disable buttons temporarily
        if (uiManager != null)
        {
            uiManager.SetButtonsInteractable(false);
        }

        // Check if the answer is correct
        if (selectedNote == currentNote.Name)
        {
            // Correct answer
            score++;
            if (uiManager != null)
            {
                uiManager.UpdateScore(score);
                uiManager.ShowCorrectFeedback(currentNote.GetFullName());
            }
            Debug.Log($"Correct! That was {currentNote.GetFullName()}");
        }
        else
        {
            // Incorrect answer
            if (uiManager != null)
            {
                uiManager.ShowIncorrectFeedback(currentNote.Name.ToString(), currentNote.GetFullName());
            }
            Debug.Log($"Incorrect! The correct answer was {currentNote.Name} ({currentNote.GetFullName()})");
        }

        // Re-enable buttons after a short delay
        StartCoroutine(ReEnableButtons());

        // Move to next note
        waitingForAnswer = false;
    }

    IEnumerator ReEnableButtons()
    {
        yield return new WaitForSeconds(0.5f);
        if (uiManager != null)
        {
            uiManager.SetButtonsInteractable(true);
        }
    }

    public void ResetGame()
    {
        score = 0;
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
        }

        StopAllCoroutines();
        StartCoroutine(GameLoop());
    }

    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;

        if (debugMode)
        {
            // Disable buttons in debug mode
            if (uiManager != null)
            {
                uiManager.SetButtonsInteractable(false);
            }

            waitingForAnswer = false;
        }
        else
        {
            // Re-enable buttons when exiting debug mode
            if (uiManager != null)
            {
                uiManager.SetButtonsInteractable(true);
            }

            // Game loop will automatically resume and generate a new note
        }
    }
}
