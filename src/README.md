# Source Code

| Path | What it is |
|---|---|
| `EyeCanDo/` | Unity 6 (6000.6.2f1) 3D proof of concept for EyeCanDo, the game that gets children ready for an eye-clinic visit. Targets iOS and Android tablets. |
| `TraceToDesign.md` | Traceability between code and design modules. |

## EyeCanDo PoC v2

This version rebuilds the 3D Eye Drops level as a **narrated storyboard with few words**, following [PoCResearch.md](../PoCResearch.md). It also adds the save files, coins, dress-up shop and settings from the 2D cosmetics PoC.

Title → **save files** → level select (coins, **Dress up**) → **Eye Drops**: 14 voiced panels, then Home / Play again.

| # | Panel | What the child does |
|---|---|---|
| 1 | Dr. Iris says hi | Watch (she waves) |
| 2 | Meet Sam, who is getting drops too | Tap Sam: he cheers and says "Hi! I'm Sam, nice to meet you." |
| 3 | Dr. Iris and Sam walk to the drops chart; each picture lights up as she explains it (two big pictures: a drop goes into the eye, then the pupil is big) | Watch |
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
  - the speaker's portrait: a live render of their 3D character, mouth moving with the voice.

  There are no quizzes and nothing can be failed.
- **Scout characters.** Everyone is a round, friendly scout (the genre of games like PEAK, with our own designs): a big round head with big eyes, brows and rosy cheeks, a sturdy chest, chunky arms with mitten fists and short legs in socks and hiking boots. They come in bright colours, not skin tones, and stand with their elbows out, ready to go. Dr. Iris is a grown-up purple scout (1.7 times a kid) in a white coat and head mirror; Sam is a blue scout in a red cap.
- **Saves, coins and dress-up.** Three save slots, each with its own scout, coins, sticker and clothes. The level pays 40 coins the first time and 5 on replays. Dress up has a tab per slot:
  - **Colour:** twelve bright colours.
  - **Eyes (18) and Mouth (19):** from big shiny eyes to sleepy, squinting, crying, anime and fierce ones; mouths from a smile to fangs and a toothy grin. Every mouth has its own talking shapes.
  - **Brows (11, all free):** thin, soft, arched, straight, short, angled, tapered, thick, bushy, dots and a unibrow (or none). Each is drawn in six moods (neutral, happy, worried, angry, surprised, focused), so brows show how a scout feels: every story line says how its speaker feels, and Dr. Iris looks focused while she examines or holds the drops.
  - **Hats:** cap, beanie, sprout and bandana are free; 21 more cost 20 to 80 (beret, ninja headband, visor, sailor, medic, straw, sun hat, pith helmet, propeller (it spins), chef, mushroom, wolf ears, frog, cowboy, gnome, wizard, jester, antlers, laurel, knight helmet, crown).
  - **Fits:** **Default** (the everyday sky-blue camp shirt with collar, pocket and buttons, navy shorts and white socks) is free; 14 more cost 30 to 90 (sailor, tropical, climber, ice puffer, cowboy, aviator, gnome, toga, mummy, skele onesie, jester, fairy, knight, astronaut).
  - **Old saves** keep their coins, stickers and the hats still sold; outfits, hats, capes, sashes and face accessories (glasses and the like) that no longer exist are refunded in coins, and anyone wearing a removed fit goes back to Default.
  - **Spin the scout round** by dragging across them, to see the back. The grid scrolls.
- **Coins in development.** In the editor and development builds every save is topped up to 1000 coins (`CoinRules.DevCoins`) so the whole shop can be tried. Release builds start at zero and earn coins only from levels.
- **Settings** (title, or the gear in a level, which pauses): sound volume, voice volume, captions, big text, move on by itself, less motion. **Reset game** asks twice.

### Studio splash

The game starts in `Scenes/Splash.unity`, about 3 seconds long. The logo, a lamp's lens holding an H whose crossbar is a little beam, sits unlit in the dark. A switch clicks, the lens lights up and throws a beam to the right, and the view shifts across as the rest of the name (INDSIGHT) shows as dark letters in the light, as if it had been standing there in shadow. Then the menu fades in. A tap skips it, and with **less motion** on the lit logo simply fades in and out, without sound.

- The logo and the backdrop (glow and beam) are drawn by two shaders (`Shaders/SplashMark`, `Shaders/SplashBackdrop`); `StudioSplash` animates them and the letters as a function of time. **POC → Rebuild Splash** rebuilds the scene.
- The sound is original, made by `Tools/SplashGen/make_splash_audio.py` (Python, no packages needed): a light switch clicking and a whoosh as the beam passes, no notes. Its cue times match the timeline in `StudioSplash.cs`: change them together and rerun the script.
- `Builds/Windows/EyeCanDo.exe -splashrecord <folder>` saves every frame as PNGs (60 a second), for making a preview video.

### Open and run

1. In Unity Hub choose **Add → Add project from disk** and pick `src/EyeCanDo` (editor 6000.6.2f1, with the Android and/or iOS modules).
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
                            TeachingChart, SpeakerPortraits, rigs
  Scripts/Scene/Bean/       BeanRig (looks, procedural animation and IK, lip sync), FaceArt (face atlas layout)
  Scripts/UI/               captions + NarrationPlayer, reward (coins fly to the purse), HUD, settings, confirm dialog,
                            saves, dress up + KidSpinner, menu, Sfx
  Scripts/DevTools/         PocAutoPilot (smoke test, only active with -autopilot)
  Editor/                   PocBuilder (+ .Ui) / PrefabBuilder / BeanBuilder + BeanMeshes / FaceArtPainter / UiKit /
                            CeilingArt / ChartArt (generate everything),
                            ShotCapture, DebugCapture, DebugLooks
  Audio/Narration/          one WAV per line (generated voices for now) + NarrationScript.md
  Audio/Sfx/                CC0 sound effects copied from the 2D PoC
  Data/                     level + step assets (generated: edit the lines in PocBuilder.BuildEyeDropsProcedure)
  Models/Bean/              generated meshes for the scouts (torso, head, clothes, hats, face patches)
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
- **Scout characters** are built in code (`BeanBuilder`, meshes from `BeanMeshes`); there is no external character art. `Kid.prefab` (1 m) is Sam and the player's kid, with different `AvatarLook`s; `Doctor.prefab` is a grown-up scout at 1.7 m with longer arms.
  - **Shape.** The torso and the head are surfaces of revolution whose side profiles are superellipses (`BeanMeshes.Shape`). Clothes and hats are shells offset from those profiles, so they hug the body.
  - **Face.** Curved patches on the head show cells of three atlases painted by `FaceArtPainter` (layout in `FaceArt`): eyes (each open and shut, with whites and a shine), rosy cheeks, mouths (each in four shapes from closed to wide open), brows (each style in every mood, in a fourth atlas) and the face accessory, drawn over the eyes and brows so glasses line up. The skin glows faintly in its own colour so it stays bright in the room's light.
  - **Switching looks.** Hats and fits are child objects named `<Slot>_<id>[_<part>]` (`Hat_crown`, `Outfit_sailor_Body`, `Outfit_sailor_ArmL`), and `BeanRig.ApplyLook` switches them on and off. The skin colour reaches the renderers through property blocks; clothes keep their own colours.
  - **Animation** is all procedural (`BeanRig`): every joint is built unrotated, and each frame the pose is written from scratch. A base (idle breathing, sway and looking around, walking, sitting) has layers on top: waving, cheering (arms up, two hops), dancing, reaching, talking (nods), head back, shivering, squashing and blinking. Then world-space layers: a **focus** (the body leans and the head turns toward a point) and **hand goals** (two-bone arm IK to a target, or up to the character's own eye).
  - **The doctor at work.** Looking into an eye (`DoctorActor.Examine`), Dr. Iris leans in, raises the light to her own eye and looks from spot to spot: over Sam in the chair, and in first person, where she starts from the end of the leg rest and leans in. For drops (`DoctorActor.HoldBottle`) she leans over and holds the bottle above the eye with her nearer hand; `ExamRoomContext` does this whenever the bottle is held up over an eye.
  - **Lip sync.** `NarrationPlayer.VoiceLevel` reads how loud the recording is at the playing position (from the clip's samples, so the volume setting doesn't matter; narration imports as Decompress On Load). The speaker's mouth picks one of its four shapes from that loudness, and the copies in the portraits do the same. Lines without a recording babble.
  - **Sitting:** `KidActor.SitInSeat` rests the bottom of the torso on the `SamSeat` anchor (on the cushion), lays the legs along the leg rest and leans the scout back against the backrest; "head back" tips the head, so the eyes end up at the chair's `ChairEyes` anchor where the drop lands.
  - **Walking** is 2.4 m/s for the kids and 2.6 m/s for Dr. Iris, so the walks to the chart and the door take a couple of seconds.
  - **POC → Debug Looks** renders contact sheets into `Captures/looks`: every fit (front and back), hat (front and side), eye style (open and shut), mouth at each talking shape, face accessory and colour, filmstrips of each animation, and the doctor's poses. **POC → Debug Captures** renders the scene: Sam seated, Dr. Iris examining him and holding the drops over him, and both first-person views; it logs how far her hand is from the bottle.
- **Speaker portraits.** `SpeakerStudio` in the EyeDrops scene holds copies of Dr. Iris and Sam, filmed into render textures. The copies talk when their character speaks.
- **Camera shots** are the `CinemachineCamera` children of `Shots` in the EyeDrops scene. Steps refer to them by name.
- **UI style** comes from the 2D prototype (`UiKit`).

### Regenerating

**POC → Rebuild All** (`PocBuilder.BuildAll`) generates:
- the prefabs, step assets and scenes;
- the sound library and the ceiling picture;
- the narration script and `lines.json`.

It is safe to rerun, but it overwrites hand edits to generated files. To change dialogue for good, edit `PocBuilder.BuildEyeDropsProcedure` and regenerate the voices.

Headless equivalents (run from `src/EyeCanDo`, with `Unity` = the 6000.6.2f1 editor executable). **Do not add `-nographics`.** The SVG icons are rasterised on the GPU, and without one they import as grey squares. The builder logs an error if that happens.

```sh
Unity -batchmode -projectPath . -executeMethod Hindsight.EyeCanDo.EditorTools.PocBuilder.BuildAll -quit
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml
Unity -batchmode -projectPath . -buildTarget Android -executeMethod Hindsight.EyeCanDo.EditorTools.PocBuilder.BuildAndroid -quit
```

The EditMode tests include the automated checks from the research plan (`StoryContentTests`):
- every line has narration
- captions are 8 words or fewer, under 120 words in all
- 14 panels, ending on the reward
- no fail language ("not so bad" is allowed: it reassures)
- Sam goes before the player and has his own lines
- the chart's pictures light up in order, while the line is being said

### Smoke test

**POC → Build Windows (smoke test)**, then run `Builds/Windows/EyeCanDo.exe -screen-fullscreen 0 -autopilot <folder>`. It:
1. Picks Save 1 and plays the level: it taps Sam and the chair, and lets go once while the drop falls. It saves frame bursts of the chart, Sam walking, dancing and leaving, and the coins flying in.
2. Opens Dress up, spins the scout round, picks new eyes, a new mouth and a face accessory, and buys a hat.
3. Logs how often each mouth shape showed while Dr. Iris and Sam talked; a mouth using fewer than 3 shapes fails the run.
   The smoke-test build is a release build, so it starts with no coins and buys a hat with the coins the level pays.
4. Saves a screenshot of every state into `<folder>`, keeping its save data in `<folder>/data`.
5. Exits with code 0 if nothing logged an error and Save 1 ends with a sticker and a bought item.

### Known PoC shortcuts

- **Generated voices.** The narration is text-to-speech (see Voices), not recorded actors. It was checked automatically for words, shouting, pace and (for Sam) how childlike it sounds, and should still be listened to.
- **The storyboard grew.** The research plan asked for 12 panels; the agreed storyboard has 14, and its spoken lines are longer than the plan's (the captions still follow the 8-word rule). The script says "Click"; "Tap" may suit tablets better.
- **Mixed art.** The room and chair come from free packs; the characters are made in code, in the same genre as PEAK's scouts but designed for this game (not copied from it).
- **Simple clothes.** Fits and hats are built from shells, rings and primitive shapes, so details are chunky.
- **Some text shop cards.** Eye, mouth and face-accessory cards show their drawing and colour cards a swatch; fit and hat cards show the item's name. Trying an item on shows it on the scout.
- **Procedural animation.** Every move is code, not hand-keyed animation, so moves are simple: walking, waving, cheering, dancing, sitting, head back, shivering, blinking, talking, leaning in to examine and holding the drops.
- **Loudness, not phonemes.** The mouth opens as wide as the voice is loud; it doesn't shape individual sounds.
- **Cuts instead of climbing in.** Sam and the player get into and out of the chair during a quick fade, not with an animation.
- **Fake blur.** The "blur" after the drops is a white haze overlay.
- **No parental gate.** Settings are not behind one yet.
- **No minigames.** Coins come only from the level.
- **iOS builds need a Mac.** Unity only exports the Xcode project on Windows.
