# PoC Research: Evidence-Based Design for Kids Learning

Nov 6, 2026 · Jake Finlay

## Purpose

The next PoC rebuilds the 3D Eye Drops level as a narrated storyboard with far fewer words. Every change is tied to published evidence.

Feedback on the earlier PoCs asked for four things:

1. Less words on screen.
2. Narration.
3. A storyboard flow instead of free roaming.
4. Research that shows the techniques will work.

The core source is the CoDHP framework for children's serious educational games (Asadzadeh et al., 2024). Supporting studies fill the gaps it leaves on narration and guidance.

## The source article

[Serious educational games for children: A comprehensive framework](https://pmc.ncbi.nlm.nih.gov/articles/PMC10963373/) (Asadzadeh et al., *Heliyon* 10, e28108, 2024) gives 15 validated design principles for games aimed at ages 3 to 12.

**How it was built.** A scoping review screened 9,468 records down to 15 studies. That produced 105 principles. A panel of 15 PhD-level experts (7+ years each) scored them on a 1 to 9 scale. Every item scored above 7 in round one. The experts added 21 more principles.

**Principles that matter most for this PoC:**

| Principle (CoDHP) | What it means for us |
| --- | --- |
| Engaging, clear, fast-paced storyline | Short scenes that always move forward |
| Relate the story to real-world experience | Show the real clinic, chair, bottle and doctor |
| Use simple words, mirror the child's language | One short sentence per line |
| Clear, repeatable verbal descriptions and visuals | Narration with a replay button |
| Meaningful icons to support or replace text | Picture answers, icon buttons |
| Recognition over recall | Pick a picture, never type or remember |
| Immediate feedback (text, audio or video) | Voice and animation respond to every action |
| Achievable challenges | No fail state, only "try again" |
| Minimum necessary mechanics | Three gestures: tap, hold, drag |
| Social interaction, engage parents | A caregiver can play along |
| Personalisation | The child's name and chosen avatar |
| Age-appropriate interaction time | One level fits a preschool attention span |

**One tension.** The paper notes children prefer an explorative mode of play. Our feedback asks for a storyboard. We resolve this below: the story is linear, but each scene has a small thing to discover. The paper itself says players should "discover the storyline", not wander outside it.

## Evidence for each feedback item

Each requested change has direct support. The strength varies, so it is rated in the last column.

| Feedback | Evidence | Strength |
| --- | --- | --- |
| Less words | CoDHP: simple words, icons to replace text, recognition over recall ([Asadzadeh 2024](https://pmc.ncbi.nlm.nih.gov/articles/PMC10963373/)). Most of our players (3 to 7) cannot read yet. | Strong, expert consensus |
| Add narration | Digital storybook meta-analysis, 43 studies, 2,147 children: narration and matching visuals help comprehension. Games and hotspots inside the story distract ([Takacs 2015](https://journals.sagepub.com/doi/pdf/10.3102/0034654314566989)). Narration beat on-screen text in 17 of 17 adult tests ([Mayer, modality principle](https://www.cambridge.org/core/books/multimedia-learning/modality-principle/E5CD6E01CEA0B568CE260F66A3CD0D1F)). | Strong for storybooks, moderate for games |
| Storyboard, not free roam | 164-study meta-analysis: unassisted discovery lost to explicit instruction (d = -0.38). Guided discovery beat other methods (d = 0.30) ([Alfieri 2011](https://www.fisme.science.uu.nl/publicaties/literatuur/2011_alfieri_discovery_learning.pdf)). | Strong |
| Story told through a child | Patchbook RCT, 120 children aged 3 to 6: a cartoon story from a child's view raised patching compliance to 89% vs 55% ([Tjiam et al.](https://pure.eur.nl/en/publications/randomised-comparison-of-three-tools-for-improving-compliance-wit/)). | Moderate, single RCT |
| Prove it reduces anxiety | 6 RCTs, 612 children: games lowered anxiety at parent separation (SMD -0.31) ([Luo 2025](https://games.jmir.org/2025/1/e67007)). | Moderate to high |

**What this means.** Narration plus pictures teaches pre-readers better than text. A guided path teaches better than open roaming. Extra interactive bits inside the teaching part should go.

## Baseline: the 3D PoC

The 3D PoC (`feat/3d-eye-drop-poc`) already runs as a fixed 14-step sequence with camera cuts. Its main gaps are text volume and no audio.

**Keep:**

- Linear, data-driven steps (`ProcedureDefinition` + `ProcedureStep`). This is already a storyboard engine.
- Cinemachine shot per step. Each step is a framed panel.
- First-person view in the chair. The child sees what they will really see.
- Real props: chair, bottle, ophthalmoscope, doctor in a white coat.
- Hold-to-keep-eye-open and blink mechanics. These rehearse the real behaviour.

**Fix:**

| Gap | Today | Research says |
| --- | --- | --- |
| Too many words | About 450 words of on-screen text, up to 3 sentences per line | Simple words, icons over text (CoDHP) |
| No narration | Text only, no audio or voice-over | Narration + visuals help comprehension (Takacs) |
| Quizzes mid-story | Two text quizzes stop the flow at step 6 and 7 | Embedded questions distract (Takacs) |
| No peer model | Only the doctor explains | A child character models the visit (Patchbook, Melamed and Siegel) |
| Text-heavy quiz answers | Answers are full sentences with icons | Recognition over recall, icons replace text (CoDHP) |
| "We'll have to do it again" on a blink | Reads as failure | Achievable challenges, no loss (CoDHP) |
| Solo play | No caregiver role | Social interaction, engage parents (CoDHP) |

## Plan: changes to the 3D PoC

The new PoC keeps the 3D engine and changes how each step talks to the child. Target: under 120 on-screen words, every line narrated.

| # | Change | How in code | Source |
| --- | --- | --- | --- |
| 1 | Narrate every line | Add an `AudioClip` field to each step line. A `NarrationPlayer` plays it and waits before advancing. | Takacs 2015, CoDHP |
| 2 | Cut text to captions | One line, 8 words max, shown as an optional caption. Captions can be turned off. | CoDHP: simple words |
| 3 | Replay button | A speaker icon replays the current line. | CoDHP: repeatable descriptions |
| 4 | Storyboard panels | Each step = one framed shot + one narrated beat. A row of dots shows progress. No free camera. | Alfieri 2011 |
| 5 | One thing to discover per panel | A single glowing object to tap (the bottle, the pupil, the light). It plays a short narrated fact. | CoDHP: discover the storyline |
| 6 | Peer model first | A child character ("Sam") gets drops first. Then it is the player's turn. | Patchbook, Melamed and Siegel 1975 |
| 7 | Move quizzes to the end | Replace the two mid-story quizzes with a picture recap after the reward. Answers are icons only, spoken aloud. | Takacs 2015, CoDHP: recognition |
| 8 | No fail language | A blink pauses with "Ready when you are." The drop never misses on screen. | CoDHP: achievable challenges |
| 9 | Caregiver prompts | A short line at key beats: "Grown-up, tilt your head back too!" | CoDHP: social interaction |
| 10 | Name and avatar | Narration uses the child's chosen character, not their typed name, so no voice per name is needed. | CoDHP: personalisation |
| 11 | Under 5 minutes | Cap the level at about 12 panels. This sits well inside the 10 to 15 minute preschool attention span from child-life guidance. | CoDHP: interaction time |

**Narration source.** For the PoC, record a team member reading the script. A TTS voice is a fallback. Final audio would need a child-friendly voice actor.

## Storyboard

The level runs as 12 narrated panels in a fixed order. Captions are the only text on screen.

| Panel | Shot | Narration (spoken) | Caption | Child does |
| --- | --- | --- | --- | --- |
| 1 | Doctor waves | "Hi! I'm Dr. Iris. I check eyes." | Hi! I'm Dr. Iris | Watch |
| 2 | Sam in waiting room | "This is Sam. Sam is getting eye drops today." | Meet Sam | Tap Sam to wave |
| 3 | Pupil close-up | "Drops make your pupil big. Then I can see inside." | Drops make it big | Tap the pupil, watch it grow |
| 4 | Sam in chair | "Watch Sam. Head back. Eyes open." | Head back, eyes open | Watch |
| 5 | Drop lands on Sam | "Cold! It stings a little. Then it stops." | Cold, then okay | Watch |
| 6 | Sam blinks, smiles | "Sam blinks. All done!" | Blink, blink | Watch |
| 7 | Chair, wide | "Your turn! Tap the chair." | Your turn | Tap the chair |
| 8 | First person, tilt back | "Look up at the star. Hold the button." | Hold the button | Press and hold |
| 9 | Drop falls | "Here it comes... cold! You did it." | You did it! | Keep holding |
| 10 | Blur overlay | "Blink, blink, blink. It's blurry for a bit." | Blink 3 times | Tap 3 times |
| 11 | Ophthalmoscope light | "Bright light. It doesn't hurt. Follow it!" | Follow the light | Tap the light |
| 12 | Reward | "Brave eyes! You're ready." | Brave Eyes! | Collect sticker |
| Recap | Picture cards | "What is your job? Tap the picture." | (icons only) | Pick 1 of 3 pictures |

Captions total about 40 words, against about 450 today. Each panel is one shot, one spoken beat and at most one action. A blink in panel 8 pauses with "Ready when you are." A caregiver prompt plays at panels 4 and 8.

## How we will show it works

The PoC is checked in three layers: automated rules, a design audit, and a small comparison with children.

**1. Automated checks (EditMode tests)**

- Every step line has an audio clip. Test fails if one is missing.
- No caption is longer than 8 words. Total captions stay under 120 words.
- No step can end in a fail state.

**2. CoDHP audit**

- Score the PoC against the 15 highlighted CoDHP attributes. Do the same for the old 3D PoC.
- A supervisor or child-life specialist reviews the scores.

**3. Small comparison with children**

- Two versions: the old text build and the new narrated storyboard. Each child plays one.
- Afterwards, ask 3 picture questions: why drops, what is your job, how it feels. Record correct answers.
- Record finish rate, time to finish, and how often a caregiver had to read text aloud.
- Optional: a smiley-face rating of how worried the child feels about drops, before and after.

All sessions with children need ethics approval and caregiver consent first. This lines up with the usability tests in the VnV plan.

**Open question:** can we reach 5 to 10 children aged 3 to 7 through the clinic before the PoC demo?

## References

1. Asadzadeh A, Shahrokhi H, Shalchi B, Khamnian Z, Rezaei-Hachesu P. [Serious educational games for children: A comprehensive framework](https://pmc.ncbi.nlm.nih.gov/articles/PMC10963373/). *Heliyon* 10, e28108, 2024. doi:10.1016/j.heliyon.2024.e28108
2. Alfieri L, Brooks PJ, Aldrich NJ, Tenenbaum HR. [Does discovery-based instruction enhance learning?](https://www.fisme.science.uu.nl/publicaties/literatuur/2011_alfieri_discovery_learning.pdf) *Journal of Educational Psychology*, 2011.
3. Takacs ZK, Swart EK, Bus AG. [Benefits and pitfalls of multimedia and interactive features in technology-enhanced storybooks](https://journals.sagepub.com/doi/pdf/10.3102/0034654314566989). *Review of Educational Research*, 2015.
4. Mayer RE. [Modality principle](https://www.cambridge.org/core/books/multimedia-learning/modality-principle/E5CD6E01CEA0B568CE260F66A3CD0D1F). In *Multimedia Learning*, Cambridge University Press.
5. Tjiam AM et al. [Randomised comparison of three tools for improving compliance with occlusion therapy](https://pure.eur.nl/en/publications/randomised-comparison-of-three-tools-for-improving-compliance-wit/). Erasmus University.
6. Luo et al. [Video games vs midazolam for preoperative anxiety in children: meta-analysis](https://games.jmir.org/2025/1/e67007). *JMIR Serious Games* 13, e67007, 2025.
7. Melamed BG, Siegel LJ. Reduction of anxiety in children facing hospitalization and surgery by use of filmed modeling. 1975. Cited via the team's `EducationalGameResearch.md`.

Note: the modality evidence in children is thinner than in adults. One small study (40 children) favoured narration. Others found no difference or found captions plus narration helped. We keep optional captions for this reason.
