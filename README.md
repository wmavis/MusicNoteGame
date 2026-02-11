# Music Note Learning Game

A Unity game designed to help players learn musical notes on the staff.

## Game Concept
- Random musical notes appear on a staff
- Player clicks the correct note name from multiple choice options
- Score increases for correct answers
- Visual and audio feedback for learning

## Setup Instructions

1. Open Unity Hub
2. Click "New Project"
3. Select "2D" template
4. Set project name to "MusicNoteGame"
5. Set location to: `C:\Users\WillardPC\Desktop\MusicNoteGame`
6. Click "Create Project"

## After Unity Creates the Project

1. Copy all the scripts from the `Scripts` folder into your Unity project's `Assets/Scripts` folder
2. Open the MainGame scene in Unity
3. Follow the hierarchy setup instructions in SETUP_GUIDE.md

## Project Structure

```
Assets/
├── Scenes/
│   └── MainGame.unity
├── Scripts/
│   ├── GameManager.cs
│   ├── MusicNote.cs
│   ├── StaffRenderer.cs
│   └── UIManager.cs
```

## How to Play

1. A musical note appears on the staff
2. Click one of the note name buttons (C, D, E, F, G, A, B)
3. Get immediate feedback on your answer
4. Your score increases with each correct answer
5. Try to get the highest score possible!

## Features

- Randomly generated notes on a musical staff
- Multiple choice note identification
- Score tracking
- Visual feedback for correct/incorrect answers
- Clean, educational UI
