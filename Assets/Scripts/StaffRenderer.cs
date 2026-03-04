using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Renders the grand staff (treble and bass clefs) and notes on it
/// </summary>
public class StaffRenderer : MonoBehaviour
{
    [Header("Staff Settings")]
    [SerializeField] private float staffWidth = 20f;
    [SerializeField] private float lineSpacing = 0.5f; // Distance between staff lines
    [SerializeField] private float staffGap = 3.5f; // Gap between treble and bass staves
    [SerializeField] private Color staffColor = Color.black;
    [SerializeField] private float lineThickness = 0.05f;

    [Header("Note Settings")]
    [SerializeField] private Sprite wholeNoteSprite;
    [SerializeField] private Color noteColor = Color.black;
    [SerializeField] private float noteSize = 0.6f;
    [SerializeField] private int noteSortingOrder = 1;
    [SerializeField] private bool showNoteNames = false;
    [SerializeField] private float noteLabelFontSize = 4f;

    [Header("Note Animation")]
    [SerializeField] private float noteTravelTime = 6f;
    [SerializeField] private float noteStartXOffset = 2f; // Units from left edge where note appears

    [Header("Answer Animation")]
    [SerializeField] private float answerAnimDuration = 1.5f;
    [SerializeField] private float answerAnimDistance = 3f;

    [Header("Clef Symbols")]
    [SerializeField] private Sprite trebleClefSprite;
    [SerializeField] private Sprite bassClefSprite;
    [SerializeField] private float clefScale = 0.25f;
    [SerializeField] private int clefSortingOrder = 0;

    [Header("Debug Settings")]
    [SerializeField] private float debugNoteSpacing = 0.7f; // Horizontal spacing between debug notes

    // Note objects are now noteGroup containers; children (sprite, ledger lines, label) move with them
    private List<GameObject> noteObjects = new List<GameObject>();
    private List<GameObject> noteLabelObjects = new List<GameObject>();
    private Dictionary<GameObject, Coroutine> noteAnimCoroutines = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, Coroutine> answerAnimCoroutines = new Dictionary<GameObject, Coroutine>();

    private LineRenderer[] trebleStaffLines;
    private LineRenderer[] bassStaffLines;
    private GameObject trebleClefObject;
    private GameObject bassClefObject;

    // Debug mode
    private bool debugMode = false;

    // Reference positions (Y coordinates in world space)
    private float trebleStaffCenter; // Y position of treble staff middle line (B4, position +6)
    private float bassStaffCenter;   // Y position of bass staff middle line (D3, position -6)

    void Start()
    {
        CreateGrandStaff();
    }

    void CreateGrandStaff()
    {
        float middleCPosition = 0f;
        trebleStaffCenter = middleCPosition + 3 * lineSpacing + staffGap / 2;
        bassStaffCenter = middleCPosition - 3 * lineSpacing - staffGap / 2;

        Debug.Log("Middle C Position: " + middleCPosition);
        Debug.Log("Treble Staff Center Y (B4): " + trebleStaffCenter);
        Debug.Log("Bass Staff Center Y (D3): " + bassStaffCenter);
        Debug.Log("Gap between staff lines: " + ((trebleStaffCenter - 2*lineSpacing) - (bassStaffCenter + 2*lineSpacing)));

        // Create treble staff (5 lines)
        trebleStaffLines = new LineRenderer[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject lineObj = new GameObject($"TrebleStaffLine_{i}");
            lineObj.transform.SetParent(transform);

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = staffColor;
            line.endColor = staffColor;
            line.startWidth = lineThickness;
            line.endWidth = lineThickness;
            line.positionCount = 2;
            line.useWorldSpace = false;

            float yPos = trebleStaffCenter + (i - 2) * lineSpacing;
            line.SetPosition(0, new Vector3(-staffWidth / 2, yPos, 0));
            line.SetPosition(1, new Vector3(staffWidth / 2, yPos, 0));

            trebleStaffLines[i] = line;
        }

        // Create bass staff (5 lines)
        bassStaffLines = new LineRenderer[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject lineObj = new GameObject($"BassStaffLine_{i}");
            lineObj.transform.SetParent(transform);

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = staffColor;
            line.endColor = staffColor;
            line.startWidth = lineThickness;
            line.endWidth = lineThickness;
            line.positionCount = 2;
            line.useWorldSpace = false;

            float yPos = bassStaffCenter + (i - 2) * lineSpacing;
            line.SetPosition(0, new Vector3(-staffWidth / 2, yPos, 0));
            line.SetPosition(1, new Vector3(staffWidth / 2, yPos, 0));

            bassStaffLines[i] = line;
        }

        CreateClefSymbols();

        transform.position = new Vector3(0, 2, 0);
    }

    void CreateClefSymbols()
    {
        trebleClefObject = new GameObject("TrebleClef");
        trebleClefObject.transform.SetParent(transform);
        SpriteRenderer trebleRenderer = trebleClefObject.AddComponent<SpriteRenderer>();
        trebleRenderer.sprite = trebleClefSprite;
        trebleRenderer.color = staffColor;
        trebleRenderer.sortingOrder = clefSortingOrder;
        trebleClefObject.transform.localPosition = new Vector3(-staffWidth / 2 + 3 * clefScale, trebleStaffCenter, -1f);
        trebleClefObject.transform.localScale = new Vector3(clefScale, clefScale, 1f);
        Debug.Log($"Treble Clef - World Pos: {trebleClefObject.transform.position}, Local Pos: {trebleClefObject.transform.localPosition}, Sorting Order: {trebleRenderer.sortingOrder}, Sprite Assigned: {trebleRenderer.sprite != null}");

        bassClefObject = new GameObject("BassClef");
        bassClefObject.transform.SetParent(transform);
        SpriteRenderer bassRenderer = bassClefObject.AddComponent<SpriteRenderer>();
        bassRenderer.sprite = bassClefSprite;
        bassRenderer.color = staffColor;
        bassRenderer.sortingOrder = clefSortingOrder;
        bassClefObject.transform.localPosition = new Vector3(-staffWidth / 2 + 3 * clefScale, bassStaffCenter, -1f);
        bassClefObject.transform.localScale = new Vector3(clefScale, clefScale, 1f);
        Debug.Log($"Bass Clef - World Pos: {bassClefObject.transform.position}, Local Pos: {bassClefObject.transform.localPosition}, Sorting Order: {bassRenderer.sortingOrder}, Sprite Assigned: {bassRenderer.sprite != null}");
    }

    // Regular play: note starts at the left edge of the staff
    public GameObject DisplayNote(MusicNote note, bool bassClef)
    {
        return DisplayNote(note, -staffWidth / 2 + noteStartXOffset, bassClef);
    }

    // Debug mode: note placed at a specific X position
    public GameObject DisplayNote(MusicNote note, float noteXPos, bool bassClef)
    {
        float noteYPos = CalculateNoteYPosition(note.StaffPosition, bassClef);

        // Create a group container so sprite, ledger lines, and label all move together
        GameObject noteGroup = new GameObject($"NoteGroup_{note.GetFullName()}");
        noteGroup.transform.SetParent(transform);
        noteGroup.transform.localPosition = new Vector3(noteXPos, noteYPos, 0f);
        noteObjects.Add(noteGroup);

        // Note sprite as child of group
        GameObject noteSprite = new GameObject("NoteSprite");
        noteSprite.transform.SetParent(noteGroup.transform);
        noteSprite.transform.localPosition = Vector3.zero;
        noteSprite.transform.localScale = new Vector3(noteSize, noteSize, 1f);

        SpriteRenderer sr = noteSprite.AddComponent<SpriteRenderer>();
        sr.sprite = wholeNoteSprite;
        sr.color = noteColor;
        sr.sortingOrder = noteSortingOrder;

        // Ledger lines as children of group (they translate with the note)
        AddLedgerLinesIfNeeded(note.StaffPosition, noteGroup, noteYPos);

        if (showNoteNames)
            CreateNoteLabel(note, noteGroup.transform);

        return noteGroup;
    }

    private float CalculateNoteYPosition(int staffPosition, bool bassClef)
    {
        float middleCPosition = bassClef ? -staffGap / 2 : staffGap / 2;
        return middleCPosition + staffPosition * (lineSpacing / 2);
    }

    private void AddLedgerLinesIfNeeded(int staffPosition, GameObject noteGroup, float noteYPos)
    {
        float trebleBottom = trebleStaffCenter - lineSpacing * 2;
        float trebleTop = trebleStaffCenter + lineSpacing * 2;
        float bassBottom = bassStaffCenter - lineSpacing * 2;
        float bassTop = bassStaffCenter + lineSpacing * 2;
        float center = (trebleStaffCenter + bassStaffCenter) / 2;
        Debug.Log($"Treble: {trebleTop} to {trebleBottom}, Bass: {bassTop} to {bassBottom}");

        // Ledger lines below treble staff (Middle C area)
        if (noteYPos < trebleBottom && noteYPos > center)
        {
            float currentY = trebleBottom - lineSpacing;
            while (currentY >= noteYPos - 0.01f)
            {
                CreateLedgerLine(noteGroup, currentY - noteYPos);
                currentY -= lineSpacing;
            }
        }

        // Ledger lines above treble staff
        if (noteYPos > trebleTop)
        {
            float currentY = trebleTop + lineSpacing;
            while (currentY <= noteYPos + 0.01f)
            {
                CreateLedgerLine(noteGroup, currentY - noteYPos);
                currentY += lineSpacing;
            }
        }

        // Ledger lines below bass staff
        if (noteYPos < bassBottom)
        {
            float currentY = bassBottom - lineSpacing;
            while (currentY >= noteYPos - 0.01f)
            {
                CreateLedgerLine(noteGroup, currentY - noteYPos);
                currentY -= lineSpacing;
            }
        }

        // Ledger lines above bass staff (between staves)
        if (noteYPos > bassTop && noteYPos < center)
        {
            float currentY = bassTop + lineSpacing;
            while (currentY <= noteYPos + 0.01f)
            {
                CreateLedgerLine(noteGroup, currentY - noteYPos);
                currentY += lineSpacing;
            }
        }
    }

    // relativeY is the ledger line's Y offset from the note group center
    private void CreateLedgerLine(GameObject noteGroup, float relativeY)
    {
        GameObject lineObj = new GameObject("LedgerLine");
        lineObj.transform.SetParent(noteGroup.transform);
        lineObj.transform.localPosition = Vector3.zero;

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = staffColor;
        line.endColor = staffColor;
        line.startWidth = lineThickness;
        line.endWidth = lineThickness;
        line.positionCount = 2;
        line.useWorldSpace = false;

        float ledgerWidth = noteSize * 1.5f;
        line.SetPosition(0, new Vector3(-ledgerWidth / 2, relativeY, 0));
        line.SetPosition(1, new Vector3(ledgerWidth / 2, relativeY, 0));
    }

    private void CreateNoteLabel(MusicNote note, Transform parent)
    {
        GameObject noteLabelObject = new GameObject("NoteLabel");
        noteLabelObject.transform.SetParent(parent);
        noteLabelObject.transform.localPosition = new Vector3(0, 0, -0.5f);
        noteLabelObjects.Add(noteLabelObject);

        TMPro.TextMeshPro textMesh = noteLabelObject.AddComponent<TMPro.TextMeshPro>();
        textMesh.text = note.GetFullName();
        textMesh.fontSize = noteLabelFontSize;
        textMesh.color = Color.white;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;

        RectTransform rectTransform = noteLabelObject.GetComponent<RectTransform>();
        if (rectTransform != null)
            rectTransform.sizeDelta = new Vector2(2f, 1f);

        textMesh.sortingOrder = 10;
    }

    public void ClearNotes()
    {
        foreach (var c in noteAnimCoroutines.Values)
            if (c != null) StopCoroutine(c);
        noteAnimCoroutines.Clear();

        foreach (var c in answerAnimCoroutines.Values)
            if (c != null) StopCoroutine(c);
        answerAnimCoroutines.Clear();

        foreach (GameObject noteGroup in noteObjects)
        {
            if (noteGroup != null)
                Destroy(noteGroup);
        }
        noteObjects.Clear();
        noteLabelObjects.Clear();  // Destroyed with parent noteGroup
    }

    // --- Animation ---

    public void StartNoteAnimation(GameObject noteGroup, System.Action onExpired)
    {
        if (noteGroup == null) return;
        float endX = staffWidth / 2;
        noteAnimCoroutines[noteGroup] = StartCoroutine(AnimateNote(noteGroup, endX, onExpired));
    }

    // Stop all note animations (used by ClearNotes path)
    public void StopNoteAnimation()
    {
        foreach (var c in noteAnimCoroutines.Values)
            if (c != null) StopCoroutine(c);
        noteAnimCoroutines.Clear();
    }

    // Stop animation for a specific note (used when player answers)
    public void StopNoteAnimation(GameObject noteGroup)
    {
        if (noteGroup == null) return;
        if (noteAnimCoroutines.TryGetValue(noteGroup, out Coroutine c))
        {
            if (c != null) StopCoroutine(c);
            noteAnimCoroutines.Remove(noteGroup);
        }
    }

    private IEnumerator AnimateNote(GameObject noteGroup, float endX, System.Action onComplete)
    {
        if (noteGroup == null) yield break;

        // totalDistance is fixed at spawn time; speed = totalDistance / noteTravelTime is
        // re-evaluated every frame so that a mid-game difficulty change takes effect immediately.
        float totalDistance = endX - noteGroup.transform.localPosition.x;

        while (true)
        {
            if (noteGroup == null) yield break;

            float speed = totalDistance / noteTravelTime; // units per second
            Vector3 pos = noteGroup.transform.localPosition;
            float newX = pos.x + speed * Time.deltaTime;

            if (newX >= endX)
            {
                noteAnimCoroutines.Remove(noteGroup);
                onComplete?.Invoke();
                yield break;
            }

            noteGroup.transform.localPosition = new Vector3(newX, pos.y, pos.z);
            yield return null;
        }
    }

    public void SetNoteTravelTime(float time)
    {
        noteTravelTime = Mathf.Max(0.5f, time);
    }

    // --- Answer Animation ---

    public void PlayAnswerAnimation(GameObject noteGroup, bool correct)
    {
        if (noteGroup == null) return;
        if (answerAnimCoroutines.TryGetValue(noteGroup, out Coroutine existing) && existing != null)
            StopCoroutine(existing);

        SpriteRenderer sr = noteGroup.transform.Find("NoteSprite")?.GetComponent<SpriteRenderer>();
        float direction = correct ? 1f : -1f;
        answerAnimCoroutines[noteGroup] = StartCoroutine(AnswerAnimCoroutine(noteGroup, sr, direction));
    }

    private IEnumerator AnswerAnimCoroutine(GameObject noteGroup, SpriteRenderer sr, float direction)
    {
        if (noteGroup == null) yield break;
        Vector3 startPos   = noteGroup.transform.localPosition;
        Color   startColor = sr != null ? sr.color : Color.black;
        float elapsed = 0f;
        while (elapsed < answerAnimDuration)
        {
            if (noteGroup == null) yield break;
            float t = elapsed / answerAnimDuration;
            Vector3 pos = noteGroup.transform.localPosition;
            noteGroup.transform.localPosition = new Vector3(pos.x, startPos.y + answerAnimDistance * direction * t, pos.z);
            if (sr != null)
                sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        answerAnimCoroutines.Remove(noteGroup);
        noteObjects.Remove(noteGroup);
        if (noteGroup != null) Destroy(noteGroup);
    }

    // --- Show Note Names ---

    public void SetShowNoteNames(bool show)
    {
        showNoteNames = show;

        if (debugMode)
            DisplayAllDebugNotes();
    }

    public bool GetShowNoteNames()
    {
        return showNoteNames;
    }

    // --- Debug Mode ---

    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;

        if (debugMode)
        {
            DisplayAllDebugNotes();
        }
        else
        {
            ClearNotes();
        }
    }

    public bool GetDebugMode()
    {
        return debugMode;
    }

    private void DisplayAllDebugNotes()
    {
        ClearNotes();

        List<MusicNote> allNotes = MusicNote.GetAllNotes();

        float totalWidth = (allNotes.Count - 1) * debugNoteSpacing;
        float startX = -totalWidth / 2;

        Debug.Log($"Displaying {allNotes.Count} debug notes with spacing {debugNoteSpacing}. Total width: {totalWidth}, Starting X: {startX}");

        int middleCIndex = (allNotes.Count - 1) / 2;

        for (int i = 0; i <= middleCIndex + 2; i++)
            DisplayNote(allNotes[i], startX + i * debugNoteSpacing, true);

        for (int i = middleCIndex - 2; i < allNotes.Count; i++)
            DisplayNote(allNotes[i], startX + i * debugNoteSpacing, false);
    }
}
