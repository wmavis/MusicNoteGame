# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity educational game for learning musical notes on the grand staff. The game displays random notes across the full 88-key piano range and challenges players to identify them using multiple-choice buttons.

## Unity Project Configuration

- **Unity Version**: 2020.3 or newer recommended
- **Template**: 2D
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Required Package**: TextMesh Pro (TMP)

## Development Workflow

Since this is a Unity project, most development happens in the Unity Editor. Code changes alone are insufficient - Unity scenes, prefabs, and project settings must be configured in the Unity Editor.

### Testing Changes

Unity projects cannot be tested from the command line in a typical way. To test:

1. Open the project in Unity Editor (Unity Hub)
2. Open the MainGame scene (`Assets/Scenes/MainGame.unity`)
3. Click the Play button in Unity Editor
4. Interact with the game in the Game view

### Making Code Changes

When modifying C# scripts:

1. Edit the `.cs` files in `Assets/Scripts/`
2. Unity will automatically recompile when you return to the Editor
3. Check the Unity Console (Window > General > Console) for compilation errors
4. Test in Play mode

### Scene Configuration

The game requires specific Unity scene setup (documented in SETUP_GUIDE.md). Key scene objects:

- **GameManager** - Empty GameObject with GameManager.cs script
- **StaffRenderer** - Empty GameObject with StaffRenderer.cs script
- **UIManager** - Empty GameObject with UIManager.cs script
- **Canvas** - UI Canvas with Score, Feedback, and 7 Note Buttons

All scene objects and their connections must be configured in the Unity Inspector.

## Code Architecture

### Core Components

The game uses a classic Unity component-based architecture with 4 main scripts:

#### GameManager.cs (Game Controller)
- **Purpose**: Orchestrates game flow and core logic
- **Key Responsibilities**:
  - Runs the main game loop as a coroutine
  - Generates random notes via MusicNote.GenerateRandomNote()
  - Validates player answers against current note
  - Manages score state
  - Coordinates between StaffRenderer and UIManager
- **Design Pattern**: Central controller/coordinator
- **Inspector References**: Requires StaffRenderer and UIManager assignments

#### MusicNote.cs (Data Model)
- **Purpose**: Represents musical note data and logic
- **Key Features**:
  - Enum-based note names (C, D, E, F, G, A, B)
  - Octave support (0-8 for full piano range)
  - Staff position calculation relative to Middle C (C4 = position 0)
  - Automatic clef determination (treble/bass based on position)
  - Static array of all 52 natural piano keys (A0 to C8)
- **Design Pattern**: Value object with factory method (GenerateRandomNote)
- **Note**: Pure data class with no MonoBehaviour dependency

#### StaffRenderer.cs (Visual Renderer)
- **Purpose**: Renders the grand staff and notes in game world space
- **Key Features**:
  - Creates 10 staff lines (5 treble + 5 bass) using LineRenderer components
  - Dynamically generates ledger lines for notes outside staves
  - Positions notes based on Middle C reference point
  - Manages clef symbols (treble and bass)
- **Design Pattern**: Renderer component
- **Visual Implementation**: Uses Unity LineRenderer for staff lines, sphere primitives for notes

#### UIManager.cs (UI Controller)
- **Purpose**: Manages all UI elements and user input
- **Key Responsibilities**:
  - Sets up 7 note buttons (C through B) with click handlers
  - Updates score display (TextMeshPro)
  - Shows timed feedback messages (correct/incorrect with colors)
  - Controls button interactivity during answer validation
- **Design Pattern**: UI facade/controller
- **Dependencies**: Requires TextMeshPro and Unity UI Button components

### Component Communication Flow

```
User Input → UIManager → GameManager → StaffRenderer
                              ↓
                         MusicNote (data)
                              ↓
                         UIManager (feedback)
```

1. **Game Loop**: GameManager generates MusicNote → passes to StaffRenderer.DisplayNote()
2. **Player Input**: UIManager captures button click → calls GameManager.CheckAnswer()
3. **Validation**: GameManager compares answer with currentNote → calls UIManager.ShowCorrectFeedback() or ShowIncorrectFeedback()
4. **Next Note**: GameManager waits for delay → loops back to step 1

### Key Design Decisions

- **Position System**: All note positioning uses Middle C (C4) as origin (position 0). Each note name increment = 0.5 lineSpacing units.
- **Staff Spacing**: Grand staff has configurable gap between treble/bass (default 2f), line spacing (default 0.5f).
- **Coroutine Game Loop**: Main game loop uses coroutines for timing control, with `waitingForAnswer` flag to pause between notes.
- **Note Range**: Supports full 88-key piano (52 natural keys), but buttons only identify letter names (C-G), not octaves.
- **UI Feedback**: Auto-dismissing feedback system using Update() timer rather than coroutines for simpler state management.

## Common Modifications

### Changing Note Range
Edit `MusicNote.cs` → modify the `allPianoKeys` array to add/remove notes.

### Adjusting Game Speed
Edit `GameManager.cs` → change `delayBeforeNextNote` field (default 2f seconds).

### Customizing Visuals
Edit `StaffRenderer.cs` → modify staff colors, line thickness, note size in Inspector-visible fields.

### Adding Sharp/Flat Support
Would require:
1. Extending `MusicNote.NoteName` enum to include accidentals
2. Updating button count in UIManager (currently hardcoded for 7 natural notes)
3. Modifying StaffRenderer to render accidental symbols

## Git and Version Control

This project includes Unity-generated files. When committing:

- **Include**: Assets/, ProjectSettings/, Packages/manifest.json
- **Exclude**: Library/, Temp/, Logs/, UserSettings/, .vs/
- Unity scenes and assets are serialized as YAML text files for git-friendly diffs
- Be cautious with .meta files - Unity requires them for asset tracking

## Build and Distribution

The game is configured for WebGL builds (v1.1). To build:

1. In Unity: File → Build Settings
2. Select WebGL platform
3. Click "Build" or "Build and Run"
4. Output will be in the build directory

Note: C# scripts cannot be "run" directly - they must be executed through Unity's runtime.
