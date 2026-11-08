# PoC Research to Features: EyeCanDo 3D PoC v2

Nov 8, 2026

## Purpose

This document links published research to the features in the EyeCanDo 3D PoC v2 build. It shows what each feature is based on, how strong that evidence is, and where the build departs from the research.

Every research claim links to its original source. Every claim about the game links to the code or script on the `3d-poc-v2` branch. The game is aimed at children aged 3 to 7.

The research supports the design. It does not show that this game works: **no child has played this build in a study yet.**

## Summary

| Measure | Result |
| --- | --- |
| Research recommendations reviewed | 25: 12 built, 6 partly built, 7 not built |
| Captions | 19 lines, 90 words in total, longest 8 words ([script](src/EyeCanDo/Assets/_Project/Audio/Narration/NarrationScript.md)) |
| Spoken narration | 19 clips, 68 seconds in total, 226 words ([recordings](src/EyeCanDo/Assets/_Project/Audio/Narration/)) |
| Fail states | None ([HoldStillStep.cs:51-57](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/HoldStillStep.cs#L51-L57)) |

**Best supported:** a guided storyboard, a peer who goes first, multimedia that illustrates the explanation, and no failure.

**Largest gaps:** no caregiver role, no coping skill, no off-screen practice, non-human characters, and a repeating coin reward.

## Evidence ratings

| Rating | Meaning |
| --- | --- |
| **Strong** | A meta-analysis, or an attribute highlighted in a validated framework |
| **Moderate** | A controlled study, or an expert recommendation inside a validated framework |
| **Limited** | A small, pilot or qualitative study, or a study of a different age or setting |
| **Guidance** | Advice from a children's hospital or eye-care provider, not an outcome study |

## Why prepare children for eye drops

- Eye drops can be "a distressing experience for children, parents/guardians and orthoptists" ([Chauhan 2024][chauhan]).
- Children under five are more likely to be distressed, and children expect drops to be worse than they are (Hirji et al. and Pilon et al., as reported in [Chauhan 2024][chauhan]).
- That study recommends pre-procedure videos. Only one department in it used one ([Chauhan 2024][chauhan]).
- Children who played a web game two days before surgery had lower anxiety on the day (20 children aged 6 to 10, P = .01) ([Buffel 2019][buffel]).
- Video games lowered anxiety at parent separation more than midazolam (6 trials, 612 children, SMD -0.31) ([Luo, JMIR 2025][luo-jmir]).
- 2D games lowered children's preoperative anxiety against control (SMD -1.02) and parents' anxiety too (SMD -1.59) (49 trials, 4,535 children) ([Luo, JAMA Pediatrics 2025][luo-jama]).

## Feature map

Code paths are under `src/EyeCanDo/Assets/_Project/`.

### Story structure

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Fixed storyboard | 14 panels play in one order, with no free camera | [ProcedureRunner.cs:51-56](src/EyeCanDo/Assets/_Project/Scripts/Procedure/ProcedureRunner.cs#L51-L56) | Explicit teaching beat unassisted discovery (d = -0.38). Assisted discovery beat other methods (d = 0.30). "Feedback, worked examples, scaffolding, and elicited explanations" help ([Alfieri 2011][alfieri]) | Strong, for education in general |
| One shot, one beat | Each panel has one framed shot, one or two lines and at most one action | [PanelStep.cs:49-104](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/PanelStep.cs#L49-L104) | "Having an engaging, clear, and fast-paced storyline" ([Asadzadeh 2024][codhp]) | Strong |
| Progress dots | A wordless dot for each panel shows done, now and to do | [LevelHud.cs:57-64](src/EyeCanDo/Assets/_Project/Scripts/UI/LevelHud.cs#L57-L64) | A "clear and predictable sequence" of stops helped patients tolerate eye assessments, in a pilot ([Medical Independent][social-stories]) | Limited |
| No quizzes in the story | No step stops the story to ask a question | [PocBuilder.cs:184-232](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L184-L232) | Children learned more from a game where maths was the mechanic than from one with a separate quiz ([Habgood and Ainsworth 2011][habgood]). Interactive extras added no significant comprehension benefit in stories ([Takacs 2015][takacs]) | Moderate |
| Short level | 68 seconds of speech. Full play time has not been timed | [NarrationScript.md](src/EyeCanDo/Assets/_Project/Audio/Narration/NarrationScript.md) | "Keep your explanations about the procedure to 10 or 15 minutes" ([UF Health][uf]). "Use age-appropriate interaction time" ([Asadzadeh 2024][codhp]) | Guidance |

### Narration and few words

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Every line spoken | Each story line has its own recording, and a test fails if one is missing | [NarrationPlayer.cs:112-146](src/EyeCanDo/Assets/_Project/Scripts/UI/NarrationPlayer.cs#L112-L146), [StoryContentTests.cs:123](src/EyeCanDo/Assets/_Project/Tests/EditMode/StoryContentTests.cs#L123) | Children typically "read familiar stories" at 6 to 7, and only "match some spoken and written words" at 5 ([KidsHealth][kidshealth]). Narration beat on-screen text in 17 of 17 tests, median d = 1.02 ([Mayer][mayer]) | Moderate, see limits |
| Short captions | One caption of 8 words or fewer, which can be turned off | [StoryLine.cs:23](src/EyeCanDo/Assets/_Project/Scripts/Procedure/StoryLine.cs#L23), [CaptionView.cs:132-137](src/EyeCanDo/Assets/_Project/Scripts/UI/CaptionView.cs#L132-L137) | "Using simple words and commands" ([Asadzadeh 2024][codhp]) | Strong |
| Replay button | Replays the current line, even while it is playing | [CaptionView.cs:112-118](src/EyeCanDo/Assets/_Project/Scripts/UI/CaptionView.cs#L112-L118) | "Clear, repeatable verbal descriptions and visual presentations of content" (Lieberman et al., in [Asadzadeh 2024][codhp]) | Moderate |
| Pictures timed to speech | Each chart picture lights up as Dr. Iris says its cue word | [ChartStep.cs:48-58](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/ChartStep.cs#L48-L58) | Multimedia added to narrated stories improved comprehension (g = 0.39). It may not help unless it draws attention to details that illustrate the text ([Takacs 2015][takacs]) | Strong, for storybooks |
| Sound effects | Sound accompanies the drop, blinks and reward | [HoldStillStep.cs:37](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/HoldStillStep.cs#L37), [BlinkStep.cs:35](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/BlinkStep.cs#L35) | "Animated pictures, music, and sound effects were found beneficial" ([Takacs 2015][takacs]) | Strong, for storybooks |
| Speaker portraits and icons | A live portrait shows who is talking, and the action buttons use pictures | [CaptionView.cs:120-130](src/EyeCanDo/Assets/_Project/Scripts/UI/CaptionView.cs#L120-L130), [PocBuilder.Ui.cs:135-142](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.Ui.cs#L135-L142) | "Use the meaningful icons as a replacement or help to texts" ([Asadzadeh 2024][codhp]). The successful Patchbook story explained itself "without words" ([Tjiam 2013][tjiam]) | Moderate |

### Rehearsing the real behaviour

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Hold to keep the eye open | The child holds a button while the drop falls | [HoldStillStep.cs:34-61](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/HoldStillStep.cs#L34-L61) | When the lesson was the mechanic, children learned more and played 7 times longer ([Habgood and Ainsworth 2011][habgood]). Most therapeutic play studies found less anxiety and more cooperation, though the evidence "is still questionable" ([Silva 2017][silva]) | Limited, ages 7 to 11 for the game study |
| Blink three times | Each tap blinks and clears the blur a little | [BlinkStep.cs:29-40](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/BlinkStep.cs#L29-L40) | As above | Limited |
| First-person chair view | The child looks up from the chair | [HoldStillStep.cs:18](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/HoldStillStep.cs#L18) | "Relating the story to the real-world players' experiences" ([Asadzadeh 2024][codhp]) | Strong |
| Real room and props | Chair, bottle, light and white coat are recognisable | [PrefabBuilder.cs](src/EyeCanDo/Assets/_Project/Editor/PrefabBuilder.cs) | Preschoolers were more likely to generalise from a story when its world was similar to reality (108 children) ([Walker 2015][walker]) | Moderate |

### Modelling and honest information

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Sam goes first | Panels 4 to 8 show a child getting drops before the player, and a test enforces the order | [PocBuilder.cs:194-213](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L194-L213), [StoryContentTests.cs:95](src/EyeCanDo/Assets/_Project/Tests/EditMode/StoryContentTests.cs#L95) | Children who watched a preparation film had lower anxiety before surgery than those who watched a control film ([Melamed and Siegel 1975][melamed], [results table][melamed-table]). A cartoon story for children raised patching compliance from 55% to 89% (P = .002) ([Tjiam 2013][tjiam]). Demonstrate on a toy "what your child will experience" ([UF Health][uf]) | Moderate |
| Sam reacts, then is fine | "Oooh, cold!" comes first, then "Yay, that wasn't so bad!" | [PocBuilder.cs:202-213](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L202-L213) | Coping and mastery models both beat a placebo at ages 5 to 7, and neither beat the other ([dental study][dental]). So Sam's worry is supported, but not shown to be better than a calm model | Limited |
| Honest about the feeling | "They will be cold and may sting for a little bit" | [PocBuilder.cs:199](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L199), [PocBuilder.cs:218](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L218) | "Be honest with your child about any discomfort or pain" and "describe as best as you can how the test will feel" ([UF Health][uf]). See the word-choice conflict below | Guidance |
| Why the drops are used | The chart explains that drops make the pupil big so she can see inside | [PocBuilder.cs:191-193](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L191-L193) | The effective Patchbook cartoon explained "why they should patch" ([Tjiam 2013][tjiam]) | Moderate |
| Blur after the drops | A haze clears partly with each blink | [BlinkStep.cs:37](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/BlinkStep.cs#L37) | Drops may make close objects hard to see for around 4 to 6 hours ([Specsavers][specsavers]) | Guidance |

### Feedback and no failure

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Nothing can be failed | Letting go only pauses the drop in mid-air, and Dr. Iris repeats the reminder | [HoldStillStep.cs:51-57](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/HoldStillStep.cs#L51-L57) | "Providing achievable challenges" ([Asadzadeh 2024][codhp]) | Strong |
| No fail language | A test bans "wrong", "oops", "fail" and similar words | [StoryContentTests.cs:83](src/EyeCanDo/Assets/_Project/Tests/EditMode/StoryContentTests.cs#L83) | As above, applied to wording | Strong, by extension |
| Instant response | Tapped objects react at once, and the hold button fills as the drop falls | [TapTargetStep.cs:38-48](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/TapTargetStep.cs#L38-L48), [ActionButtonsView.cs:35](src/EyeCanDo/Assets/_Project/Scripts/UI/ActionButtonsView.cs#L35) | "Immediate feedback" ([Asadzadeh 2024][codhp]) | Strong |
| Two simple gestures | Tapping and holding are the only inputs in the level | [ActionButtonsView.cs](src/EyeCanDo/Assets/_Project/Scripts/UI/ActionButtonsView.cs) | "Using the minimum amount of mechanics" ([Asadzadeh 2024][codhp]) | Strong |

### Personalisation, rewards and control

| Feature | In the game | Code | Research | Evidence |
| --- | --- | --- | --- | --- |
| Own character | The child's dressed-up scout appears at their turn and at the reward | [ExamRoomContext.cs:78](src/EyeCanDo/Assets/_Project/Scripts/Scene/ExamRoomContext.cs#L78) | "Providing personalization options" ([Asadzadeh 2024][codhp]) | Strong |
| Dress-up outside the level | The shop is reached from level select, away from the teaching | [DressUpView.cs](src/EyeCanDo/Assets/_Project/Scripts/UI/DressUpView.cs) | Interactive extras add no significant comprehension benefit ([Takacs 2015][takacs]), so keeping these out of the story is a reasonable inference | Limited |
| Brave Eyes sticker | A sticker and dance end the level | [RewardStep.cs:26-42](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/RewardStep.cs#L26-L42) | "Granting rewards" ([Asadzadeh 2024][codhp]) | Strong |
| Coins | 40 the first time and 5 on replays, never taken away | [CoinRules.cs:11-12](src/EyeCanDo/Assets/_Project/Scripts/Core/Progress/CoinRules.cs#L11-L12) | Supported by "granting rewards" ([Asadzadeh 2024][codhp]), but challenged below | Conflicting |
| Leave at any time | The Home button is on screen throughout the level | [LevelHud.cs:26](src/EyeCanDo/Assets/_Project/Scripts/UI/LevelHud.cs#L26) | "Allow your child to make choices" ([CHOP][chop]), applied here to leaving | Guidance |

## Where the build departs from the research

| Topic | Research says | Build does | Suggested response |
| --- | --- | --- | --- |
| Characters | Anthropomorphic books led to less learning of facts in younger children ([Ganea 2014][ganea]). Children generalise more from worlds similar to reality ([Walker 2015][walker]) | Everyone is a round scout in a bright, non-skin colour ([BeanRig.cs](src/EyeCanDo/Assets/_Project/Scripts/Scene/Bean/BeanRig.cs)) | Test recognition with children, or move closer to human. These studies used animals and fantasy stories, so they apply only partly |
| Coins | Children who expected a reward showed less later interest in the activity ([Lepper 1973][lepper]). A reward calendar did not significantly raise compliance (67% against 55%, P = .301) ([Tjiam 2013][tjiam]) | A repeating coin payout funds a large shop | Decide with the supervisor. Consider making the sticker the main reward |
| Praise | Parents' praise of effort at 14 to 38 months predicted a belief at 7 to 8 years that ability can grow ([Gunderson 2013][gunderson]) | "You did it!", "Amazing!" and "Fantastic!" ([script](src/EyeCanDo/Assets/_Project/Audio/Narration/NarrationScript.md)) | Low-cost script change, for example "You kept your eye open!" |
| Waiting for the drops | Drops "take 30 to 45 minutes to work" ([SickKids][sickkids]) | Panels 12 to 13 go straight from blink to examination ([PocBuilder.cs:226-230](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L226-L230)) | Add one line about waiting |
| The word "sting" | Sources conflict. Hospital advice says to be honest about discomfort ([UF Health][uf]). Orthoptists avoid "sting" and prefer "tingly" ([Chauhan 2024][chauhan]) | Says "may sting" ([PocBuilder.cs:199](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L199)) | Match the partner clinic's wording |
| Spoken length | "Plain words" ([UF Health][uf]) and "mirroring the language of target children" ([Asadzadeh 2024][codhp]) | The chart line is 32 words and uses "pupil" ([PocBuilder.cs:192](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L192)) | Split the line and test the word |
| "Click" | "Simple words and commands" ([Asadzadeh 2024][codhp]) | Lines say "Click" although the game targets tablets ([PocBuilder.cs:189](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L189)) | Re-record with "Tap" |
| Sticker title | At 5, children typically only "match some spoken and written words" ([KidsHealth][kidshealth]) | "Brave Eyes!" has no voice line ([PocBuilder.cs:231](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L231)) | Add a line. The reward step already supports one |
| Interactive taps | Interactive features showed no significant benefit in stories ([Takacs 2015][takacs]) | Tapping Sam and the chair is required ([TapTargetStep.cs:26-59](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/TapTargetStep.cs#L26-L59)) | Keep them. They move the story forward, and that study covers storybooks |

**The research conflicts on discovery.** "Children prefer an explorative mode of play" (Kinzie and Joseph, in [Asadzadeh 2024][codhp]), and the framework highlights "allowing the players to discover the game storyline". Takacs found interactive features gave no significant benefit ([Takacs 2015][takacs]). The engine supports a glowing object to discover ([PanelStep.cs:68-79](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/PanelStep.cs#L68-L79)), but no panel uses it ([PocBuilder.cs:184-232](src/EyeCanDo/Assets/_Project/Editor/PocBuilder.cs#L184-L232)).

## Coverage of the research recommendations

| # | Recommendation | Source | Status | Note |
| --- | --- | --- | --- | --- |
| 1 | Make the action the lesson | [Habgood and Ainsworth 2011][habgood] | Built | Hold and blink |
| 2 | Guide the learner | [Alfieri 2011][alfieri] | Built | Fixed storyboard |
| 3 | Keep interactive extras out of the story | [Takacs 2015][takacs] | Built | No discovery objects or quizzes in use |
| 4 | Use multimedia that illustrates the words | [Takacs 2015][takacs] | Built | Chart timing, animation, sound |
| 5 | Show a model first | [Melamed and Siegel 1975][melamed], [Tjiam 2013][tjiam] | Built | Sam |
| 6 | Say honestly how it will feel | [UF Health][uf] | Built | Cold, may sting |
| 7 | Keep explanations short | [UF Health][uf] | Built | Full play time not timed |
| 8 | Give immediate feedback | [Asadzadeh 2024][codhp] | Built | |
| 9 | Provide achievable challenges | [Asadzadeh 2024][codhp] | Built | |
| 10 | Use minimum mechanics | [Asadzadeh 2024][codhp] | Built | |
| 11 | Offer personalisation | [Asadzadeh 2024][codhp] | Built | |
| 12 | Grant rewards | [Asadzadeh 2024][codhp] | Built | |
| 13 | Use plain language | [UF Health][uf], [Asadzadeh 2024][codhp] | Partly | Short captions, some long spoken lines |
| 14 | Use icons and recognition over recall | [Asadzadeh 2024][codhp] | Partly | Level uses icons, some shop cards show names |
| 15 | Show a predictable sequence | [Medical Independent][social-stories] | Partly | Within the drops level only, not the whole visit |
| 16 | Use a realistic world | [Walker 2015][walker], [Ganea 2014][ganea] | Partly | Room and props are real, characters are not |
| 17 | Offer choices | [UF Health][uf], [CHOP][chop] | Partly | Dress-up and leaving, no choice within the procedure |
| 18 | Be careful with expected rewards | [Lepper 1973][lepper] | Partly | Sticker is symbolic, coins repeat |
| 19 | Praise effort or action | [Gunderson 2013][gunderson] | Not built | Praise is general |
| 20 | Explain the wait for the drops | [SickKids][sickkids] | Not built | |
| 21 | Prepare for light sensitivity | [SickKids][sickkids] | Not built | Blur is shown, light sensitivity is not |
| 22 | Say it is not a punishment | [UF Health][uf] | Not built | No such line in the [script](src/EyeCanDo/Assets/_Project/Audio/Narration/NarrationScript.md) |
| 23 | Involve the caregiver | [Asadzadeh 2024][codhp], [Rasmussen 2016][rasmussen] | Not built | Gains in that study came with parent conversation |
| 24 | Practise off-screen | [Barr 2010][barr] | Not built | Children learn less from screens than live until at least age 3. Linking this to practice is our inference |
| 25 | Teach a coping skill beforehand | [CHOP][chop] | Not built | Breathing, distraction, comfort positions |

## Features with no direct research basis

These are design choices. They should not be presented as evidence-based.

- The quiet pause after each line ([NarrationPlayer.cs:149](src/EyeCanDo/Assets/_Project/Scripts/UI/NarrationPlayer.cs#L149)).
- Showing the hold button only after the instructions ([PanelStep.cs:87-93](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/PanelStep.cs#L87-L93)).
- The Play again button ([RewardStep.cs:41](src/EyeCanDo/Assets/_Project/Scripts/Procedure/Steps/RewardStep.cs#L41)).
- Brow moods, lip sync and procedural animation ([BeanRig.cs](src/EyeCanDo/Assets/_Project/Scripts/Scene/Bean/BeanRig.cs)).
- The scout art style and the range of cosmetics ([AvatarCatalog.cs](src/EyeCanDo/Assets/_Project/Scripts/Core/Avatar/AvatarCatalog.cs)).
- Three save slots, and the settings for big text, auto-continue and less motion ([GameSettings.cs:17-32](src/EyeCanDo/Assets/_Project/Scripts/Core/Progress/GameSettings.cs#L17-L32)).

## Limits of the evidence

- **No outcome data.** Learning, anxiety and compliance have not been measured for this game.
- **Narration against text is untested at this age.** [Takacs 2015][takacs] included oral narration in both study conditions, so it cannot show that narration beats text. The [Mayer][mayer] chapter summary does not give the learners' ages. The case for narration here rests mainly on children's [reading ability][kidshealth].
- **Storybooks are not games.** [Takacs 2015][takacs] covers stories for ages 3 to 10. Its interactive-feature effects were not statistically significant (g = -0.21 to -0.03), although its authors describe those features as distracting.
- **Anxiety trials test distraction.** Both Luo reviews treat games as distraction, not preparation, and the larger one has a mean age of 7.42 ([JMIR][luo-jmir], [JAMA Pediatrics][luo-jama]).
- **Age mismatch.** The [Habgood and Ainsworth][habgood] game study was of 7 to 11-year-olds (58 children for learning, 16 for free play).
- **Discovery research is general.** [Alfieri 2011][alfieri] covers education broadly, not children's games.
- **The Patchbook result is one trial.** Compliance was measured in 88 of 120 children ([Tjiam 2013][tjiam]). A later rollout of the cartoon with orthoptist training reported a smaller change in low-income areas (52% to 62.3%, P = .146) ([AAO summary][aao]).
- **Modelling is not the strongest technique.** A review of 75 controlled studies of preparation found an overall effect of 0.44, with modelling below average ([Saile 1988][saile]).
- **Small or indirect studies.** [Chauhan 2024][chauhan] interviewed 8 orthoptists and says its findings cannot be generalised. [Buffel 2019][buffel] had 20 children. [Gunderson 2013][gunderson] followed 53 children and shows an association. The social-stories report is a [pilot][social-stories] with qualitative results.

## Sources

"Full text" and "Abstract" mean the figures here were read from that version on Nov 8, 2026. "Excerpt" means they were read from the source's text as shown in search results, because the page itself could not be opened.

| Source | Check |
| --- | --- |
| [Asadzadeh A et al. Serious educational games for children: A comprehensive framework. *Heliyon*, 2024][codhp] | Full text |
| [Takacs ZK, Swart EK, Bus AG. Benefits and pitfalls of multimedia and interactive features in technology-enhanced storybooks. *Review of Educational Research*, 2015][takacs] | Full text |
| [Alfieri L et al. Does discovery-based instruction enhance learning? *Journal of Educational Psychology*, 2011][alfieri] | Abstract |
| [Mayer RE. Modality principle. *Multimedia Learning*, Cambridge University Press][mayer] | Chapter summary |
| [Habgood MPJ, Ainsworth SE. Motivating children to learn effectively. *Journal of the Learning Sciences*, 2011][habgood] | Excerpt |
| [Melamed BG, Siegel LJ. Reduction of anxiety in children facing hospitalization and surgery by use of filmed modeling. *J Consult Clin Psychol*, 1975][melamed] | Citation record |
| [Review table reporting Melamed and Siegel's results][melamed-table] | Excerpt |
| [Tjiam AM et al. Randomised comparison of three tools for improving compliance with occlusion therapy. *Graefes Arch Clin Exp Ophthalmol*, 2013][tjiam] | Abstract |
| [AAO summary of the educational cartoon rollout, *Ophthalmology*, 2012][aao] | Summary page |
| [Coping and mastery models in dental stress, *Australian Journal of Psychology*, 1985][dental] | Excerpt |
| [Saile H et al. Meta-analysis of psychological preparation of children facing medical procedures. *Psychology and Health*, 1988][saile] | Excerpt |
| [Silva RD et al. Therapeutic play to prepare children for invasive procedures. *Jornal de Pediatria*, 2017][silva] | Abstract |
| [Walker CM, Gopnik A, Ganea PA. Learning to learn from stories. *Child Development*, 2015][walker] | Abstract |
| [Ganea PA et al. Do cavies talk? *Frontiers in Psychology*, 2014][ganea] | Abstract |
| [Barr R. Transfer of learning between 2D and 3D sources during infancy. *Developmental Review*, 2010][barr] | Abstract |
| [Lepper MR, Greene D, Nisbett RE. Undermining children's intrinsic interest with extrinsic reward. *JPSP*, 1973][lepper] | Excerpt |
| [Gunderson EA et al. Parent praise to 1- to 3-year-olds predicts children's motivational frameworks 5 years later. *Child Development*, 2013][gunderson] | Abstract |
| [Rasmussen EE et al. Active mediation, *Daniel Tiger's Neighborhood* and preschoolers' development. *Journal of Children and Media*, 2016][rasmussen] | Excerpt |
| [Chauhan H. The orthoptist experience of instilling eye drops in children. *British and Irish Orthoptic Journal*, 2024][chauhan] | Full text |
| [Buffel C et al. CliniPup pilot randomized controlled trial. *JMIR Serious Games*, 2019][buffel] | Abstract |
| [Luo Z et al. Video games versus midazolam in reducing perioperative anxiety. *JMIR Serious Games*, 2025][luo-jmir] | Abstract |
| [Luo Z et al. Digital health interventions in pediatric perioperative care. *JAMA Pediatrics*, 2025][luo-jama] | Full text |
| [UF Health. Preschooler test or procedure preparation][uf] | Guidance page |
| [Children's Hospital of Philadelphia. Coping with healthcare procedures][chop] | Guidance page |
| [SickKids. Eye Clinic][sickkids] | Guidance page |
| [Specsavers. Cycloplegic eye drops][specsavers] | Excerpt |
| [KidsHealth. Reading milestones][kidshealth] | Guidance page |
| [Medical Independent. Project helps autistic patients prepare for eye appointments][social-stories] | Article |

[codhp]: https://pmc.ncbi.nlm.nih.gov/articles/PMC10963373/
[takacs]: https://pmc.ncbi.nlm.nih.gov/articles/PMC4647204/
[alfieri]: https://www.fisme.science.uu.nl/publicaties/literatuur/2011_alfieri_discovery_learning.pdf
[mayer]: https://www.cambridge.org/core/books/multimedia-learning/modality-principle/E5CD6E01CEA0B568CE260F66A3CD0D1F
[habgood]: https://nottingham-repository.worktribe.com/output/23577510
[melamed]: https://doi.org/10.1037/h0076896
[melamed-table]: https://pmc.ncbi.nlm.nih.gov/articles/PMC4884908/table/jsv094-T2
[tjiam]: https://europepmc.org/article/MED/22820813
[aao]: https://www.aao.org/editors-choice/educational-cartoon-improves-occlusion-therapy-res
[dental]: https://www.scilit.com/publications/4c5d7856a581a3089177be8f0c04420d
[saile]: https://informahealthcare.com/doi/abs/10.1080/08870448808400347
[silva]: https://europepmc.org/article/MED/27485756
[walker]: https://europepmc.org/article/MED/25156667
[ganea]: https://pmc.ncbi.nlm.nih.gov/articles/PMC3989584/
[barr]: https://pmc.ncbi.nlm.nih.gov/articles/PMC2885850/
[lepper]: https://typeset.io/pdf/undermining-children-s-intrinsic-interest-with-extrinsic-3qn44yqiou.pdf
[gunderson]: https://pmc.ncbi.nlm.nih.gov/articles/PMC3655123/
[rasmussen]: https://scholars.ttu.edu/en/publications/relation-between-active-mediation-exposure-to-daniel-tigers-neigh-12
[chauhan]: https://pmc.ncbi.nlm.nih.gov/articles/PMC11328678/
[buffel]: https://pmc.ncbi.nlm.nih.gov/articles/PMC6592396/
[luo-jmir]: https://games.jmir.org/2025/1/e67007
[luo-jama]: https://pmc.ncbi.nlm.nih.gov/articles/PMC12439185/
[uf]: https://ufhealth.org/conditions-and-treatments/preschooler-test-or-procedure-preparation
[chop]: https://www.chop.edu/health-resources/coping-healthcare-procedures
[sickkids]: https://www.sickkids.ca/en/care-services/clinics/eye-clinic/
[specsavers]: https://www.specsavers.co.uk/eye-test/cycloplegics
[kidshealth]: https://kidshealth.org/AkronChildrensXML/en/parents/milestones.html?view=ptr
[social-stories]: https://www.medicalindependent.ie/in-the-news/conference/new-project-helps-autistic-patients-prepare-for-eye-appointments/
