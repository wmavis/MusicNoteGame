# Changelog

All notable changes to the Music Note Learning Game project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-02-03

### Added
- **Grand Staff Support** - Replaced single treble clef with full grand staff (treble and bass clefs)
- **88-Key Piano Range** - Support for all 52 natural piano keys from A0 to C8
- **Clef Symbols** - Added treble and bass clef SVG symbols for visual clarity
- **Dynamic Ledger Lines** - Automatically renders ledger lines for notes outside the staff
- **Octave Information** - Displays full note name with octave (e.g., "C4", "A0") in feedback messages
- **Enhanced Note System** - Notes now include octave and clef type properties

### Changed
- **Expanded Note Range** - From 9 positions (E4-F5) to 52 natural keys across full piano range
- **Staff Rendering** - Now renders 10 staff lines (5 treble + 5 bass) with proper spacing
- **Note Positioning** - Uses Middle C (C4) as reference point for calculating note positions
- **Feedback Messages** - Enhanced to show both note letter and octave for educational purposes
- **MusicNote Class** - Complete rewrite to support octave-based note generation
- **StaffRenderer Class** - Major update to support grand staff, clef symbols, and ledger lines

## [1.0.0] - 2026-01-29

### Added

#### Core Game Scripts
- **GameManager.cs** - Main game logic and state management for the music note learning game
- **MusicNote.cs** - Musical note data structure and behavior implementation
- **StaffRenderer.cs** - Renders the musical staff using Unity's line rendering
- **UIManager.cs** - Manages user interface elements, buttons, and score display

#### Game Features
- Random musical note generation on a staff
- Multiple choice note identification system (C, D, E, F, G, A, B)
- Score tracking system
- Visual feedback for correct/incorrect answers
- Educational UI designed for learning

#### Unity Project Setup
- **MainGame scene** - Complete game scene with configured hierarchy
- **Universal Render Pipeline (URP)** - 2D rendering pipeline configuration
  - Default Volume Profile
  - URP Global Settings
  - 2D Renderer asset
- **TextMesh Pro** - Integrated for high-quality UI text rendering
  - LiberationSans font with SDF materials
  - Emoji support
  - Custom shaders and materials
- **Input System** - Modern Unity Input System configuration
- **2D Scene Template** - Custom scene template for URP 2D projects

#### Documentation
- **README.md** - Project overview, game concept, setup instructions, and features
- **SETUP_GUIDE.md** - Detailed setup guide for Unity project configuration
- **QUICK_START.md** - Quick reference guide for getting started

#### Project Configuration
- **.gitignore** - Unity-specific gitignore configuration
- **.vsconfig** - Visual Studio integration configuration
- **Unity Project Settings** - Complete project settings including:
  - Audio, Physics, Graphics, and Quality settings
  - Input Manager configuration
  - Tag and Layer setup
  - URP-specific settings
- **Package Dependencies** - Unity package manifest with required packages

[1.0.0]: https://github.com/wmavis/MusicNoteGame/commit/7f17a3188e3b71dcef64ffffec9efbf69f325785
