using UnityEngine;

/// <summary>
/// Renders the musical staff and note on it
/// </summary>
public class StaffRenderer : MonoBehaviour
{
    [Header("Staff Settings")]
    [SerializeField] private float staffWidth = 8f;
    [SerializeField] private float lineSpacing = 0.5f;
    [SerializeField] private Color staffColor = Color.black;
    [SerializeField] private float lineThickness = 0.05f;

    [Header("Note Settings")]
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Color noteColor = Color.black;
    [SerializeField] private float noteSize = 0.6f;

    private GameObject currentNoteObject;
    private LineRenderer[] staffLines;

    void Start()
    {
        CreateStaff();
    }

    void CreateStaff()
    {
        // Create 5 horizontal lines for the staff
        staffLines = new LineRenderer[5];
        
        for (int i = 0; i < 5; i++)
        {
            GameObject lineObj = new GameObject($"StaffLine_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = staffColor;
            line.endColor = staffColor;
            line.startWidth = lineThickness;
            line.endWidth = lineThickness;
            line.positionCount = 2;
            
            float yPos = i * lineSpacing;
            line.SetPosition(0, new Vector3(-staffWidth / 2, yPos, 0));
            line.SetPosition(1, new Vector3(staffWidth / 2, yPos, 0));
            
            staffLines[i] = line;
        }

        // Center the staff
        transform.position = new Vector3(0, 0, 0);
    }

    public void DisplayNote(MusicNote note)
    {
        // Remove previous note if exists
        if (currentNoteObject != null)
        {
            Destroy(currentNoteObject);
        }

        // Create new note
        currentNoteObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        currentNoteObject.name = "CurrentNote";
        currentNoteObject.transform.SetParent(transform);
        
        // Position the note based on staff position
        float yPos = (note.StaffPosition * lineSpacing) / 2f;
        currentNoteObject.transform.localPosition = new Vector3(0, yPos, -0.1f);
        currentNoteObject.transform.localScale = new Vector3(noteSize, noteSize, noteSize);
        
        // Set note color
        Renderer renderer = currentNoteObject.GetComponent<Renderer>();
        renderer.material.color = noteColor;

        // Add ledger lines if needed (for notes above or below the staff)
        AddLedgerLinesIfNeeded(note.StaffPosition);
    }

    private void AddLedgerLinesIfNeeded(int staffPosition)
    {
        // Ledger lines for notes below the staff (position < 0)
        // Ledger lines for notes above the staff (position > 8)
        // For simplicity, we're keeping notes within the standard staff range (0-8)
        // This can be expanded later if needed
    }

    public void ClearNote()
    {
        if (currentNoteObject != null)
        {
            Destroy(currentNoteObject);
            currentNoteObject = null;
        }
    }
}
