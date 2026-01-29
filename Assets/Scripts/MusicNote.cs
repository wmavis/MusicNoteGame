using UnityEngine;

/// <summary>
/// Represents a musical note with its properties
/// </summary>
public class MusicNote
{
    public enum NoteName
    {
        C, D, E, F, G, A, B
    }

    public NoteName Name { get; private set; }
    public int StaffPosition { get; private set; } // Position on the staff (0 = bottom line)

    // Staff positions for treble clef
    // 0 = E (bottom line), 1 = F, 2 = G, 3 = A, 4 = B (middle line), 5 = C, 6 = D, 7 = E, 8 = F (top line)
    private static readonly NoteName[] staffPositionToNote = new NoteName[]
    {
        NoteName.E, // 0 - bottom line
        NoteName.F, // 1
        NoteName.G, // 2 - second line
        NoteName.A, // 3
        NoteName.B, // 4 - middle line
        NoteName.C, // 5
        NoteName.D, // 6 - fourth line
        NoteName.E, // 7
        NoteName.F  // 8 - top line
    };

    public MusicNote(int staffPosition)
    {
        StaffPosition = staffPosition;
        Name = staffPositionToNote[staffPosition];
    }

    public static MusicNote GenerateRandomNote()
    {
        int randomPosition = Random.Range(0, staffPositionToNote.Length);
        return new MusicNote(randomPosition);
    }

    public override string ToString()
    {
        return Name.ToString();
    }
}
