using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Renders the grand staff (treble and bass clefs) and notes on it
/// </summary>
public class StaffRenderer : MonoBehaviour
{
    [Header("Staff Settings")]
    [SerializeField] private float staffWidth = 12f;
    [SerializeField] private float lineSpacing = 0.5f;
    [SerializeField] private float staffGap = 2f; // Gap between treble and bass staves
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

    private GameObject currentNoteObject;
    private GameObject currentNoteLabelObject;
    private MusicNote currentNote; // Store current note for toggle updates
    private float currentNoteYPos; // Store Y position for label creation
    private LineRenderer[] trebleStaffLines;
    private LineRenderer[] bassStaffLines;
    private List<GameObject> ledgerLines = new List<GameObject>();
    private GameObject trebleClefObject;
    private GameObject bassClefObject;

    // Reference positions
    private float trebleStaffCenter; // Center of treble staff (B4)
    private float bassStaffCenter;   // Center of bass staff (D3)

    void Start()
    {
        CreateGrandStaff();
    }

    void CreateGrandStaff()
    {
        // Calculate staff positions
        // Treble staff is above, bass staff is below
        trebleStaffCenter = staffGap / 2 + lineSpacing * 2; // Middle line of treble staff
        bassStaffCenter = -staffGap / 2 - lineSpacing * 2;  // Middle line of bass staff
        Debug.Log("Treble Staff Center Y: " + trebleStaffCenter);
        Debug.Log("Bass Staff Center Y: " + bassStaffCenter);

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

    public void DisplayNote(MusicNote note)
    {
        // Remove previous note and ledger lines
        ClearNote();

        // Store current note and position for toggle updates
        currentNote = note;
        currentNoteYPos = CalculateNoteYPosition(note.StaffPosition);

        // Create new note
        currentNoteObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        currentNoteObject.name = "CurrentNote";
        currentNoteObject.transform.SetParent(transform);
        currentNoteObject.transform.localPosition = new Vector3(0, currentNoteYPos, -0.1f);
        currentNoteObject.transform.localScale = new Vector3(noteSize, noteSize, noteSize);

        // Set note color
        Renderer renderer = currentNoteObject.GetComponent<Renderer>();
        renderer.material.color = noteColor;

        // Add ledger lines if needed
        AddLedgerLinesIfNeeded(note.StaffPosition, currentNoteYPos);

        // Create note label if enabled
        if (showNoteNames)
        {
            CreateNoteLabel(note, currentNoteYPos);
        }
    }

    private float CalculateNoteYPosition(int staffPosition)
    {
        // staffPosition is relative to Middle C (C4) = 0
        // Each position is a half-step on the staff (lineSpacing / 2)
        // Middle C is between the two staves

        float middleCPosition = (trebleStaffCenter - lineSpacing * 2 + bassStaffCenter + lineSpacing * 2) / 2;
        Debug.Log($"Middle C Y Position: {middleCPosition}");
        return middleCPosition + staffPosition * (lineSpacing / 2);
    }

    private void AddLedgerLinesIfNeeded(int staffPosition, float noteYPos)
    {
        // Determine if ledger lines are needed
        float trebleBottom = trebleStaffCenter - lineSpacing * 2; // E4
        float trebleTop = trebleStaffCenter + lineSpacing * 2;    // F5
        float bassBottom = bassStaffCenter - lineSpacing * 2;     // G2
        float bassTop = bassStaffCenter + lineSpacing * 2;        // A3
        Debug.Log(trebleTop + " " + trebleBottom + " " + bassTop + " " + bassBottom);

        // Ledger lines below treble staff (between staves and for Middle C)
        if (noteYPos < trebleBottom && noteYPos > bassTop)
        {
            // Middle C and notes between staves
            float currentY = trebleBottom - lineSpacing;
            while (currentY >= noteYPos - 0.01f)
            {
                if (Mathf.Abs(currentY - noteYPos) < lineSpacing / 4)
                {
                    CreateLedgerLine(currentY);
                }
                currentY -= lineSpacing;
            }
        }

        // Ledger lines above treble staff
        if (noteYPos > trebleTop)
        {
            float currentY = trebleTop + lineSpacing;
            while (currentY <= noteYPos + 0.01f)
            {
                if (Mathf.Abs(currentY - noteYPos) < lineSpacing / 4)
                {
                    CreateLedgerLine(currentY);
                }
                currentY += lineSpacing;
            }
        }

        // Ledger lines below bass staff
        if (noteYPos < bassBottom)
        {
            float currentY = bassBottom - lineSpacing;
            while (currentY >= noteYPos - 0.01f)
            {
                if (Mathf.Abs(currentY - noteYPos) < lineSpacing / 4)
                {
                    CreateLedgerLine(currentY);
                }
                currentY -= lineSpacing;
            }
        }

        // Ledger lines above bass staff (between staves)
        if (noteYPos > bassTop && noteYPos < trebleBottom)
        {
            float currentY = bassTop + lineSpacing;
            while (currentY <= noteYPos + 0.01f)
            {
                if (Mathf.Abs(currentY - noteYPos) < lineSpacing / 4)
                {
                    CreateLedgerLine(currentY);
                }
                currentY += lineSpacing;
            }
        }
    }

    private void CreateLedgerLine(float yPos)
    {
        GameObject lineObj = new GameObject("LedgerLine");
        lineObj.transform.SetParent(transform);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = staffColor;
        line.endColor = staffColor;
        line.startWidth = lineThickness;
        line.endWidth = lineThickness;
        line.positionCount = 2;

        // Ledger lines are shorter than staff lines
        float ledgerWidth = noteSize * 1.5f;
        line.SetPosition(0, new Vector3(-ledgerWidth / 2, yPos, 0));
        line.SetPosition(1, new Vector3(ledgerWidth / 2, yPos, 0));
        line.useWorldSpace = false;

        ledgerLines.Add(lineObj);
    }

    private void CreateNoteLabel(MusicNote note, float noteYPos)
    {
        // Create a new GameObject for the label
        currentNoteLabelObject = new GameObject("NoteLabel");
        currentNoteLabelObject.transform.SetParent(transform);
        currentNoteLabelObject.transform.localPosition = new Vector3(0, noteYPos, -0.5f);

        // Add TextMeshPro component
        TMPro.TextMeshPro textMesh = currentNoteLabelObject.AddComponent<TMPro.TextMeshPro>();
        textMesh.text = note.GetFullName();
        textMesh.fontSize = noteLabelFontSize;
        textMesh.color = Color.black; // Black text for contrast on white note
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;

        // Center the text horizontally and vertically
        RectTransform rectTransform = currentNoteLabelObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(2f, 1f);
        }

        // Set sorting layer to render in front
        textMesh.sortingOrder = 10;
    }

    public void ClearNote()
    {
        if (currentNoteObject != null)
        {
            Destroy(currentNoteObject);
            currentNoteObject = null;
        }

        if (currentNoteLabelObject != null)
        {
            Destroy(currentNoteLabelObject);
            currentNoteLabelObject = null;
        }

        // Clear stored note data
        currentNote = null;

        // Clear all ledger lines
        foreach (GameObject ledgerLine in ledgerLines)
        {
            if (ledgerLine != null)
            {
                Destroy(ledgerLine);
            }
        }
        ledgerLines.Clear();
    }

    public void SetShowNoteNames(bool show)
    {
        showNoteNames = show;

        // Update the currently displayed note's label
        if (currentNote != null && currentNoteObject != null)
        {
            if (show && currentNoteLabelObject == null)
            {
                // Show label for current note
                CreateNoteLabel(currentNote, currentNoteYPos);
            }
            else if (!show && currentNoteLabelObject != null)
            {
                // Hide label for current note
                Destroy(currentNoteLabelObject);
                currentNoteLabelObject = null;
            }
        }
    }

    public bool GetShowNoteNames()
    {
        return showNoteNames;
    }
}