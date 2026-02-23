using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

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
    private RawImage blurBackdrop;
    private Texture2D blurTexture;

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
        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Final Score: {finalScore}";

        StartCoroutine(ShowGameOverWithBlur());
    }

    private IEnumerator ShowGameOverWithBlur()
    {
        // Wait until the frame has fully rendered (no panel overlay yet)
        yield return new WaitForEndOfFrame();

        // Capture the scene
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // Replace any previous blur texture
        if (blurTexture != null) Destroy(blurTexture);
        blurTexture = BlurScreenshot(screenshot);
        Destroy(screenshot);

        if (blurBackdrop == null && gameOverPanel != null)
        {
            // Parent to the gameOverPanel itself — no new Canvases, no sorting tricks.
            // Filling a known parent with stretch anchors always works regardless of
            // Canvas render mode, Canvas Scaler, or reference resolution.
            GameObject go = new GameObject("BlurBackdrop");
            go.transform.SetParent(gameOverPanel.transform, false);
            go.transform.SetAsFirstSibling(); // render behind panel's text and buttons

            // LayoutElement.ignoreLayout stops any Layout Group on the panel from
            // overriding the RectTransform size/position.
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            blurBackdrop = go.AddComponent<RawImage>();
            blurBackdrop.raycastTarget = false;

            RectTransform rt = blurBackdrop.rectTransform;
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.sizeDelta        = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        if (blurBackdrop != null)
        {
            blurBackdrop.texture = blurTexture;
            blurBackdrop.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // 4× downsample + 3-pass separable box blur → bilinear upscale on GPU when displayed
    private static Texture2D BlurScreenshot(Texture2D source)
    {
        const int scale  = 4;
        const int radius = 4;
        int sw = source.width  / scale;
        int sh = source.height / scale;

        Color32[] src    = source.GetPixels32();
        Color32[] pixels = new Color32[sw * sh];

        // Nearest-neighbour downsample
        for (int y = 0; y < sh; y++)
            for (int x = 0; x < sw; x++)
                pixels[y * sw + x] = src[y * scale * source.width + x * scale];

        // Separable box blur, 3 passes
        Color32[] temp = new Color32[sw * sh];
        for (int pass = 0; pass < 3; pass++)
        {
            // Horizontal
            for (int y = 0; y < sh; y++)
                for (int x = 0; x < sw; x++)
                {
                    int r = 0, g = 0, b = 0, n = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        Color32 p = pixels[y * sw + Mathf.Clamp(x + k, 0, sw - 1)];
                        r += p.r; g += p.g; b += p.b; n++;
                    }
                    temp[y * sw + x] = new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), 255);
                }
            // Vertical
            for (int y = 0; y < sh; y++)
                for (int x = 0; x < sw; x++)
                {
                    int r = 0, g = 0, b = 0, n = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        Color32 p = temp[Mathf.Clamp(y + k, 0, sh - 1) * sw + x];
                        r += p.r; g += p.g; b += p.b; n++;
                    }
                    pixels[y * sw + x] = new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), 255);
                }
        }

        Texture2D result = new Texture2D(sw, sh, TextureFormat.RGB24, false);
        result.filterMode = FilterMode.Bilinear; // GPU handles smooth upscaling
        result.SetPixels32(pixels);
        result.Apply();
        return result;
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        // blurBackdrop is a child of gameOverPanel and deactivates with it
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
