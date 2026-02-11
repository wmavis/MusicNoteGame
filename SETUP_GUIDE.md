# Unity Setup Guide for Music Note Learning Game

## Step 1: Create Unity Project

1. Open **Unity Hub**
2. Click **"New Project"**
3. Select **"2D"** template
4. Set **Project Name**: `MusicNoteGame`
5. Set **Location**: `C:\Users\WillardPC\Desktop\MusicNoteGame`
6. Click **"Create Project"**

Unity will create the project and open the Unity Editor.

---

## Step 2: Import Scripts

1. In Unity, locate the **Project** window (usually at the bottom)
2. Right-click in the **Assets** folder
3. Select **Create > Folder** and name it `Scripts`
4. Copy all `.cs` files from `C:\Users\WillardPC\Desktop\MusicNoteGame\Scripts\` into the `Assets/Scripts` folder in Unity

The scripts you should have:
- `GameManager.cs`
- `MusicNote.cs`
- `StaffRenderer.cs`
- `UIManager.cs`

---

## Step 3: Install TextMeshPro

1. In Unity, go to **Window > TextMeshPro > Import TMP Essential Resources**
2. Click **Import** in the dialog that appears
3. Wait for the import to complete

---

## Step 4: Create the Game Scene

### A. Create Main Camera Setup
1. In the **Hierarchy** window, select the **Main Camera**
2. In the **Inspector**, set:
   - **Position**: X=0, Y=2, Z=-10
   - **Background**: Choose a light color (e.g., light blue or white)
   - **Size** (if Orthographic): 5

### B. Create Game Manager Object
1. Right-click in **Hierarchy** > **Create Empty**
2. Rename it to `GameManager`
3. With `GameManager` selected, click **Add Component** in Inspector
4. Search for and add the `GameManager` script

### C. Create Staff Renderer Object
1. Right-click in **Hierarchy** > **Create Empty**
2. Rename it to `StaffRenderer`
3. Set **Position**: X=0, Y=2, Z=0
4. Click **Add Component** and add the `StaffRenderer` script

### D. Create UI Canvas
1. Right-click in **Hierarchy** > **UI > Canvas**
2. A Canvas and EventSystem will be created automatically
3. Select the **Canvas** and in Inspector, set:
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler > UI Scale Mode**: Scale With Screen Size
   - **Reference Resolution**: 1920 x 1080

### E. Create Score Text
1. Right-click on **Canvas** > **UI > Text - TextMeshPro**
2. Rename it to `ScoreText`
3. In the **Rect Transform**:
   - Click the anchor preset (top-left square icon)
   - Hold **Alt+Shift** and click **top-left** anchor
   - Set **Pos X**: 100, **Pos Y**: -50
4. In **TextMeshPro** component:
   - **Text**: "Score: 0"
   - **Font Size**: 36
   - **Color**: Black
   - **Alignment**: Left

### F. Create Feedback Text
1. Right-click on **Canvas** > **UI > Text - TextMeshPro**
2. Rename it to `FeedbackText`
3. In the **Rect Transform**:
   - Click anchor preset and select **top-center**
   - Set **Pos Y**: -100
   - Set **Width**: 600, **Height**: 100
4. In **TextMeshPro** component:
   - **Text**: "" (leave empty)
   - **Font Size**: 32
   - **Alignment**: Center
   - **Color**: Green

### G. Create Note Buttons Panel
1. Right-click on **Canvas** > **UI > Panel**
2. Rename it to `ButtonPanel`
3. In **Rect Transform**:
   - Click anchor preset and select **bottom-center**
   - Set **Pos Y**: 100
   - Set **Width**: 1000, **Height**: 150
4. In **Image** component, set **Color** to a semi-transparent color (e.g., white with alpha 0.3)

### H. Create Note Buttons (C, D, E, F, G, A, B)
For each of the 7 notes, create a button:

1. Right-click on **ButtonPanel** > **UI > Button - TextMeshPro**
2. Rename to `Button_C` (then D, E, F, G, A, B for subsequent buttons)
3. Position them horizontally across the panel:
   - **Button_C**: Pos X = -450
   - **Button_D**: Pos X = -300
   - **Button_E**: Pos X = -150
   - **Button_F**: Pos X = 0
   - **Button_G**: Pos X = 150
   - **Button_A**: Pos X = 300
   - **Button_B**: Pos X = 450
   - All buttons: Pos Y = 0, Width = 120, Height = 100
4. For each button, expand it in Hierarchy and select the **Text (TMP)** child
5. Set the text to the note name (C, D, E, F, G, A, B)
6. Set **Font Size**: 48
7. Set **Color**: Black

---

## Step 5: Create UI Manager Object

1. Right-click in **Hierarchy** > **Create Empty**
2. Rename it to `UIManager`
3. Click **Add Component** and add the `UIManager` script
4. In the **Inspector**, you'll see the UIManager component with empty fields:
   - **Score Text**: Drag the `ScoreText` object from Hierarchy here
   - **Feedback Text**: Drag the `FeedbackText` object here
   - **Note Buttons**: Set **Size** to 7, then drag each button (Button_C through Button_B) into the array slots

---

## Step 6: Connect GameManager References

1. Select the **GameManager** object in Hierarchy
2. In the **Inspector**, find the GameManager script component:
   - **Staff Renderer**: Drag the `StaffRenderer` object here
   - **UI Manager**: Drag the `UIManager` object here

---

## Step 7: Save and Test

1. Go to **File > Save As**
2. Name the scene `MainGame` and save it in `Assets/Scenes/`
3. Click the **Play** button at the top of the Unity Editor
4. You should see:
   - A musical staff with 5 lines
   - A note appearing on the staff
   - Seven buttons (C, D, E, F, G, A, B) at the bottom
   - Score display at the top
5. Click the correct note name button to test the game!

---

## Troubleshooting

### If the staff doesn't appear:
- Make sure the Main Camera can see the StaffRenderer (check Z positions)
- Check that the StaffRenderer script is attached and has no errors

### If buttons don't work:
- Make sure all 7 buttons are assigned in the UIManager's Note Buttons array
- Check that the EventSystem exists in the Hierarchy

### If text doesn't show:
- Make sure TextMeshPro is imported
- Check that ScoreText and FeedbackText are assigned in UIManager

### If you see script errors:
- Make sure all 4 scripts are in the Assets/Scripts folder
- Check the Console window (Window > General > Console) for specific errors

---

## Customization Ideas

Once the game is working, you can customize:
- **Colors**: Change button colors, staff color, background
- **Difficulty**: Modify the note range in MusicNote.cs
- **Speed**: Adjust `delayBeforeNextNote` in GameManager
- **Visuals**: Replace the sphere note with a sprite
- **Sound**: Add audio feedback for correct/incorrect answers

Enjoy learning musical notes!
