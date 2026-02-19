using UnityEngine;
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
    [SerializeField] private Color noteColor = Color.black;
    [SerializeField] private float noteSize = 0.6f;
    [SerializeField] private bool showNoteNames = false;
    [SerializeField] private float noteLabelFontSize = 4f;

    [Header("Clef Symbols")]
    [SerializeField] private Sprite trebleClefSprite;
    [SerializeField] private Sprite bassClefSprite;
    [SerializeField] private float clefScale = 0.25f;
    [SerializeField] private int clefSortingOrder = 0;

    [Header("Debug Settings")]
    [SerializeField] private float debugNoteSpacing = 0.7f; // Horizontal spacing between debug notes

    private List<GameObject> noteObjects = new List<GameObject>();
    private List<GameObject> noteLabelObjects = new List<GameObject>();
    private List<GameObject> noteLedgerLineObjects = new List<GameObject>();
    private MusicNote currentNote; // Store current note for toggle updates
    private float currentNoteYPos; // Store Y position for label creation
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
        // Calculate staff positions to maintain correct musical relationships
        //
        // In standard notation:
        // - Treble staff middle line = B4
        // - Bass staff middle line = D3
        // - B4 is 6 positions above Middle C (C4)
        // - D3 is 6 positions below Middle C
        // - Each position = lineSpacing / 2

        float middleCPosition = 0f; // Use screen center as reference
        trebleStaffCenter = middleCPosition + 3 * lineSpacing + staffGap / 2; // B4 at 3 * lineSpacing above C4
        bassStaffCenter = middleCPosition - 3 * lineSpacing - staffGap / 2;    // D3 at 3 * lineSpacing below C4

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

            float yPos = trebleStaffCenter + (i - 2) * lineSpacing; // -2 to center on middle line
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
            //lineObj.transform.position = new Vector3(0, 0, 0);

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = staffColor;
            line.endColor = staffColor;
            line.startWidth = lineThickness;
            line.endWidth = lineThickness;
            line.positionCount = 2;
            line.useWorldSpace = false;

            float yPos = bassStaffCenter + (i - 2) * lineSpacing; // -2 to center on middle line
            line.SetPosition(0, new Vector3(-staffWidth / 2, yPos, 0));
            line.SetPosition(1, new Vector3(staffWidth / 2, yPos, 0));

            bassStaffLines[i] = line;
        }

        // Create clef symbols (placeholder for now - will need sprites)
        CreateClefSymbols();

        // Center the staff
        transform.position = new Vector3(0, 2, 0);
    }

    void CreateClefSymbols()
    {
        // Treble clef symbol
        trebleClefObject = new GameObject("TrebleClef");
        trebleClefObject.transform.SetParent(transform);
        SpriteRenderer trebleRenderer = trebleClefObject.AddComponent<SpriteRenderer>();
        trebleRenderer.sprite = trebleClefSprite;
        trebleRenderer.color = staffColor;
        trebleRenderer.sortingOrder = clefSortingOrder;
        trebleClefObject.transform.localPosition = new Vector3(-staffWidth / 2 + 3 * clefScale, trebleStaffCenter, -1f);
        trebleClefObject.transform.localScale = new Vector3(clefScale, clefScale, 1f);
        Debug.Log($"Treble Clef - World Pos: {trebleClefObject.transform.position}, Local Pos: {trebleClefObject.transform.localPosition}, Sorting Order: {trebleRenderer.sortingOrder}, Sprite Assigned: {trebleRenderer.sprite != null}");

        // Bass clef symbol
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

    public void DisplayNote(MusicNote note, bool bassClef)
    {
        DisplayNote(note, 0, bassClef);
    }

    public void DisplayNote(MusicNote note, float noteXPos, bool bassClef)
    {
        // Store current note and position for toggle updates
        currentNote = note;
        currentNoteYPos = CalculateNoteYPosition(note.StaffPosition, bassClef);

        // Create new note
        GameObject noteObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        noteObject.name = $"Note_{note.GetFullName()}";
        noteObject.transform.SetParent(transform);
        noteObject.transform.localPosition = new Vector3(noteXPos, currentNoteYPos, -0.1f);
        noteObject.transform.localScale = new Vector3(noteSize, noteSize, noteSize);
        noteObjects.Add(noteObject);

        // Set note color
        Renderer renderer = noteObject.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = noteColor;

        // Add ledger lines if needed
        AddLedgerLinesIfNeeded(note.StaffPosition, noteXPos, currentNoteYPos);

        // Create note label if enabled
        if (showNoteNames)
        {
            CreateNoteLabel(note, noteXPos, currentNoteYPos);
        }
    }

    private float CalculateNoteYPosition(int staffPosition, bool bassClef)
    {
        // staffPosition is relative to Middle C (C4) = 0
        // Each position moves up one line or space (lineSpacing / 2)
        // Middle C is at Y positions staffGap/2 (right hand) and -staffGap/2 (left hand)

        float middleCPosition = bassClef ? -staffGap / 2 : staffGap / 2;
        return middleCPosition + staffPosition * (lineSpacing / 2);
    }

    private void AddLedgerLinesIfNeeded(int staffPosition, float noteXPos, float noteYPos)
    {
        // Determine if ledger lines are needed
        // Treble staff: E4 (bottom line) to F5 (top line)
        // Bass staff: G2 (bottom line) to A3 (top line)
        float trebleBottom = trebleStaffCenter - lineSpacing * 2; // E4 (position 2)
        float trebleTop = trebleStaffCenter + lineSpacing * 2;    // F5 (position 10)
        float bassBottom = bassStaffCenter - lineSpacing * 2;     // G2 (position -10)
        float bassTop = bassStaffCenter + lineSpacing * 2;        // A3 (position -2)
        float center = (trebleStaffCenter + bassStaffCenter) / 2; // 0
        Debug.Log($"Treble: {trebleTop} to {trebleBottom}, Bass: {bassTop} to {bassBottom}");

        // Ledger lines below treble staff (between staves and for Middle C)
        if (noteYPos < trebleBottom && noteYPos > center)
        {
            // Middle C and notes between staves
            float currentY = trebleBottom - lineSpacing;
            while (currentY >= noteYPos - 0.01f)
            {
                CreateLedgerLine(noteXPos, currentY);
                currentY -= lineSpacing;
            }
        }

        // Ledger lines above treble staff
        if (noteYPos > trebleTop)
        {
            float currentY = trebleTop + lineSpacing;
            while (currentY <= noteYPos + 0.01f)
            {
                CreateLedgerLine(noteXPos, currentY);
                currentY += lineSpacing;
            }
        }

        // Ledger lines below bass staff
        if (noteYPos < bassBottom)
        {
            float currentY = bassBottom - lineSpacing;
            while (currentY >= noteYPos - 0.01f)
            {
                CreateLedgerLine(noteXPos, currentY);
                currentY -= lineSpacing;
            }
        }

        // Ledger lines above bass staff (between staves)
        if (noteYPos > bassTop && noteYPos < center)
        {
            float currentY = bassTop + lineSpacing;
            while (currentY <= noteYPos + 0.01f)
            {
                CreateLedgerLine(noteXPos, currentY);
                currentY += lineSpacing;
            }
        }
    }

    private void CreateLedgerLine(float xPos, float yPos)
    {
        GameObject lineObj = new GameObject("LedgerLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = new Vector3(0, 0, 0);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = staffColor;
        line.endColor = staffColor;
        line.startWidth = lineThickness;
        line.endWidth = lineThickness;
        line.positionCount = 2;
        line.useWorldSpace = false;

        // Ledger lines are shorter than staff lines
        float ledgerWidth = noteSize * 1.5f;
        line.SetPosition(0, new Vector3(xPos - ledgerWidth / 2, yPos, 0));
        line.SetPosition(1, new Vector3(xPos + ledgerWidth / 2, yPos, 0));

        noteLedgerLineObjects.Add(lineObj);
    }

    private void CreateNoteLabel(MusicNote note, float noteXPos, float noteYPos)
    {
        // Create a new GameObject for the label
        GameObject noteLabelObject = new GameObject("NoteLabel");
        noteLabelObject.transform.SetParent(transform);
        noteLabelObject.transform.localPosition = new Vector3(noteXPos, noteYPos, -0.5f);
        noteLabelObjects.Add(noteLabelObject);

        // Add TextMeshPro component
        TMPro.TextMeshPro textMesh = noteLabelObject.AddComponent<TMPro.TextMeshPro>();
        textMesh.text = note.GetFullName();
        textMesh.fontSize = noteLabelFontSize;
        textMesh.color = Color.white; // White text for contrast on black note
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;

        // Center the text horizontally and vertically
        RectTransform rectTransform = noteLabelObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(2f, 1f);
        }

        // Set sorting layer to render in front
        textMesh.sortingOrder = 10;
    }

    public void ClearNotes()
    {
        // Clear all note objects
        foreach (GameObject noteObj in noteObjects)
        {
            if (noteObj != null)
            {
                Destroy(noteObj);
            }
        }
        noteObjects.Clear();

        // Clear all note labels
        foreach (GameObject labelObj in noteLabelObjects)
        {
            if (labelObj != null)
            {
                Destroy(labelObj);
            }
        }
        noteLabelObjects.Clear();

        // Clear all notes ledger lines
        foreach (GameObject ledgerLine in noteLedgerLineObjects)
        {
            if (ledgerLine != null)
            {
                Destroy(ledgerLine);
            }
        }
        noteLedgerLineObjects.Clear();
    }

    public void SetShowNoteNames(bool show)
    {
        showNoteNames = show;

        // Update the currently displayed note's label
        if (currentNote != null && noteObjects.Count > 0)
        {
            if (show && noteLabelObjects.Count == 0)
            {
                // Show label for current note
                CreateNoteLabel(currentNote, 0, currentNoteYPos);
            }
            else if (!show && noteLabelObjects.Count > 0)
            {
                foreach(GameObject noteLabelObject in noteLabelObjects)
                {
                    Destroy(noteLabelObject);

                }
                noteLabelObjects.Clear();
            }
        }

        // Update debug mode labels if in debug mode
        if (debugMode)
        {
            // Refresh debug display to show/hide labels
            DisplayAllDebugNotes();
        }
    }

    public bool GetShowNoteNames()
    {
        return showNoteNames;
    }

    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;

        if (debugMode)
        {
            // Show all notes
            DisplayAllDebugNotes();
        }
        else
        {
            // Clear all debug notes
            ClearNotes();
        }
    }

    public bool GetDebugMode()
    {
        return debugMode;
    }

    private void DisplayAllDebugNotes()
    {
        // Clear any existing notes first
        ClearNotes();

        // Get list of notes
        List<MusicNote> allNotes = MusicNote.GetAllNotes();

        // Calculate starting X position to center all notes
        float totalWidth = (allNotes.Count - 1) * debugNoteSpacing;
        float startX = -totalWidth / 2;

        Debug.Log($"Displaying {allNotes.Count} debug notes with spacing {debugNoteSpacing}. Total width: {totalWidth}, Starting X: {startX}");

        int middleCIndex = (allNotes.Count - 1) / 2;
        // Display each bass clef note
        for (int i = 0; i <= middleCIndex + 2; i++)
        {
            DisplayNote(allNotes[i], startX + i * debugNoteSpacing, true);
        }

        // Display each treble clef note
        for (int i = middleCIndex - 2; i < allNotes.Count; i++)
        {
            DisplayNote(allNotes[i], startX + i * debugNoteSpacing, false);
        }
    }
}