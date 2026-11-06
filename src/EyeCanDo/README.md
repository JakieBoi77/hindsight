# EyeCanDo: Special Drops PoC (Unity 2D)

Proof of concept for the eye-drops section of EyeCanDo. A child meets
**Dr. Iris** and learns what eye drops do and how they feel. They **watch Mia**
get her drops, then **their own kid** hops into the exam chair and they do
each step themselves. Two arcade games turn the same behaviours into play
that is worth repeating. Built from [GameDesign.md](../../GameDesign.md) and
[EducationalGameResearch.md](../../EducationalGameResearch.md); where they
differ, the notes under *Changes from the design docs* explain why.

Title → save files → clinic map → Special Drops → **Story** (unlocks the games), **Drop Catch**,
**Blurry Spy**. Coins from the story and games buy clothes and items in **Dress Up**.
The other eight stops are greyed out with a *Soon* ribbon.

**Out of scope for this PoC:**

- narration (captions stand in) and music (sound effects only)
- leaderboards (personal bests only)
- caregiver setup (notice, age group); the settings are not behind a parental gate yet

## Open and run

1. Install Unity **6000.6.2f1** with the Android and/or iOS modules.
2. In Unity Hub choose **Add → Add project from disk** and pick `src/EyeCanDo`.
3. Open `Assets/_Project/Scenes/Title.unity` and press **Play**. Any scene can
   be played directly. For a device-shaped view, switch the Game view to
   **Simulator** and pick a phone and a tablet.

Each player is saved to `Application.persistentDataPath/saves/save_1.json` to
`save_3.json`, and the settings to `settings.json` beside them. A save from an
earlier build (`progress.json`) moves into Save 1 on first launch. To start
over, use **Settings → Reset game**, or delete those files.

## What to try

| Screen | What the child does | What it is for |
| --- | --- | --- |
| **Title** | Tap **Play!** or **Settings**. The kid on the right is the one from the save played last. | |
| **Save files** | Three cards. A used save shows its kid, coins, stars (out of 18) and whether the story is done; tap it to carry on. An empty one says **New game**. **Delete** under a used save asks first, naming the coins that will go; **Keep it** changes nothing. Home goes back to the title. | Several children can share one device, each with their own character and progress. |
| **Settings** | Sound volume (slider, and a speaker button to mute), text size (Normal or Big), text speed (Slow, Normal, Fast), auto-continue (lines move on 1.6 s after they finish), and reduce motion (no bobbing or drifting clouds, less confetti). Changes apply at once and are shared by every save. **Reset game** sits apart at the bottom: it asks "Reset the whole game?", then "Are you really sure?", saying everything that will be deleted. Only a second yes deletes every save and setting; the title then says so. | FR_Preferences, FR_ResetJourney, FR_ClearAllData (without the parental gate, as asked). |
| **Dress Up** | Eight tabs: skin and ears; eye shape, eye colour and eyebrows; nose and mouth; hair style and colour; shirt, shirt picture, shorts and shoes; hats; glasses; things to hold. Faces, hair and most colours are free. The shop has 74 items: 12 special colours, 17 shirt pictures, 18 hats, 11 glasses and 16 things to hold, from 15 to 80 coins. Tapping one tries it on, and **Buy** keeps it. | A reason to earn coins; the character is the child's own. |
| **Clinic map** | Tap Special Drops. Grey stops only wiggle and say "coming soon". **Dress Up** bottom-right; Home goes back to the save files. | Visit order (FR_StartLevel). |
| **Story** | Lines type themselves out; a tap shows the whole line (and speeds up what is happening on stage), and the next tap continues. Watch and listen while Dr. Iris explains the drops: tap the bottle to see the pupil grow, then answer a picture question. Hear how they feel: cold, a little sting for a few seconds, blurry and bright after (this step cannot be skipped). **Watch Mia** get her drops, answer "what is your job?", then **your turn**: tap the chair to hop up, drag your head back, press and hold the ceiling star while the drop goes in, tap your eye to blink, and drag the tissue to the drip. Earn the Brave Eyes sticker and 40 coins (5 for replays). The games unlock. | The step-by-step walkthrough of the first PoC, with an honest sensation step, a peer model and real practice (FR_DescribeSensations, FR_ClinicsDiffer, FR_SpeakUp). Letting go of the star only pauses ("Ready when you are"). |
| **Drop Catch** | Locked until the story is done. One tap past the instruction, then 3, 2, 1, Go! Your kid leans back in the chair. Drag to slide the chair and catch drops from friendly bottles in your eyes. Every catch is cold (a shiver); golden drops are worth 3. When the eye meter fills, tap twice to blink for a bonus. 30–40 s rounds; each level is faster with a smaller catch zone, and level 3 has two bottles. **Three stars only for catching every drop**; two for three in four. | Head back, eyes open, cold is okay, blink afterwards. A miss only splashes the floor. |
| **Blurry Spy** | Locked until the story is done. One tap past the instruction, then 3, 2, 1, Go! The playroom is blurry and bright. Find the thing on the card; each find clears the blur a little. Levels have 5, 7 and 9 things to find among 10, 14 and 18, smaller and blurrier each level; level 3's things drift. **Three stars only with no wrong taps and quick finds**; two with at most two wrong taps. | After drops things can look blurry and bright, and it goes away. |
| **Results** | Stars (always at least one for finishing), the score, your best and the coins earned, which fly into the purse. *Again*, *Level N+1* or *Map*. | Coins: 5 per round, plus 2 per star, plus 10 for each star earned for the first time. Personal bests only go up (thm_NoLoss). Every level of an unlocked game is open; the suggested one bobs. |

Every story and game screen has **Home** and **Restart** top-left in the same
place. The story also has **hold-to-skip** top-right: hold it for 1 s to
fast-forward the current step. It is hidden while sensations are described.

## Built for phones and tablets

- **Nothing important is ever cut off.** The canvas uses a 1920x1080
  reference area that is always fully on screen (CanvasScaler *Expand*).
  Wider phones get extra room at the sides, and 4:3 tablets get extra room
  above and below; backdrops fill the extra space.
- **Captions never cover anyone.** Captions have their own strip across the
  top, between Home and Skip. Characters, props and games stay below it, and
  the eye close-up sits beside the characters, not on them. The storyboard
  checks this at a phone shape (19.5:9) and a tablet shape (4:3).
- **Everything is sharp.** All art is SVG, imported as sprites rendered well
  above their largest on-screen size with 4x anti-aliasing. Panels and rings
  are drawn in code at 512 px, and text is TextMesh Pro (SDF).
- **Nothing is clipped.** Art canvases leave room for raised arms and tall
  hats, and menus sit beside the characters (the Drop Catch chair moves aside
  while a menu is open).
- Buttons are at least 15 mm (`TouchTargetScaler`), and Home, Restart and
  Skip follow the safe area around notches.

## How it is organised

```
Assets/_Project/
  Art/                SVG art, generated by Tools/ArtGen (Dr. Iris's poses; Avatar/ holds the kids'
                      recolourable parts, hats, glasses, shirt pictures and held items; props, eye
                      close-up, rooms, icons, Spy items)
  Audio/              Sound effects: CC0 clips fetched and levelled by Tools/SoundFetch/fetch_sounds.py
  Scripts/Core/       EyeCanDo.Core, no engine references: DropCatchRound, BlurrySpyRound, StillHold,
                      AvatarLook/AvatarCatalog (choices and prices), CoinRules, GameRecord/PlayerProgress
                      (coins, shop, look), ProgressFile, SaveSlots, GameSettings, InputGate, SkipHold, IdleHint, ContentRules
  Scripts/Runtime/    EyeCanDo.Runtime: App (services, fader, sound), UI (Stage layout, CaptionBar,
                      ghost hand, chrome, buttons, coin purse, confirm dialog, volume slider), Menus (save files,
                      settings), Characters (CharacterRig, AvatarHead,
                      EyeCloseUp, ExamChair), Title, Map, Story, Games, DressUp
  Scripts/Editor/     PocBuilder (settings, font, theme, art and sound libraries, blur material, scenes),
                      ContentValidator, BuildScripts, SVG import settings
  Shaders/            UIBlur (Blurry Spy), UIRecolor (avatar colours: white takes the colour, black the ink)
  Data/               StoryScript, CaptionBank, StopCatalog: edit text here
  Resources/          UiTheme, ArtLibrary, SoundLibrary, SpyBlur and Recolor materials (generated)
  Scenes/             Title, Saves, ClinicMap, EyeDropsStory, DropCatch, BlurrySpy, DressUp (generated)
  Tests/              EditMode (rules, progress, content, generated assets), PlayMode (end to end, storyboard)
Assets/ThirdParty/    Fredoka (OFL): see THIRD_PARTY_NOTICES.md
Tools/ArtGen/         The art generator (Python 3, no dependencies)
Tools/SoundFetch/     Downloads and prepares the CC0 sound effects (Python 3 and ffmpeg)
```

- **Game rules live in `Core`** as plain C#, so they are unit tested without
  Unity. The MonoBehaviours in `Runtime` present the rules and forward
  touches to them.
- **Screens are built in code** on one shared `Stage` layout from one UI kit,
  and every widget reads the same `UiTheme`.
- **The story is data.** Each `CaptionLine` has a speaker (shown in the
  caption bar, and their mouth moves on stage), a pictogram, a *beat* (what
  happens on stage, such as `MiaTipsHead` or `YouLookUp`) and an empty
  `narration` audio slot. Beats starting with *You* wait for the child to act.
  Recorded narration can be dropped in later with no code change.
- **Characters are rigs**, assembled from SVG parts so they can blink, talk,
  tip their head back, shiver, hop, walk and wave. Kids are built from an
  `AvatarLook`; their skin, hair and clothes are black-and-white art
  recoloured by the `UI Recolor` shader, so any colour works with one set of
  parts.
- **Sounds are CC0 clips from OpenGameArt** (Kenney's Interface Sounds,
  fupi's Win Jingle and bells, water splashes and bubble pops), chosen by
  measuring them: rewards rise or end on a major chord, catches are small
  splashes, wrong taps are a soft bump, buttons and dialogue are a tiny soft
  click, and coins landing are one jingle of money. Each sound plays on its own
  voice, so a pitched run (Blurry Spy finds climb a major scale) never bends
  another sound. Results go stars, then the jingle, then the coins. Sounds that happen often (caption
  pops, footsteps, coins) are mixed quietly. To swap one, change its row in
  `Tools/SoundFetch/fetch_sounds.py`, rerun it, then run **Build All**.

### Editing content

Edit the story and captions in `Assets/_Project/Data/` in the Inspector, then
run **EyeCanDo → Validate Content**. The same rules run as an EditMode test.
The validator fails on:

- a sentence longer than 10 words;
- a threat or false-reassurance word (hurt, needle, "won't feel", and so on);
- a drop shown before the sensation step;
- a skippable sensation step.

It also lists every caption still waiting for recorded narration.

**All child-facing text is a DRAFT** pending paediatric ophthalmology and
child-life review (Development Plan, PoC Risk 1).

### Changing the art

Edit `Tools/ArtGen/generate_art.py` and run it from `src/EyeCanDo`
(`python Tools/ArtGen/generate_art.py`), or replace any SVG in
`Assets/_Project/Art/` in place. Then run **EyeCanDo → Build All** so the art
library picks up new files.

### Regenerating

**EyeCanDo → Build All** (`PocBuilder.BuildAll`) re-creates the settings,
font, theme, art and sound libraries, blur material and scenes. It keeps
existing content assets. **EyeCanDo → Reset Content To Defaults** restores
the default story and captions from `DefaultContent.cs`.

```sh
Unity -batchmode -quit -projectPath . -executeMethod EyeCanDo.EditorTools.PocBuilder.BuildAll
```

## Tests

Use **Window → General → Test Runner**, or run them headless from `src/EyeCanDo`:

```sh
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults/editmode.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults TestResults/playmode.xml
```

**EditMode (61 tests).** These cover:

- Drop Catch: drops stop at time-up; higher levels are faster; golden drops; a
  miss only resets the combo; the blink bonus; three stars only when every drop
  is caught; finishing always earns a star;
- Blurry Spy: distinct targets; wrong taps cost nothing but the third star; the
  blur clears to zero; speed stars;
- the look-up hold, where letting go pauses and keeps progress;
- personal bests only go up, and the suggested level;
- coins for the story and rounds (with the new-star bonus), the games unlocking
  after the story, buying (enough coins, once only), free face and hair changes;
- saving and loading (coins, owned items, look), damaged or old save files, and
  a leftover temp file;
- three save slots that keep their own players, deleting one or all, a damaged
  slot showing as empty, and moving the earlier single save into Save 1;
- settings saving and loading, and damaged settings going back to defaults;
- the input gate, hold-to-skip and idle hints;
- the content rules on the defaults and on the project's Data assets;
- the generated art, sound and material assets load and are complete.

**PlayMode (14 tests).** These play the real scenes through the same handlers touches use:

- Dress Up: free changes are worn at once; shop items are tried on, refused
  without coins, then bought and worn;
- coming-soon stops are harmless, and the games stay locked before the story;
- the story plays to the end, the sensation step cannot be skipped, and the
  sticker and coins are saved;
- skip fast-forwards a step;
- perfect Drop Catch play earns three stars and coins;
- one wrong tap in Blurry Spy costs the third star, and the blur clears;
- a game opened before the story shows the lock;
- Home from a game goes back to the Special Drops panel;
- Play leads to the save files, and each save keeps its own player;
- deleting a save asks first and only deletes after confirming;
- settings apply at once (the caption bar grows with big text) and survive deleting a save;
- reset asks twice, and only the second yes deletes every save and setting.

**Storyboard (explicit).** This test plays the whole game and saves a
screenshot of every screen and moment, at a phone shape and a tablet shape,
to `TestResults/Storyboard/phone` and `TestResults/Storyboard/tablet`:

```sh
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testFilter StoryboardCapture
```

## Builds

From **EyeCanDo → Build**, or headless with `-executeMethod`
`EyeCanDo.EditorTools.BuildScripts.BuildWindows` (smoke test), `BuildAndroid`
or `BuildIos` (Xcode project; needs a Mac to finish). Output goes to `Builds/`
(git-ignored).

## Changes from the design docs

- **The games unlock after the story**, so every child hears the honest
  sensation step before playing. Levels within a game are all open.
- **Coins and a shop for cosmetics** (GameDesign §7), with a free character
  maker. No leaderboards: only personal bests.

- **No coping-tool choice (My Plan, Balloon Breath, squeeze a hand, hold
  Teddy).** Clinics may not offer these, so the game does not promise them.
  The story teaches what the child can always do: head back, look up, hold
  still, blink.
- **The story is a walkthrough in the spirit of the first PoC:** Dr. Iris
  explains, picture questions check understanding, and the child watches Mia
  before doing it themselves. Wrong answers get a gentle hint and another try.
- **The games are arcade games with scores, stars and personal bests** rather
  than gentle drills, so there is a reason to replay. They still never fail
  the child: every finished round earns a star, nothing is taken away, and
  every level is always open.

## Known PoC shortcuts

- **Characters face the front**, so "head back" is shown by the face lifting
  and the eyes looking up. The round eye close-up beside the characters
  carries the drop, blink and drip moments.
- **Drop Catch catches several drops**, which is a game abstraction; a real
  visit is usually one or two drops per eye. This needs clinician review.
- **No narration or music yet.** The sound effect volume is in Settings.
- **Settings are not behind a parental gate** (FR_ParentGate), as asked for
  the PoC. The two-step reset question is the guard against accidents.
- **Coin prices and rewards are first guesses** and need tuning with children.
- **Device testing is still needed.** Layout is checked at phone and tablet
  shapes in the storyboard. Touch-target sizing uses `Screen.dpi`, with a
  160 dpi fallback, and has not yet been checked on real devices.
