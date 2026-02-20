using UnityEngine;
using System.Collections;

/// <summary>
/// Main game controller that manages game flow, scoring, and lives
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float delayBeforeNextNote = 2f;
    [SerializeField] private int maxLives = 3;

    [Header("References")]
    [SerializeField] private StaffRenderer staffRenderer;
    [SerializeField] private UIManager uiManager;

    private MusicNote currentNote;
    private int score = 0;
    private int lives;
    private bool waitingForAnswer = false;
    private bool debugMode = false;
    private bool isGameOver = false;

    public static readonly int firstNoteIndex = 9; // C2
    public static readonly int lastNoteIndex = 37; // C6

    void Start()
    {
        if (staffRenderer == null)
            staffRenderer = FindFirstObjectByType<StaffRenderer>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        lives = maxLives;
        if (uiManager != null)
            uiManager.UpdateLives(lives, maxLives);

        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (isGameOver || debugMode)
            {
                yield return null;
                continue;
            }

            GenerateNewNote();

            waitingForAnswer = true;
            while (waitingForAnswer)
            {
                if (debugMode || isGameOver)
                {
                    waitingForAnswer = false;
                    break;
                }
                yield return null;
            }

            if (!isGameOver)
                yield return new WaitForSeconds(delayBeforeNextNote);
        }
    }

    void GenerateNewNote()
    {
        currentNote = MusicNote.GenerateRandomNote(firstNoteIndex, lastNoteIndex);

        if (staffRenderer != null)
        {
            staffRenderer.ClearNotes();
            staffRenderer.DisplayNote(currentNote, currentNote.BassClef);
            staffRenderer.StartNoteAnimation(OnNoteExpired);
        }

        Debug.Log($"New note generated: {currentNote.GetFullName()} (pos: {currentNote.StaffPosition})");
    }

    public void CheckAnswer(MusicNote.NoteName selectedNote)
    {
        if (!waitingForAnswer) return;

        staffRenderer?.StopNoteAnimation();

        if (uiManager != null)
            uiManager.SetButtonsInteractable(false);

        if (selectedNote == currentNote.Name)
        {
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
            if (uiManager != null)
                uiManager.ShowIncorrectFeedback(currentNote.Name.ToString(), currentNote.GetFullName());
            Debug.Log($"Incorrect! The correct answer was {currentNote.Name} ({currentNote.GetFullName()})");
            LoseLife();
        }

        if (!isGameOver)
            StartCoroutine(ReEnableButtons());

        waitingForAnswer = false;
    }

    // Called when the note reaches the right edge without an answer
    private void OnNoteExpired()
    {
        if (!waitingForAnswer) return;

        if (uiManager != null)
        {
            uiManager.SetButtonsInteractable(false);
            uiManager.ShowIncorrectFeedback(currentNote.Name.ToString(), currentNote.GetFullName());
        }

        Debug.Log($"Note expired! The correct answer was {currentNote.Name} ({currentNote.GetFullName()})");

        LoseLife();

        if (!isGameOver)
            StartCoroutine(ReEnableButtons());

        waitingForAnswer = false;
    }

    private void LoseLife()
    {
        lives--;
        if (uiManager != null)
            uiManager.UpdateLives(lives, maxLives);

        if (lives <= 0)
            GameOver();
    }

    private void GameOver()
    {
        isGameOver = true;
        waitingForAnswer = false;
        staffRenderer?.StopNoteAnimation();

        if (uiManager != null)
            uiManager.ShowGameOver(score);

        Debug.Log("Game Over!");
    }

    IEnumerator ReEnableButtons()
    {
        yield return new WaitForSeconds(0.5f);
        if (uiManager != null)
            uiManager.SetButtonsInteractable(true);
    }

    public void RestartGame()
    {
        isGameOver = false;
        score = 0;
        lives = maxLives;

        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            uiManager.UpdateLives(lives, maxLives);
            uiManager.HideGameOver();
            uiManager.SetButtonsInteractable(true);
        }

        if (staffRenderer != null)
            staffRenderer.ClearNotes();

        StopAllCoroutines();
        StartCoroutine(GameLoop());
    }

    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;

        if (debugMode)
        {
            staffRenderer?.StopNoteAnimation();
            if (uiManager != null)
                uiManager.SetButtonsInteractable(false);
            waitingForAnswer = false;
        }
        else
        {
            if (uiManager != null)
                uiManager.SetButtonsInteractable(true);
        }
    }
}
