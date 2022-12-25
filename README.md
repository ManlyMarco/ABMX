# Advanced Bone Modifier eXtended (BonemodX / ABMX)
Plugin that adds more character customization settings to character maker of various games made by Illusion. These additional settings are saved inside the card and used by the main game and studio. It is possible to change male height, make good-looking thick necks, customize skirts, adjust hand and feet size and much more. Supported games:
- PlayHome (PHABMX)
- Koikatu / Koikatsu Party (KKABMX)
- Emotion Creators (ECABMX)
- AI-Shoujo / AI-Syoujyo (AIABMX)
- HoneySelect2 (HS2ABMX)
- Koikatsu Sunshine (KKSABMX)

## How to use 
1. Make sure that you have at least BepInEx 5.0 and the latest [Modding API](https://github.com/ManlyMarco/KKAPI) for your game installed, and your game is updated.
2. Download the latest release for your game from [here](https://github.com/ManlyMarco/KKABMX/releases/latest). You only need the version specific for your game.
3. Extract the release to your game directory. The dll file should end up inside BepInEx\plugins folder in your game's directory.
4. Start character maker, you should see new settings and categories show up. ABMX settings are highlighted in yellow. You can turn on the advanced window in bottom right in KK/EC and in plugin settings in AI (press F1).

## Changes from original ABM
This is an upgraded version of the original Koikatu ABM/Bonemod plugin. It fixes multiple issues with the original while adding new features.
- Added bone position and rotation adjustments
- Added controls integrated into character maker interface
- Added advanced editing UI with bone searching and other creature comforts
- Added ability to have different skirt bone settings for each outfit type
- Improved performance, no more stuttering or FPS drops
- No more .txt files, everything is saved into the cards themselves (will still read .txt files and delete them after re-saving the card)
- Refactoring and cleanup of the source code, a lot of useless fat trimmed
- And more...

## Screenshots
![KK maker GUI](https://user-images.githubusercontent.com/39247311/48379581-e6891980-e6d4-11e8-8253-21feed5ac6cb.png)
![AI maker GUI](https://user-images.githubusercontent.com/39247311/65235718-79ee6080-dad7-11e9-87ff-366ef4d5101a.PNG)
![Advanced window in KK maker](https://user-images.githubusercontent.com/39247311/209483144-905d04d3-1115-4d13-a60e-2f33b819f56e.png)
