using System.Collections.Generic;
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
    public int Octave { get; private set; } // 0-8 for piano range
    public int StaffPosition { get; private set; } // Position relative to Middle C
    public bool BassClef { get; private set; } // Is the note in the bass or treble clef?

    // All 52 natural piano keys (A0 to C8)
    // Subset: 29 notes: 4 below bass clef - C2 (index 9), 2 above bass middle C4 - E4 (25),
    //                   2 below treble middle C4 - A3 (21), 4 above treble clef - C6 (37)
    //                   Middle C (23)
    private static readonly (NoteName name, int octave)[] allPianoKeys = new (NoteName, int)[]
    {
        // Octave 0 (0-1)
        (NoteName.A, 0), (NoteName.B, 0),
        // Octave 1 (2-8)
        (NoteName.C, 1), (NoteName.D, 1), (NoteName.E, 1), (NoteName.F, 1), (NoteName.G, 1), (NoteName.A, 1), (NoteName.B, 1),
        // Octave 2 (9-15)
        (NoteName.C, 2), (NoteName.D, 2), (NoteName.E, 2), (NoteName.F, 2), (NoteName.G, 2), (NoteName.A, 2), (NoteName.B, 2),
        // Octave 3 (16-22)
        (NoteName.C, 3), (NoteName.D, 3), (NoteName.E, 3), (NoteName.F, 3), (NoteName.G, 3), (NoteName.A, 3), (NoteName.B, 3),
        // Octave 4 (Middle C is C4) (23-29)
        (NoteName.C, 4), (NoteName.D, 4), (NoteName.E, 4), (NoteName.F, 4), (NoteName.G, 4), (NoteName.A, 4), (NoteName.B, 4),
        // Octave 5 (30-36)
        (NoteName.C, 5), (NoteName.D, 5), (NoteName.E, 5), (NoteName.F, 5), (NoteName.G, 5), (NoteName.A, 5), (NoteName.B, 5),
        // Octave 6 (37-43)
        (NoteName.C, 6), (NoteName.D, 6), (NoteName.E, 6), (NoteName.F, 6), (NoteName.G, 6), (NoteName.A, 6), (NoteName.B, 6),
        // Octave 7 (44-50)
        (NoteName.C, 7), (NoteName.D, 7), (NoteName.E, 7), (NoteName.F, 7), (NoteName.G, 7), (NoteName.A, 7), (NoteName.B, 7),
        // Octave 8 (51)
        (NoteName.C, 8)
    };

    public MusicNote(NoteName name, int octave)
    {
        Name = name;
        Octave = octave;
        StaffPosition = CalculateStaffPosition(name, octave);
        if (StaffPosition > 2) // Treble only
        {
            BassClef = false;
        }
        else if(StaffPosition < -2) // Bass only
        {
            BassClef = true;
        }
        else // Either treble or bass (pick at random)
        {
            BassClef = Random.Range(0, 1) == 0;
        }
    }

    private int CalculateStaffPosition(NoteName name, int octave)
    {
        // Calculate position relative to Middle C (C4) = 0
        // Each note is a half-step, but we only care about natural notes
        // C=0, D=1, E=2, F=3, G=4, A=5, B=6 within an octave
        
        int noteValue = name switch
        {
            NoteName.C => 0,
            NoteName.D => 1,
            NoteName.E => 2,
            NoteName.F => 3,
            NoteName.G => 4,
            NoteName.A => 5,
            NoteName.B => 6,
            _ => 0
        };

        // Calculate position: (octave - 4) * 7 + noteValue
        // Middle C (C4) = (4-4)*7 + 0 = 0
        return (octave - 4) * 7 + noteValue;
    }

    public static MusicNote GenerateRandomNote()
    {
        return GenerateRandomNote(0, allPianoKeys.Length);
    }

    public static MusicNote GenerateRandomNote(int firstIndex, int lastIndex)
    {
        int randomIndex = Random.Range(firstIndex, lastIndex);
        var (name, octave) = allPianoKeys[randomIndex];
        return new MusicNote(name, octave);
    }

    public static List<MusicNote> GetAllNotes()
    {
        List<MusicNote> allNotes = new List<MusicNote>();

        for (int i = GameManager.firstNoteIndex; i <= GameManager.lastNoteIndex; i++)
        {
            var (name, octave) = allPianoKeys[i];
            allNotes.Add(new MusicNote(name, octave));

        }

        return allNotes;
    }

    public string GetFullName()
    {
        return $"{Name}{Octave}";
    }

    public override string ToString()
    {
        return Name.ToString();
    }
}
