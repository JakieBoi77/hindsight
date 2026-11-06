# Traceability between code files and design modules

The Module Guide is not written yet, so this table traces the PoC code to the
SRS requirements and the design documents
([GameDesign.md](../GameDesign.md), [EducationalGameResearch.md](../EducationalGameResearch.md)).
Paths are under `EyeCanDo/Assets/_Project/Scripts/`.

| Code | Provides | Requirements and design |
| --- | --- | --- |
| `Core/Games/DropCatchRound.cs` | Drop Catch rules: catches, golden drops, blink bonus; a miss costs nothing | FR_NoPenalty, ULC_NoFail; GameDesign §5 (Stop 4 challenge) |
| `Core/Games/BlurrySpyRound.cs` | Blurry Spy rules: the blur clears with each find; wrong taps cost only the third star | FR_DescribeSensations (blurry and bright after drops), FR_NoPenalty |
| `Core/Games/GameLevels.cs`, `Core/Progress/PlayerProgress.cs` (`GameRecord`) | Three open levels, stars for finishing, personal bests that only go up | GameDesign §6, §7 (My Best); thm_NoLoss, FR_NoDuplicateReward |
| `Core/Avatar/AvatarCatalog.cs`, `AvatarLook.cs`, `Core/Progress/CoinRules.cs` | Character choices and shop prices; coins for the story and rounds | GameDesign §7 (characters, outfits, coins and shop) |
| `Core/Interaction/StillHold.cs` | Look up and hold still; letting go only pauses | FR_NoPenalty; research FR_PracticeActions |
| `Core/Progress/PlayerProgress.cs` | One player's save: look, coins, owned items, story status, sticker, game records; games unlock after the story | FR_FirstReward, FR_ExitKeepsData, NFR_Minimization |
| `Core/Progress/ProgressFile.cs`, `SaveSlots.cs` | Atomic save; damaged-data recovery; three save files, delete one or all | FR_PersistState, FR_CorruptData, FR_ResetJourney, FR_ClearAllData |
| `Core/Progress/GameSettings.cs` | Device settings shared by every save: volume, text size and speed, auto-continue, reduce motion | FR_Preferences, NFR_Accessibility |
| `Runtime/Menus/SaveSelectScreen.cs` | Pick, start or delete a save file (asks first) | FR_ExitKeepsData, FR_ResetJourney |
| `Runtime/Menus/SettingsPanel.cs`, `Runtime/UI/ConfirmDialog.cs`, `VolumeSlider.cs` | Settings applied at once; reset asks twice and states what is deleted. No parental gate in the PoC | FR_Preferences, FR_ResetJourney, FR_ClearAllData (FR_ParentGate deferred) |
| `Core/Interaction/InteractionRules.cs` | Input gate, hold-to-skip, idle hints | FR_IgnoreRepeatedTouches, FR_HintOnMiss |
| `Core/Content/ContentRules.cs`, `Editor/ContentValidator.cs` | Caption and story checks | NFR_Language, FR_DescribeSensations; research NFR_WordChoice |
| `Runtime/UI/Stage.cs` | Shared layout: a play area that is always visible, a caption strip, chrome in the safe area | NFR_Layout, NFR_TouchTarget |
| `Runtime/UI/CaptionBar.cs`, `Runtime/Content/CaptionLine.cs` | Captions standing in for narration: speaker face, pictogram, audio slot | FR_SpokenInstructions (intent), FR_NarrationFallback |
| `Runtime/UI/GhostHand.cs` | Demonstrations of each gesture | FR_SpokenInstructions, FR_HintOnMiss |
| `Runtime/UI/LevelChrome.cs` | Home, Restart, hold-to-skip in the same place on every screen | NFR_Exit, NFR_Layout, FR_RestartLevel, FR_ExitLevel |
| `Runtime/UI/TouchTargetScaler.cs`, `SafeAreaFitter.cs` | Minimum 15 mm targets; notch-safe layout | NFR_TouchTarget |
| `Runtime/UI/StarRow.cs`, `Confetti.cs` | Filled vs outlined stars; gentle celebrations | NFR_Colour, NFR_Flashing |
| `Runtime/App/Sfx.cs`, `SoundLibrary.cs`, `Tools/SoundFetch/fetch_sounds.py` | CC0 sound effects at the volume in the settings | GameDesign §8.5 (feedback) |
| `Runtime/App/GameServices.cs`, `AppRoot.cs` | Saving, scene fades, pause on interruption | FR_Suspend, FR_PersistState |
| `Runtime/Title/TitleScreen.cs` | Title screen, Play and Settings | GameDesign §7 (characters) |
| `Runtime/DressUp/DressUpScreen.cs`, `Runtime/UI/CoinCounter.cs` | Character maker and shop (try on, buy); the coin purse | GameDesign §7 |
| `Runtime/Map/ClinicMap.cs`, `StopPanel.cs` | Clinic map in visit order; coming-soon stops; games locked until the story is done | FR_JourneyMap, FR_StartLevel; GameDesign §3, §8.2 |
| `Runtime/Story/EyeDropsStory.cs`, `Runtime/Content/StoryScript.cs` | The Special Drops story: explain, quiz, honest sensations, watch Mia, your turn, sticker | FR_LevelContent, FR_DescribeSensations, FR_ClinicsDiffer, FR_SpeakUp, FR_FirstReward; research §8.3 |
| `Runtime/Games/*` | Level picker, one-tap instruction and 3-2-1 countdown, HUD, results; Drop Catch and Blurry Spy | GameDesign §5–7 |
| `Runtime/Characters/*`, `Runtime/Art/*`, `Shaders/UIRecolor.shader`, `Tools/ArtGen/*` | Character rigs and avatar heads, eye close-up, exam chair, original SVG art | NFR_ChildSafe, NFR_Licensing; GameDesign §8.5 |
