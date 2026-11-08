# Recording the narration with ElevenLabs

There are 19 lines: 15 for Dr. Iris and 4 for Sam. They take about 1½ minutes in total. This guide covers:

1. Making the two voices.
2. Generating each line, with the text and directions to paste in.
3. Adding the files to the game.

Plan names, limits and licence terms change, so check ElevenLabs' current terms before anything ships. Free plans have usually been non-commercial and required credit.

---

## 1. Make the two voices (Voice Design)

Go to **Voices → Add a new voice → Voice Design**. Paste in a description and a preview text. ElevenLabs gives you a few versions; save the one you like best. If none are right, try again.

### Dr. Iris

**Voice description:**

> A warm, gentle woman in her thirties with a friendly American accent, like a kind children's doctor or a kids' TV presenter. Soft, smiling, reassuring and unhurried, with a light brightness in her voice. Calm and clear, never loud, never sing-songy, never patronising. Close-mic studio recording with no background noise.

**Preview text** (use her real lines so you can judge the fit):

> Hey, I'm Doctor Iris. I'm going to be looking at your eyes today. When I put the drops in, they will be cold and may sting for a little bit. You're doing great so far!

Pick the take that sounds like she's **smiling while she talks** and is quieter rather than bigger.

### Sam

**Voice description:**

> A cheerful young boy around eight years old, the voice of an animated cartoon kid character. Bright, high-pitched, friendly and a little bit cheeky, with natural kid energy but not shouty. Clear American accent. Close-mic studio recording with no background noise.

**Preview text:**

> Hi! I'm Sam, nice to meet you. Will do! Oooh, cold! Yay, that wasn't so bad!

If Voice Design keeps giving you a grown-up voice, or won't make a child's voice:
- Describe it as **"a youthful, high-pitched cartoon boy voice, like a voice actor playing a kid in an animated show"**.
- Or search the **Voice Library** for "kid", "boy", "cartoon" or "animated", and filter by the "young" age.

Name the saved voices **Dr Iris** and **Sam** so they're easy to find.

---

## 2. Generate each line

Use **Text to Speech** with the **Eleven v3** model. It's the most expressive, and it understands the bracketed **audio tags** below.

- **Settings:** Stability **Natural**. If a take sounds over the top, switch to **Robust**. Keep the same settings for every line of a character so they all match.
- **Takes:** make 3–4 of each line and keep the best. Play it right after the previous line to check the conversation flows.
- **What to listen for:**
  - every word is said, and nothing is added;
  - nothing is shouted;
  - it isn't rushed; the players are young children;
  - it suits the moment: explanations are calm, happy moments are bright.
- **What goes in the box:**
  - Paste only the **Text to paste**. The **Direction** is for you, to judge takes against.
  - The audio tags (`[warmly]`, `[excited]`) are hints and aren't spoken. If a tag gets read aloud or overdone, delete it.
  - "Doctor" is spelled out because "Dr." can be read as "D R". The game's captions still say "Dr. Iris".
  - Ellipses (…) add a short natural pause. Remove them if the pause sounds too long.

If you'd rather do Sam's whole scene in one go, **Text to Dialogue** (v3) voices both characters together, which can make the replies sound more natural. You then have to cut each line out into its own file, so it's more work. Line by line is fine.

### Dr. Iris (voice: Dr Iris)

| File | Direction | Text to paste |
|---|---|---|
| `p01_hello` | First meeting. Friendly and welcoming, smiling, relaxed. | `[warmly] Hey, I'm Doctor Iris. I'm going to be looking at your eyes today.` |
| `p02_sam` | Introducing a friend, then a gentle invitation. Not loud. | `[warmly] This is Sam. He is also getting eye drops today. [gently] Click on Sam to say hi.` |
| `p03_chart` | Explaining by a picture. Calm, slow, a little wonder on "all the way inside". | `[calmly] In order to see into your eye, I'll need to use some special eye drops. The drops make your pupil nice and big… so that I can see all the way inside.` |
| `p04_first` | Light and encouraging, like asking a helper. | `[cheerfully] Sam, you're up first. Hop into the chair so we can see how it's done.` |
| `p05_cold` | Honest and reassuring. Soft on "sting", bright on the praise at the end. | `[softly] When I put the drops in, they will be cold and may sting for a little bit. [reassuring] Lots of kids say it's not so bad. [warmly] You're going to do great!` |
| `p06_blink` | Calm instructions. | `[gently] Make sure to blink to spread the drop around. Then I can take a look inside your eye.` |
| `p07_good` | Pleased and proud of Sam. Bright, not loud. | `[happily] Looking good, Sam!` |
| `p08_set` | After Sam's "Yay": warm, wrapping up. | `[warmly] You're all set!` |
| `p09_turn` | Inviting the player. Warm and excited for them. | `[warmly] Now it's your turn! Click on the chair to have a seat.` |
| `p10_hold` | The most important instructions: slow, calm, steady. No button on screen yet. | `[gently] Make sure to keep your head back and eyes open. Remember, the drops will feel cold and may sting for a little bit.` |
| `p10_button` | The button and the bottle appear with this line. Encouraging. | `[encouraging] Hold the button to keep your eye open!` |
| `p11_again` | Plays when the child lets go mid-drop. Patient, no pressure. The game currently reuses the first sentence of `p10_hold`. | `[gently] Make sure to keep your head back and eyes open.` |
| `p12_did` | Proud right away, then a gentle instruction. | `[excited] You did it! [gently] Now blink! The drops need to spread around.` |
| `p13_look` | Impressed, then calm while she looks. | `[warmly] Amazing! Now I'll just take a quick look into your eye.` |
| `p13_great` | The big happy ending. Bright and proud. | `[happily] Fantastic! Your eyes look great!` |

### Sam (voice: Sam)

| File | Direction | Text to paste |
|---|---|---|
| `p02_hi` | Friendly and a little shy, saying hi to a new kid. Not yelled. | `[cheerfully] Hi! I'm Sam, nice to meet you.` |
| `p04_willdo` | Eager, quick, happy to help. | `[eagerly] Will do!` |
| `p06_ooh` | Surprised by the cold, then a little giggle in the voice, not pain. | `[surprised] Oooh, cold! [giggles]` |
| `p08_yay` | Relieved and proud, while dancing. | `[excited] Yay, that wasn't so bad!` |

`p06_ooh`: if the giggle comes out too long, or sounds odd, remove `[giggles]`.

---

## 3. Add the audio to the game

1. **Download** each line you keep. MP3 is fine and Unity plays it.
2. **Name** each file exactly after the **File** column: `p01_hello.mp3`, `p02_hi.mp3` and so on. The game finds clips by these names, so a typo means a missing line.
3. **Move the old generated WAVs out of the folder** `Assets/_Project/Audio/Narration/`. Delete each `pXX_….wav` and its `.wav.meta`, or move them outside the project as a backup.
   - This step matters: the builder looks for `.wav` first, so an old WAV with the same name wins over your new MP3.
   - If you download WAV instead of MP3, just overwrite the old files.
   - Leave `NarrationScript.md` where it is.
4. **Copy in** your new files.
5. **Trim** long silence at the start or end of a clip, for example in Audacity. The game already adds its own pause after each line, so extra silence makes the conversation drag. Keep a tenth of a second or so.
6. **Close any other Unity window** using the project, open it in Unity, and run **POC → Rebuild All** from the menu bar. This connects the clips to the story.
7. **Check it:**
   - **Tests:** in **Window → General → Test Runner → EditMode**, click **Run All**. The test `EveryLineHasNarrationAudio` passes when all 19 lines are found. If any are missing, it fails and lists their ids.
   - **Play:** press Play in the `Title` scene and go through the level. Listen for lines that come in too early, cut off, or don't fit the moment, and remake those in ElevenLabs.
   - **The chart scene (`p03_chart`):** the chart's pictures light up at fixed points in the line, around the words "special", "big" and "see". They're timed as a share of the clip's length, so a different pace still mostly works. If one lights up noticeably early or late, tell me and I can tune it.

### Notes

- `generate_voices.py` never overwrites files that already exist unless it's run with `--force`, so it won't replace your recordings. Don't run it with `--force` once you have the ElevenLabs files.
- Update the narration row in `THIRD_PARTY_ASSETS.md` to say the voices are from ElevenLabs, along with your plan's licence terms.
- **If a line's wording changes in the game,** it changes in `Editor/PocBuilder.cs`. After **Rebuild All**, `NarrationScript.md` shows the new wording; remake that line with the same file name.
