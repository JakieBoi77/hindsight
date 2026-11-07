# Source Code

| Path | What it is |
|---|---|
| `OphthalmologyGame/` | Unity 6 (6000.6.2f1) 3D proof of concept for the ophthalmology clinic game. Targets iOS and Android tablets. |
| `TraceToDesign.md` | Traceability between code and design modules. |

## Ophthalmology Game PoC v2

This version rebuilds the 3D Eye Drops level as a **narrated storyboard with few words**, following [PoCResearch.md](../PoCResearch.md). It also adds the save files, coins, dress-up shop and settings from the 2D cosmetics PoC.

Title → **save files** → level select (coins, **Dress up**) → **Eye Drops**: 14 voiced panels, then Home / Play again.

| # | Panel | What the child does |
|---|---|---|
| 1 | Dr. Iris says hi | Watch (she waves) |
| 2 | Meet Sam, who is getting drops too | Tap Sam: he cheers and says "Hi! I'm Sam, nice to meet you." |
| 3 | Dr. Iris and Sam walk to the drops chart; each picture lights up as she explains it (drops go in, the pupil gets big, she can see inside) | Watch |
| 4 | "Sam, you're up first." Sam: "Will do!" and walks to the chair | Watch |
| 5 | Sam in the chair, head back: the drops are cold and may sting | Watch |
| 6 | The drop lands. Sam: "Oooh, cold!" Dr. Iris: blink to spread it. Sam blinks | Watch |
| 7 | Dr. Iris looks into Sam's eye with her light: "Looking good, Sam!" | Watch |
| 8 | Sam's happy dance ("Yay, that wasn't so bad!"), Dr. Iris: "You're all set!", then he walks out of the door | Watch |
| 9 | Your turn: the player's own kid stands by the chair | Tap the chair |
| 10 | First person, head back, looking up at the smiley sun painted on the ceiling. Dr. Iris explains first; only when she says "Hold the button to keep your eye open!" do the button and the bottle appear | Press and hold |
| 11 | The drop falls. Letting go only pauses it, and Dr. Iris repeats "Make sure to keep your head back and eyes open." | Keep holding |
| 12 | "You did it! Now blink!" | Tap blink 3 times |
| 13 | Dr. Iris takes a quick look inside: "Fantastic! Your eyes look great!" | Watch |
| 14 | Everyone dances, the Brave Eyes sticker pops up and the coins fly into the purse | Home or Play again |

- **Voiced lines.** Every line is spoken, and Dr. Iris and Sam have their own voices (see Voices). Each line has:
  - a caption of 8 words or fewer, which can be turned off;
  - a replay button;
  - the speaker's portrait: a live render of their 3D model, mouth moving.

  There are no quizzes and nothing can be failed.
- **Saves, coins and dress-up.** Three save slots, each with its own kid, coins, sticker and clothes. The level pays 40 coins the first time and 5 on replays. Dress up has:
  - **Free everyday clothes** for a trip to the doctor: T-shirt and jeans, raincoat, jacket.
  - **Dressing-up clothes to buy** (20 to 70 coins): outfits (Explorer, Adventurer, Viking, Knight armour), hats (cap, beanie, bow, party hat, wizard hat, bear hat, knight helmet, crown) and capes (travel, fur, wizard, hero).
  - **Free looks:** hairstyles (short, long, tidy), and every colour for skin, hair, shirt and shoes. Every hat fits every hairstyle.
  - **Spin the kid round** by dragging across them, to see the back.
- **Settings** (title, or the gear in a level, which pauses): sound volume, voice volume, captions, big text, move on by itself, less motion. **Reset game** asks twice.

### Open and run

1. In Unity Hub choose **Add → Add project from disk** and pick `src/OphthalmologyGame` (editor 6000.6.2f1, with the Android and/or iOS modules).
2. Open `Assets/_Project/Scenes/Menu.unity` and press Play. For a tablet-shaped view, switch the Game view to **Simulator** and pick an iPad or Android tablet.

Saves go to `Application.persistentDataPath/saves/save_1.json` to `save_3.json`, and settings to `settings.json` beside them.

### Voices

Every line in `Assets/_Project/Audio/Narration/<id>.wav` is cut from one ElevenLabs recording of the whole script, with Dr. Iris and Sam in their own voices. Each clip is an unbroken stretch of that recording, cut in the silence between lines; the audio itself is not changed (no fades, no effects). `p11_again` is the first sentence of `p10_hold`, reused.

- **Lines in the game use the recording's wording,** which differs a little from the first script (e.g. "You're going to do great!").
- **To replace a line,** save a new recording as `<id>.wav` in that folder and run **POC → Rebuild All**. `Audio/Narration/NarrationScript.md` lists every line's id, speaker and words, and `Tools/VoiceGen/ElevenLabs.md` has voice and delivery notes for ElevenLabs. If you save an `.mp3` instead, delete the old `.wav` first: the builder picks `.wav` first.
- **Every line must have a file:** the `EveryLineHasNarrationAudio` test fails otherwise.
- `Tools/VoiceGen/generate_voices.py` is the earlier generator (Chatterbox, open-source and local). It only fills in missing files unless run with `--force`, which would overwrite the recording.

**Pacing in the game.** After every line the speaker's mouth stops, but nothing moves on for about a second more (a little longer for long lines). Next, the next line, a camera cut or a scene change only comes after that pause, so children have time to take each line in.

### How it is organised

```
Assets/_Project/
  Scripts/Core/             levels, scene flow, GameServices (saves + settings)
  Scripts/Core/Progress/    engine-free, ported from the 2D PoC: DataFile, SaveSlots, PlayerProgress, GameSettings, CoinRules
  Scripts/Core/Avatar/      AvatarCatalog (choices and prices), AvatarLook
  Scripts/Procedure/        ProcedureDefinition, ProcedureStep, StoryLine (narration + caption + clip), ProcedureRunner
  Scripts/Procedure/Steps/  Panel, TapTarget, Chart, HoldStill, Blink, Exam, Reward (+ ShowCloseup, unused)
  Scripts/Scene/            ExamRoomContext (stage beats), ShotDirector, DoctorActor, KidActor (Sam + the player's kid),
                            TeachingChart, WaveIK, SpeakerPortraits, rigs
  Scripts/UI/               captions + NarrationPlayer, reward (coins fly to the purse), HUD, settings, confirm dialog,
                            saves, dress up + KidSpinner, menu, Sfx
  Scripts/DevTools/         PocAutoPilot (smoke test, only active with -autopilot)
  Editor/                   PocBuilder (+ .Ui) / PrefabBuilder / UiKit / CeilingArt / ChartArt (generate everything),
                            ShotCapture, DebugCapture, DebugLooks
  Audio/Narration/          one WAV per line (generated voices for now) + NarrationScript.md
  Audio/Sfx/                CC0 sound effects copied from the 2D PoC
  Data/                     level + step assets (generated: edit the lines in PocBuilder.BuildEyeDropsProcedure)
  Prefabs/                  ExamRoom, ExamChair, Doctor, Kid, EyeDropBottle, Ophthalmoscope, EyeModel
  Resources/                SoundLibrary
  Scenes/                   Menu, EyeDrops
  Tests/EditMode/           runner, progress/saves/shop, story content rules
Assets/ThirdParty/          downloaded models (see THIRD_PARTY_ASSETS.md)
Tools/VoiceGen/             generate_voices.py (Chatterbox + director), voices/ reference clips, lines.json
```

- **A panel is data.** `PanelStep` holds:
  - one shot and one `StoryLine`;
  - an optional stage beat (`SamSit`, `DropOnSam`, `SamDance`, `KidAppear`...), played by `ExamRoomContext.PlayBeat`;
  - an optional follow-up line, often the other character;
  - an optional beat after the lines (`SamWalkToChair`, `SamLeave`);
  - an optional glowing thing to discover.
- **The drops chart.** `ChartStep` walks Dr. Iris and Sam over while the camera pans, then lights each picture of the chart (`TeachingChart`) as she says its cue word. The time of each word is taken from its place in the line and the length of the recording. `ChartArt` paints the pictures.
- **Kids.** `Kid.prefab` is the KayKit Mage at child scale. Sam and the player's kid use the same prefab with different `AvatarLook`s.
  - **Colours** are painted at runtime into a per-kid copy of each atlas, and given to the renderers through a property block. (Per-kid material copies that differed only in their texture were drawn with each other's texture.)
  - **Outfits, hairstyles, hats and capes** come from all the KayKit Adventurers characters (Mage, Rogue, Knight, Barbarian). They share one skeleton, so the builder copies their bodies, arms, legs, heads, helmets and capes onto the kid's. Each is a child object named `<Slot>_<id>[_<part>]` (`Outfit_tee_Body`, `HairStyle_long`, `Hat_crown`, `Cape_hero`), and `KidActor.Apply` switches them on and off.
  - **Everyday outfits** are the same meshes painted differently (`PaintStyle` in `PrefabBuilder`): belts, straps and books take the shirt colour so they disappear, gloves become hands, trousers are jeans. The jacket mixes the knight's top with the T-shirt's jeans.
  - **Hats fit every hairstyle.** Each hat was made for one head. For the other hairstyles the builder measures the hair above the hat band and stores a raised and scaled pose per hat and hairstyle (`HatFit`).
  - **POC → Debug Looks** renders every outfit (front and back), every hat on every hairstyle (front and side) and the animation frames into `Captures/looks`.
  - **Sitting:** `KidActor.SitInSeat` puts the hips on the `SamSeat` anchor, leans the upper body back against the reclined backrest and lays the shins along the leg rest. **POC → Debug Captures** logs the chair's profile and renders the seat from the side.
- **Waving.** Dr. Iris waves with two-bone IK (`WaveIK`): her hand goes up beside her head and her forearm swings outward, never toward the head. The kids' arms are shorter than their heads are wide, so a procedural wave folds their hand onto the head. They greet with the pack's hand-animated Cheer clip instead. Runtime frame bursts in the autopilot (`anim_*` screenshots) are how the animation was checked.
- **Speaker portraits.** `SpeakerStudio` in the EyeDrops scene holds copies of Dr. Iris and Sam, filmed into render textures. The copies talk when their character speaks.
- **Camera shots** are the `CinemachineCamera` children of `Shots` in the EyeDrops scene. Steps refer to them by name.
- **UI style** comes from the 2D prototype (`UiKit`).

### Regenerating

**POC → Rebuild All** (`PocBuilder.BuildAll`) generates:
- the prefabs, step assets and scenes;
- the sound library and the ceiling picture;
- the narration script and `lines.json`.

It is safe to rerun, but it overwrites hand edits to generated files. To change dialogue for good, edit `PocBuilder.BuildEyeDropsProcedure` and regenerate the voices.

Headless equivalents (run from `src/OphthalmologyGame`, with `Unity` = the 6000.6.2f1 editor executable). **Do not add `-nographics`.** The SVG icons are rasterised on the GPU, and without one they import as grey squares. The builder logs an error if that happens.

```sh
Unity -batchmode -projectPath . -executeMethod Hindsight.Ophthalmology.EditorTools.PocBuilder.BuildAll -quit
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml
Unity -batchmode -projectPath . -buildTarget Android -executeMethod Hindsight.Ophthalmology.EditorTools.PocBuilder.BuildAndroid -quit
```

The EditMode tests include the automated checks from the research plan (`StoryContentTests`):
- every line has narration
- captions are 8 words or fewer, under 120 words in all
- 14 panels, ending on the reward
- no fail language ("not so bad" is allowed: it reassures)
- Sam goes before the player and has his own lines
- the chart's pictures light up in order, while the line is being said

### Smoke test

**POC → Build Windows (smoke test)**, then run `Builds/Windows/OphthalmologyGame.exe -screen-fullscreen 0 -autopilot <folder>`. It:
1. Picks Save 1 and plays the level: it taps Sam and the chair, and lets go once while the drop falls. It saves frame bursts of the chart, Sam walking, dancing and leaving, and the coins flying in.
2. Opens Dress up, spins the kid round and buys a hat.
3. Saves a screenshot of every state into `<folder>`, keeping its save data in `<folder>/data`.
4. Exits with code 0 if nothing logged an error and Save 1 ends with a sticker and a bought item.

### Known PoC shortcuts

- **Generated voices.** The narration is text-to-speech (see Voices), not recorded actors. It was checked automatically for words, shouting, pace and (for Sam) how childlike it sounds, and should still be listened to.
- **The storyboard grew.** The research plan asked for 12 panels; the agreed storyboard has 14, and its spoken lines are longer than the plan's (the captions still follow the 8-word rule). The script says "Click"; "Tap" may suit tablets better.
- **Mixed art.** It comes from free packs, so styles don't fully match.
  - The doctor is the KayKit "Rogue" with her tunic recoloured into a white coat.
  - The kids are built on the KayKit "Mage" skeleton, wearing parts of all four characters. Their faces keep the pack's stern eyebrows.
  - The barbarian's head has a beard, so it isn't offered as a hairstyle.
- **Simple accessories.** The cap, beanie, bow, party hat and crown are built from primitive shapes. The other hats and all the capes come from the pack.
- **Everyday clothes from fantasy meshes.** The T-shirt, raincoat and jacket are the pack's adventurer meshes repainted, so a belt or a strap still shows as a shape.
- **Text-only shop cards.** Dress-up item cards show the item's name, not a picture. Trying an item on shows it on the kid.
- **Procedural animation.** The pack has no wave, talk or dance clips, so these are code layers on top of the pack's clips:
  - waving
  - talking
  - lying back in the chair
  - tilting back
  - shivering
  - blinking
  - the dance (the pack's cheer, with a bounce and a wiggle)
- **Cuts instead of climbing in.** Sam and the player get into and out of the chair during a quick fade, not with an animation.
- **Fake blur.** The "blur" after the drops is a white haze overlay.
- **No parental gate.** Settings are not behind one yet.
- **No minigames.** Coins come only from the level.
- **iOS builds need a Mac.** Unity only exports the Xcode project on Windows.
