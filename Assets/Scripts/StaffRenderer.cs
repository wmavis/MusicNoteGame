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

    [Header("Clef Symbols")]
    [SerializeField] private Sprite trebleClefSprite;
    [SerializeField] private Sprite bassClefSprite;
    [SerializeField] private float clefScale = 0.25f;

    private GameObject currentNoteObject;
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
        trebleClefObject.transform.localPosition = new Vector3(-staffWidth / 2 + 3*clefScale, trebleStaffCenter, -0.1f);
        trebleClefObject.transform.localScale = new Vector3(clefScale, clefScale, 1f);

        // Bass clef symbol
        bassClefObject = new GameObject("BassClef");
        bassClefObject.transform.SetParent(transform);
        SpriteRenderer bassRenderer = bassClefObject.AddComponent<SpriteRenderer>();
        bassRenderer.sprite = bassClefSprite;
        bassRenderer.color = staffColor;
        bassClefObject.transform.localPosition = new Vector3(-staffWidth / 2 + 3*clefScale, bassStaffCenter, -0.1f);
        bassClefObject.transform.localScale = new Vector3(clefScale, clefScale, 1f);
    }

    public void DisplayNote(MusicNote note)
    {
        // Remove previous note and ledger lines
        ClearNote();

        // Calculate Y position based on staff position
        // Middle C (C4) is position 0, which is between the two staves
        float yPos = CalculateNoteYPosition(note.StaffPosition);

        // Create new note
        currentNoteObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        currentNoteObject.name = "CurrentNote";
        currentNoteObject.transform.SetParent(transform);
        currentNoteObject.transform.localPosition = new Vector3(0, yPos, -0.1f);
        currentNoteObject.transform.localScale = new Vector3(noteSize, noteSize, noteSize);
        
        // Set note color
        Renderer renderer = currentNoteObject.GetComponent<Renderer>();
        renderer.material.color = noteColor;

        // Add ledger lines if needed
        AddLedgerLinesIfNeeded(note.StaffPosition, yPos);
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

    public void ClearNote()
    {
        if (currentNoteObject != null)
        {
            Destroy(currentNoteObject);
            currentNoteObject = null;
        }

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
}
