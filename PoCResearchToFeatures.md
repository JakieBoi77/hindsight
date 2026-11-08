# PoC Research to Features: EyeCanDo 3D PoC v2

Nov 8, 2026

## Purpose

This document links the research behind EyeCanDo to the features in the 3D PoC v2 build. It shows where each feature comes from, how strong the evidence is, and where the build departs from the research.

It draws on two team documents:

- [PoCResearch.md](PoCResearch.md): the evidence and 11-point plan written for this PoC. Plan items are cited as **Plan 1** to **Plan 11**.
- [EducationalGameResearch.md](https://github.com/JakieBoi77/hindsight/blob/2d-poc-with-cosmetics-and-minigames/EducationalGameResearch.md): the wider review of children's learning games and medical preparation. It lives on the `2d-poc-with-cosmetics-and-minigames` branch. Its 15 design principles are cited as **P1** to **P15**.

Every feature below was checked against the code on `3d-poc-v2`, not against the plan. The research supports the design. It does not show that this game works: **no child has played this build in a study yet.**

## Summary

| Measure | Result |
| --- | --- |
| Plan items (11) | 7 built, 2 partly built, 1 engine only, 1 not built |
| Design principles (15) | 2 built, 8 partly built, 5 not built |
| Captions | 19 lines, 90 words in total, longest 8 words (targets: under 120 and 8) |
| Spoken narration | 19 clips, 68 seconds in total, 226 words |
| Fail states | None |

**Strongest matches to the research:** narrated pre-reader design, a guided storyboard, a peer model who goes first, and rehearsal of the real hold-still and blink actions.

**Largest gaps:** no caregiver role, no coping strategy, no off-screen practice, non-human characters, and a coin economy the research warned about.

## Evidence ratings

| Rating | Meaning |
| --- | --- |
| **Strong** | Meta-analysis or validated expert consensus that applies closely |
| **Moderate** | A single trial, or a strong finding from a different setting or age group |
| **Practice** | Clinical or child-life guidance, not an outcome trial |

## Feature map

Code paths are under `src/EyeCanDo/Assets/_Project/`.

### Story structure

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Fixed storyboard | 14 panels play in one order, with no free camera | [ProcedureRunner.cs:51-56](src/EyeCanDo/Assets/_Project/Scripts/Procedure/ProcedureRunner.cs#L51-L56) | Unassisted discovery lost to explicit teaching (d = -0.38) and guided discovery did better than other methods (d = 0.30) (Alfieri 2011). Plan 4, P3 | Strong, though not specific to games or this age |
| One shot, one beat | Each panel has one framed shot, one or two lines and at most one action | [PanelStep.cs:49-104](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/PanelStep.cs#L49-L104) | CoDHP: minimum mechanics, clear storyline. Distraction lowers comprehension (Takacs 2015). P9 | Strong |
| Progress dots | A wordless dot for each panel shows done, now and to do | [LevelHud.cs:57-64](src/EyeCanDo/Assets/_Project/Scripts/UI/LevelHud.cs#L57-L64) | Knowing what comes next helps children cope (social stories). Plan 4, P3 | Practice |
| Quizzes removed from the story | The two mid-story text quizzes are gone | [PocBuilder.cs:184-232](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L184-L232) | Embedded games and hotspots distract (Takacs 2015). A quiz wrapped in a game teaches less than an integrated mechanic (Habgood). Plan 7 | Strong |
| Short level | 68 seconds of speech across 14 panels. Full play time has not been timed | [NarrationScript.md](src/EyeCanDo/Assets/_Project/Audio/Narration/NarrationScript.md) | CoDHP: age-appropriate interaction time. Child-life guides suggest 10 to 15 minutes at most. Plan 11, P9 | Practice |

### Narration and few words

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Every line spoken | Each story line has its own recording, and a test fails if one is missing | [NarrationPlayer.cs:112-146](src/EyeCanDo/Assets/_Project/Scripts/UI/NarrationPlayer.cs#L112-L146), [StoryContentTests.cs:123](src/EyeCanDo/Assets/_Project/Tests/EditMode/StoryContentTests.cs#L123) | Narration helps storybook comprehension (Takacs 2015: 43 studies, 2,147 children). Most players cannot read. Plan 1, P12 | Strong for storybooks, moderate for games |
| Short, optional captions | One caption of 8 words or fewer, which can be turned off | [StoryLine.cs:23](src/EyeCanDo/Assets/_Project/Scripts/Procedure/StoryLine.cs#L23), [CaptionView.cs:132-137](src/EyeCanDo/Assets/_Project/Scripts/UI/CaptionView.cs#L132-L137) | CoDHP: "using simple words and commands". Captions stay optional because modality evidence in children is mixed. Plan 2 | Strong for simple words, mixed for captions |
| Replay button | Replays the current line, even while it is playing | [CaptionView.cs:112-118](src/EyeCanDo/Assets/_Project/Scripts/UI/CaptionView.cs#L112-L118) | CoDHP: "clear, repeatable verbal descriptions". Plan 3 | Strong |
| Pause after each line | About 1 to 1.5 seconds of quiet before anything moves on | [NarrationPlayer.cs:149](src/EyeCanDo/Assets/_Project/Scripts/UI/NarrationPlayer.cs#L149) | Supports processing time for young listeners. No study tested this directly | None direct |
| Pictures timed to speech | Each chart picture lights up as Dr. Iris says its cue word | [ChartStep.cs:48-58](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/ChartStep.cs#L48-L58) | Multimedia that matches the story helps (Takacs 2015). P9 | Strong |
| Button appears after the instructions | The hold button and bottle only appear with "Hold the button..." | [PanelStep.cs:87-93](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/PanelStep.cs#L87-L93) | Keeps attention on the explanation (Takacs 2015). P9 | Moderate, by extension |
| Speaker portraits | A live picture of the speaker shows who is talking, without a name | [CaptionView.cs:120-130](src/EyeCanDo/Assets/_Project/Scripts/UI/CaptionView.cs#L120-L130) | CoDHP: icons as "a replacement or help to texts". Plan 2 | Strong for the principle |

### Rehearsing the real behaviour

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Hold to keep the eye open | The child holds a button while the drop falls | [HoldStillStep.cs:34-61](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/HoldStillStep.cs#L34-L61) | Integrated mechanics teach more and were played about 7 times longer (Habgood). Therapeutic play helps preparation. P1 | Moderate |
| Blink three times | Each tap blinks and clears the blur a little | [BlinkStep.cs:29-40](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/BlinkStep.cs#L29-L40) | As above. P1 | Moderate |
| First-person chair view | The child looks up from the chair at a painted sun | [PocBuilder.cs:631](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L631), [HoldStillStep.cs:18](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/HoldStillStep.cs#L18) | CoDHP: relate the story to real experience. Realistic content transfers better for this age (Richert, Walker). P7 | Moderate |
| Real props and room | Chair, bottle, light and white coat are recognisable | [PrefabBuilder.cs](src/EyeCanDo/Assets/_Project/Editor/PrefabBuilder.cs) | As above. P7 | Moderate |

### Modelling and honest information

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Sam goes first | Panels 4 to 8 show a child getting drops before the player, and a test enforces the order | [PocBuilder.cs:194-213](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L194-L213), [StoryContentTests.cs:95](src/EyeCanDo/Assets/_Project/Tests/EditMode/StoryContentTests.cs#L95) | Watching a peer reduced anxiety before surgery (Melamed and Siegel 1975). A cartoon story from a child's view raised patching compliance from 55% to 89% (Tjiam 2013). Plan 6, P4 | Moderate |
| Sam reacts, then is fine | "Oooh, cold!" comes first, then "Yay, that wasn't so bad!" | [PocBuilder.cs:202-213](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L202-L213) | A peer shows a small, realistic worry and is fine. Children often expect drops to be worse than they are (Chauhan 2024). P4 | Practice |
| Honest about the feeling | "They will be cold and may sting for a little bit", said for Sam's turn and again for the player's | [PocBuilder.cs:199](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L199), [PocBuilder.cs:218](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L218) | Child-life guidance: honest but gentle, with what the child will feel. P6 | Practice |
| Why the drops are used | The chart explains that drops make the pupil big so she can see inside | [PocBuilder.cs:191-193](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L191-L193) | The Patchbook explained why the treatment matters (Tjiam 2013) | Moderate |
| Blur after the drops | A haze clears partly with each blink, leaving the view slightly blurry | [BlinkStep.cs:37](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/BlinkStep.cs#L37) | Show what the child will see. P6 | Practice |

### Feedback and no failure

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Nothing can be failed | Letting go only pauses the drop in mid-air, and Dr. Iris repeats the reminder | [HoldStillStep.cs:51-57](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/HoldStillStep.cs#L51-L57) | CoDHP: "providing achievable challenges". No failure states or pressure. Plan 8 | Strong |
| No fail language | A test bans "wrong", "oops", "fail" and similar words | [StoryContentTests.cs:83](src/EyeCanDo/Assets/_Project/Tests/EditMode/StoryContentTests.cs#L83) | As above. Plan 8 | Strong |
| Instant response | Tapped objects react at once, even mid-sentence, and the hold button fills as the drop falls | [TapTargetStep.cs:38-48](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/TapTargetStep.cs#L38-L48), [ActionButtonsView.cs:35](src/EyeCanDo/Assets/_Project/Scripts/UI/ActionButtonsView.cs#L35) | CoDHP: "immediate feedback" | Strong |
| Two simple gestures | Tapping and holding are the only inputs in the level | [ActionButtonsView.cs](src/EyeCanDo/Assets/_Project/Scripts/UI/ActionButtonsView.cs) | CoDHP: "using the minimum amount of mechanics" | Strong |

### Personalisation, rewards and control

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Own character | The child's dressed-up scout appears at their turn and at the reward | [ExamRoomContext.cs:78](src/EyeCanDo/Assets/_Project/Scripts/Scene/ExamRoomContext.cs#L78) | CoDHP: "providing personalization options". Plan 10 | Strong for the principle |
| Dress-up outside the level | The shop sits on the level-select screen, away from the teaching | [DressUpView.cs](src/EyeCanDo/Assets/_Project/Scripts/UI/DressUpView.cs) | Rewards and customisation belong between levels (Takacs 2015, P9) | Strong for the placement |
| Brave Eyes sticker | Sticker and dance end the level | [RewardStep.cs:26-42](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/RewardStep.cs#L26-L42) | CoDHP: "granting rewards". A symbolic sticker that mirrors the clinic. P11 | Practice |
| Coins | 40 the first time and 5 on replays, never taken away | [CoinRules.cs:11-12](src/EyeCanDo/Assets/_Project/Scripts/Core/Progress/CoinRules.cs#L11-L12) | CoDHP supports rewards, but see the departures below. P11 | Conflicting |
| Leave at any time | The Home button is on screen throughout the level | [LevelHud.cs:26](src/EyeCanDo/Assets/_Project/Scripts/UI/LevelHud.cs#L26) | Give the child control. P8 | Practice |
| Play again | The end card offers a replay | [RewardStep.cs:41](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/RewardStep.cs#L41) | Effects fade without repetition (CliniPup). P14 | Moderate |
| Settings | Voice and sound volume, captions, big text, auto-continue, less motion | [GameSettings.cs:17-32](src/EyeCanDo/Assets/_Project/Scripts/Core/Progress/GameSettings.cs#L17-L32) | Captions and text size support P12. The others are accessibility choices | Practice |

## Where the build departs from the research

These are the places where a reviewer could fairly challenge the design.

| Topic | Research says | Build does | Suggested response |
| --- | --- | --- | --- |
| Characters | The patient "should be a human child". Human-like animals reduced learning at ages 3 to 5 (Ganea 2014). P7 | Everyone is a round scout in a bright, non-skin colour, including Dr. Iris (purple) | Test recognition with children, or move closer to human proportions and colours. The strongest evidence concerns animals and fantasy, so it applies only partly |
| Coins and shop | Expected, tangible rewards can lower motivation (Lepper 1973). A reward calendar did not reach significance in the Patchbook trial. Section 2.6 says the coin economy "deserves a second look" | A known coin payout funds a large shop | Keep it as a PoC feature, then decide with the supervisor. Consider making the sticker the main reward |
| Praise | Praise the action ("You held so still!"), not in general. P11 | "You did it!", "Amazing!" and "Fantastic!" | Low-cost script change, for example "You kept your eye open!" |
| Waiting for the drops | Observed visit has about 20 minutes between drops and the examination (ER5) | Panels 12 to 13 go straight from blink to examination | Add one line about waiting. It also serves P3 and P13 |
| Spoken length | One short sentence per line (CoDHP, as applied in the plan) | The chart line is 32 words, across two long sentences | Split the line, and check whether a 3-year-old follows "pupil" |
| Word choice | Simple words and commands (CoDHP), and the game targets tablets | Lines say "Click on Sam" and "Click the chair" | Re-record with "Tap" |
| Sticker title | Pre-readers need everything spoken. P12 | "Brave Eyes!" appears as text, with no voice line on panel 14 | Add a line to the reward step, which already supports one |
| Interactive taps | Hotspots distract from comprehension (Takacs 2015) | Tapping Sam and the chair are required but tied to the story | Keep them. The study covers storybooks, and these taps move the story forward |

**The research conflicts on discovery.** Plan 5 asked for one glowing object to discover per panel, because children prefer to explore (CoDHP). Takacs 2015 warns against this kind of hotspot. The engine supports it ([PanelStep.cs:68-79](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/PanelStep.cs#L68-L79)), but no panel uses it, so the current build matches Takacs.

## Status of every plan item

| Plan | Change | Status |
| --- | --- | --- |
| 1 | Narrate every line | Built |
| 2 | Cut text to captions | Built |
| 3 | Replay button | Built |
| 4 | Storyboard panels and progress dots | Built |
| 5 | One thing to discover per panel | Engine only, no panel uses it |
| 6 | Peer model first | Built |
| 7 | Move quizzes to the end | Partly: mid-story quizzes removed, picture recap not built |
| 8 | No fail language | Built, with Dr. Iris's reminder replacing "Ready when you are" |
| 9 | Caregiver prompts | Not built |
| 10 | Name and avatar | Built, using the avatar and not the name, as planned |
| 11 | Under 5 minutes, about 12 panels | Partly: 14 panels by agreement, full play time not timed |

The plan's automated checks exist in [StoryContentTests.cs](src/EyeCanDo/Assets/_Project/Tests/EditMode/StoryContentTests.cs). No results from the CoDHP audit or the comparison with children are recorded in the repository.

## Status of every design principle

| # | Principle | Status | Note |
| --- | --- | --- | --- |
| P1 | Rehearse the body actions | Built | Hold and blink, for the one level that exists |
| P2 | Take it off the screen | Not built | No "try it with your grown-up" activity |
| P3 | Show the visit in order | Partly | Order within the drops level only, no whole-visit view |
| P4 | Peer model who copes | Partly | Sam goes first and is fine, but shows no coping strategy |
| P5 | Teach coping tools | Not built | No breathing, counting or chosen plan |
| P6 | Honest sensory information | Partly | Feel is covered. Blur is shown but not spoken. Bright light is not mentioned |
| P7 | Realistic world | Partly | Room and props are real, characters are not human |
| P8 | Give the child control | Partly | Home, replay, pause by letting go and dress-up. No role-play mode |
| P9 | Short, focused, no clutter | Built | No hotspots active, and rewards come at the end |
| P10 | Prompt the caregiver | Not built | Also Plan 9 |
| P11 | Specific praise, symbolic rewards | Partly | Sticker fits, praise is general, coins are in question |
| P12 | Adapt to age, not reading | Partly | Narration first, but no difference between ages 3-4 and 5-7 |
| P13 | Design for after the drops | Not built | |
| P14 | Repeat close to the day | Partly | Play again exists, with no caregiver guidance on timing |
| P15 | Test with real children | Not built | Only automated and smoke tests so far |

## Features with no direct research basis

These are design choices. They should not be presented as evidence-based.

- Brow moods and per-line facial expressions.
- Lip sync and procedural animation.
- The scout art style and the variety of hats, fits, eyes and mouths.
- Three save slots.
- The less-motion setting.

## Limits of the evidence

- **No outcome data.** Every claim here concerns design inputs. Learning, anxiety and compliance have not been measured for this game.
- **Storybooks are not games.** Takacs 2015 studied digital storybooks. Applying it to a 3D game is an extension.
- **Adult evidence on narration.** Mayer's 17 of 17 result comes from adult learners. Evidence in children is thinner and mixed.
- **Anxiety studies test distraction.** The game trials (Luo, JMIR 2025 and JAMA Pediatrics 2025) mostly study games used as distraction on the day, with a mean age of 7.4 in the larger review. EyeCanDo is preparation before the day, for ages 3 to 7.
- **Alfieri 2011** covers instruction broadly, not children's games.
- **The Patchbook** was one small trial, with compliance measured in 88 of 120 children. An AAO summary of later work with the cartoon reports a smaller change in low-income areas (52% to 62.3%, P = 0.146).
- **CoDHP count.** PoCResearch.md describes "15 validated design principles". The paper's framework has 47 design principles, 15 of them highlighted.

## Sources

"Confirmed" means the figures used here were matched on Nov 8, 2026 to the source or its published abstract. "Carried over" means they come from the team documents and were not rechecked.

| Source | Used for | Check |
| --- | --- | --- |
| [Asadzadeh et al. 2024, *Heliyon*](https://pmc.ncbi.nlm.nih.gov/articles/PMC10963373/) (CoDHP) | Design principles | Confirmed |
| [Takacs, Swart and Bus 2015, *Review of Educational Research*](https://journals.sagepub.com/doi/pdf/10.3102/0034654314566989) | Narration, multimedia, distraction | Confirmed |
| [Alfieri et al. 2011, *Journal of Educational Psychology*](https://www.fisme.science.uu.nl/publicaties/literatuur/2011_alfieri_discovery_learning.pdf) | Guided over open play | Confirmed |
| [Mayer, modality principle, *Multimedia Learning*](https://www.cambridge.org/core/books/multimedia-learning/modality-principle/E5CD6E01CEA0B568CE260F66A3CD0D1F) | Narration over text | Confirmed |
| [Luo et al. 2025, *JMIR Serious Games*](https://games.jmir.org/2025/1/e67007) | Games and anxiety | Confirmed from abstract text in search results |
| [Luo et al. 2025, *JAMA Pediatrics*](https://pmc.ncbi.nlm.nih.gov/articles/PMC12439185/) | 2D games and anxiety | Confirmed |
| Melamed and Siegel 1975, *J Consult Clin Psychol* 43:511-521 | Peer modelling | Citation confirmed, findings from a secondary table |
| [Tjiam et al. 2013, *Graefes Arch Clin Exp Ophthalmol* 251:321-329](https://pure.eur.nl/en/publications/randomised-comparison-of-three-tools-for-improving-compliance-wit/) | Child's-view story | Citation confirmed, compliance figures carried over (abstract not accessible) |
| [AAO summary of the educational cartoon](https://www.aao.org/editors-choice/educational-cartoon-improves-occlusion-therapy-res) | Later low-income results | Confirmed |
| Habgood 2007, Hirsh-Pasek 2015, Lepper 1973, Ganea 2014, Richert and Walker, CliniPup 2019, Chauhan 2024 | Integration, rewards, realism, replay, orthoptist views | Carried over, full links in EducationalGameResearch.md section 10 |
</content>
</invoke>
<invoke name="Bash">
<parameter name="command">cd "/c/Users/jakef/Repositories/hindsight-fork/3d-poc-v2"
python3 -I - <<'E'
import re,os
s=open('PoCResearchToFeatures.md',encoding='utf8').read()
bad=0;n=0
for m in re.finditer(r'\]\((src/[^)#]+|PoCResearch\.md)(#L(\d+)(?:-L(\d+))?)?\)',s):
    p,_,a,b=m.groups();n+=1
    if not os.path.exists(p): print('MISSING',p);bad+=1;continue
    if a:
        c=sum(1 for _ in open(p,encoding='utf8',errors='ignore'))
        if int(b or a)>c: print('RANGE',p,a,b,c);bad+=1
print(n,'links',bad,'bad')
E
git add PoCResearchToFeatures.md
git status --short