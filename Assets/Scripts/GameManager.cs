using UnityEngine;
using System.Collections;

/// <summary>
/// Main game controller that manages game flow, scoring, and lives
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum DifficultyLevel { Adagio = 0, Andante = 1, Moderato = 2, Allegro = 3 }

    private struct DifficultySettings
    {
        public float baseTravelTime;      // starting note travel time in seconds
        public float speedIncreasePerScore; // seconds removed from travel time per correct answer
        public float minTravelTime;       // floor so the note never becomes impossibly fast

        public DifficultySettings(float baseTravelTime, float speedIncreasePerScore, float minTravelTime)
        {
            this.baseTravelTime = baseTravelTime;
            this.speedIncreasePerScore = speedIncreasePerScore;
            this.minTravelTime = minTravelTime;
        }
    }

    // Adagio → Allegro: progressively faster base speed and steeper ramp
    private static readonly DifficultySettings[] difficultyPresets = new DifficultySettings[]
    {
        new DifficultySettings(10f, 0.05f, 5f),   // Adagio
        new DifficultySettings( 8f, 0.08f, 4f),   // Andante
        new DifficultySettings( 6f, 0.12f, 2.5f), // Moderato
        new DifficultySettings( 4f, 0.15f, 1.5f), // Allegro
    };

    [Header("Game Settings")]
    [SerializeField] private float delayBeforeNextNote = 2f;
    [SerializeField] private int maxLives = 3;

    [Header("Difficulty")]
    [SerializeField] private DifficultyLevel startingDifficulty = DifficultyLevel.Moderato;
    private DifficultyLevel currentDifficulty;

    [Header("References")]
    [SerializeField] private StaffRenderer staffRenderer;
    [SerializeField] private UIManager uiManager;

    [Header("Audio")]
    [SerializeField] private AudioClip correctSoundClip;
    [SerializeField] private AudioClip incorrectSoundClip;
    [SerializeField][Range(0f, 1f)] private float audioVolume = 0.5f;

    private MusicNote currentNote;
    private int score = 0;
    private int lives;
    private bool waitingForAnswer = false;
    private bool debugMode = false;
    private bool isGameOver = false;

    private AudioSource audioSource;
    private AudioClip currentNoteToneClip;
    private AudioClip proceduralIncorrectClip;

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

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        proceduralIncorrectClip = GeneratePianoSlam();

        currentDifficulty = startingDifficulty;
        UpdateNoteSpeed();

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
        currentNoteToneClip = GeneratePianoTone(GetNoteFrequency(currentNote));

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

        bool isCorrect = (selectedNote == currentNote.Name);
        PlaySound(isCorrect);
        staffRenderer?.PlayAnswerAnimation(isCorrect);

        uiManager?.SetButtonsInteractable(false);

        if (isCorrect)
        {
            score++;
            UpdateNoteSpeed();
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

        PlaySound(correct: false);
        staffRenderer?.PlayAnswerAnimation(correct: false);

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
        UpdateNoteSpeed();

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

    private void PlaySound(bool correct)
    {
        if (audioSource == null) return;
        AudioClip clip = correct
            ? (correctSoundClip   != null ? correctSoundClip   : currentNoteToneClip)
            : (incorrectSoundClip != null ? incorrectSoundClip : proceduralIncorrectClip);
        if (clip != null)
            audioSource.PlayOneShot(clip, audioVolume);
    }

    // Piano-like tone with harmonic overtones and exponential decay
    private static AudioClip GeneratePianoTone(float frequency, float duration = 0.8f)
    {
        int sampleRate   = 44100;
        int sampleCount  = Mathf.CeilToInt(sampleRate * duration);
        float[] samples  = new float[sampleCount];
        float[] harmAmps = { 1.0f, 0.5f, 0.3f, 0.2f, 0.12f, 0.07f, 0.04f };
        float totalAmp   = 0f;
        foreach (float a in harmAmps) totalAmp += a;

        int attackSamples = Mathf.CeilToInt(0.008f * sampleRate); // 8ms sharp attack
        for (int i = 0; i < sampleCount; i++)
        {
            float t    = (float)i / sampleRate;
            float norm = (float)i / sampleCount;
            float env  = i < attackSamples
                ? (float)i / attackSamples
                : Mathf.Exp(-4.5f * (norm - (float)attackSamples / sampleCount));

            float sample = 0f;
            for (int h = 0; h < harmAmps.Length; h++)
            {
                float partialEnv = Mathf.Exp(-(h * 1.5f) * norm); // upper partials decay faster
                sample += harmAmps[h] * partialEnv * Mathf.Sin(2f * Mathf.PI * frequency * (h + 1) * t);
            }
            samples[i] = (sample / totalAmp) * env;
        }

        AudioClip clip = AudioClip.Create($"PianoTone_{(int)frequency}Hz", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Dense low-register cluster simulating an arm slam across multiple piano keys
    private static AudioClip GeneratePianoSlam()
    {
        int sampleRate  = 44100;
        float duration  = 1.2f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] raw     = new float[sampleCount];

        // Spread across A1–G3 (55–196 Hz) for a wide, heavy slam
        float[] freqs    = { 55f, 65.4f, 82.4f, 98f, 110f, 130.8f, 146.8f, 164.8f, 196f };
        float[] harmAmps = { 1.0f, 0.55f, 0.3f, 0.18f, 0.1f };

        int attackSamples = Mathf.CeilToInt(0.004f * sampleRate); // 4ms forceful attack
        for (int i = 0; i < sampleCount; i++)
        {
            float t    = (float)i / sampleRate;
            float norm = (float)i / sampleCount;
            float env  = i < attackSamples
                ? (float)i / attackSamples
                : Mathf.Exp(-3.5f * norm);

            float sample = 0f;
            foreach (float freq in freqs)
                for (int h = 0; h < harmAmps.Length; h++)
                {
                    float partialEnv = Mathf.Exp(-(h * 1.2f) * norm);
                    sample += harmAmps[h] * partialEnv * Mathf.Sin(2f * Mathf.PI * freq * (h + 1) * t);
                }
            raw[i] = sample * env;
        }

        // Normalize to 0.95 peak so it hits hard without clipping
        float peak = 0f;
        foreach (float s in raw) peak = Mathf.Max(peak, Mathf.Abs(s));
        float[] samples = new float[sampleCount];
        float scale = peak > 0f ? 0.95f / peak : 1f;
        for (int i = 0; i < sampleCount; i++)
            samples[i] = raw[i] * scale;

        AudioClip clip = AudioClip.Create("PianoSlam", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float GetNoteFrequency(MusicNote note)
    {
        int[] semitones = { 0, 2, 4, 5, 7, 9, 11 }; // C D E F G A B
        int midi = (note.Octave + 1) * 12 + semitones[(int)note.Name];
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }

    // --- Difficulty ---

    public int StartingDifficultyIndex => (int)startingDifficulty;

    public void SetDifficultyByIndex(int index)
    {
        currentDifficulty = (DifficultyLevel)Mathf.Clamp(index, 0, difficultyPresets.Length - 1);
        UpdateNoteSpeed();
    }

    private void UpdateNoteSpeed()
    {
        DifficultySettings s = difficultyPresets[(int)currentDifficulty];
        float travelTime = Mathf.Max(s.minTravelTime, s.baseTravelTime - s.speedIncreasePerScore * score);
        staffRenderer?.SetNoteTravelTime(travelTime);
        Debug.Log($"Note speed: {travelTime:F2}s travel time (difficulty: {currentDifficulty}, score: {score})");
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
