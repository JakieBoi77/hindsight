# Educational Game Research: Designing EyeCanDo

This document collects research on what makes educational games work for
young children, reviews existing medical and ophthalmology games, and uses
those findings to recommend what EyeCanDo should be. It ends with the changes
this research suggests for our Problem Statement, SRS, and Hazard Analysis,
and a description of the game.

It is a working document for the team and the supervisor. Nothing here is
final, and no implementation is proposed. The aim is to make sure we build
the right thing before we build it well.

**Documents reviewed:** Problem Statement and Goals (this branch), SRS and
Hazard Analysis (latest versions on `upstream/main`, 2026-10-19), and the PoC
plan in the Development Plan (this branch).

**A note on evidence strength:** Much of the research is small-sample, and
some findings below come from abstracts and summaries rather than full papers.
Each claim is labelled with how strong its evidence is, where that matters.
Anything we plan to cite in a deliverable should be checked against the
original paper first.

---

## Contents

1. [Summary](#1-summary)
2. [What makes educational games work](#2-what-makes-educational-games-work)
3. [Games and media in health care](#3-games-and-media-in-health-care)
4. [Games and media in ophthalmology](#4-games-and-media-in-ophthalmology)
5. [Design principles for teaching ages 3 to 7 about eye care](#5-design-principles-for-teaching-ages-3-to-7-about-eye-care)
6. [How our current documents compare](#6-how-our-current-documents-compare)
7. [Recommended requirement changes](#7-recommended-requirement-changes)
8. [What the game will look like](#8-what-the-game-will-look-like)
9. [Open questions for the supervisor and child-life specialists](#9-open-questions-for-the-supervisor-and-child-life-specialists)
10. [Sources](#10-sources)

---

## 1. Summary

**What works in educational games for young children:**

1. **The learning is the gameplay.** Games where the core action is the skill
   being learned teach more and hold attention far longer than games where a
   quiz is wrapped in a reward ("chocolate-covered broccoli").
2. **Mental activity, not just tapping.** The child must think, choose, or do,
   rather than only watch or tap to advance.
3. **Little distraction.** Animations and effects that are not related to the
   lesson lower what children learn.
4. **Connection to the child's real life.** Realistic settings and characters
   transfer to real situations better than fantasy ones for this age group.
5. **An adult in the loop.** Learning effects are much stronger when a parent
   plays or talks with the child about what they saw.
6. **Process praise and no punishment.** Feedback on effort and specific
   actions, with no failure states and no pressure-based design.

**What works in preparing children for medical procedures:**

- Watching a model (a peer, puppet, or cartoon) go through the procedure.
- Medical play: rehearsing the procedure with dolls or equipment.
- Honest sensory information: what the child will see, hear, and feel.
- Teaching coping strategies and giving the child small choices and control.
- Digital tools work. A 2025 network meta-analysis of 49 trials found 2D games
  and 2D videos reduce preoperative anxiety about as much as VR, and 2D games
  also reduced *parents'* anxiety.

**What exists in ophthalmology:**

- *Treatment* games for amblyopia (Luminopia, Dig Rush) and *screening*
  games for vision testing exist, but none of them prepare a child for
  the experience of an eye appointment.
- Commercial "eye doctor" pretend-play apps exist but are fantasy makeover
  games that show unrealistic procedures.
- Hospitals offer preparation videos (Moorfields, SickKids, Lurie), and a
  paediatric eye clinic in Dublin uses social stories with real photos.
- The strongest ophthalmology evidence is the **Patchbook**: an educational
  cartoon about wearing an eye patch raised patching compliance in children
  aged 3–6 from 55% to 89% in a randomized trial.
- **We found no published game designed to prepare children for an eye
  examination.** EyeCanDo would fill a real gap.

**What this means for EyeCanDo:** The SRS already covers the safety side
well: honest sensations, no punishment, all levels open, narration first,
accessibility, and privacy. The main gaps are on the *learning* side. The
requirements say a level must show part of a procedure, but they do not
require the child to **practise the physical actions** the clinic will ask
of them. They also do not require the game to **teach coping strategies**,
**show the visit as a sequence**, **bridge to the real world**, or
**involve the caregiver as a co-player**. Section 7 proposes 14 new
requirements and 11 changes. Section 8 describes a game built around the
real order of a clinic visit, in which the child's character goes through
each step, the child does the physical action of each step, and the child
leaves with a coping plan to show the doctor.

---

## 2. What makes educational games work

### 2.1 Intrinsic integration: make the learning the game

Habgood and Ainsworth tested two versions of the maths game *Zombie
Division*. In one, the maths was the combat mechanic. In the other, the same
maths was a quiz between rounds of an unrelated game. The integrated version
was more effective at teaching and was far more motivating: children
voluntarily played it about **seven times longer**. Habgood calls the
alternative "chocolate-covered broccoli": a quiz with a game tacked on
for reward. *(Moderate evidence; widely cited in the field.)*

A 2021 follow-up study had more mixed results. A bare quiz did slightly
better than a game on an immediate test, but the difference was gone on a
delayed test, and children enjoyed the game much more. The lesson is that the
game does not need to *beat* direct teaching. It needs to teach as well while
being something children choose to return to.

**For EyeCanDo:** the child's input should *be* the clinic behaviour.
Covering an eye, pointing to the matching picture, tilting the head back,
blinking, resting the chin, and holding still should all be the mechanic. A
quiz at the end of an unrelated mini-game teaches the procedure much less.

### 2.2 The Four Pillars of learning

Hirsh-Pasek, Zosh, Golinkoff and colleagues (2015) set out four conditions
under which apps support learning in young children:

| Pillar | Meaning | Common failure |
| --- | --- | --- |
| **Active** | Requires the child to think ("minds-on"), not only tap. | Tapping to advance; watching. |
| **Engaged** | Keeps attention on the content. | Pop-ups, rewards, and animations that pull attention away. |
| **Meaningful** | Connects to the child's life and what they already know. | Abstract or fantasy content with no link to the child's world. |
| **Socially interactive** | Involves others, such as a parent playing along. | Designed for solitary play only. |

A 2021 analysis of the 100 most-downloaded "educational" apps for children,
plus 24 apps that preschoolers actually used, scored them low on all four
pillars. Most apps marketed as educational do not meet these conditions.
*(Moderate evidence: a framework drawn from learning science, tested as a
coding scheme rather than an outcome trial.)*

### 2.3 Distraction undermines learning

- A meta-analysis of 43 studies of digital storybooks (2,147 children) found
  that narration, animated pictures, and music that match the story
  *help* comprehension, while hotspots, games, and dictionaries *distract*
  (Takacs et al., 2015).
- A 2026 meta-analysis found that interactive features such as hotspots and
  embedded questions had about zero effect on comprehension, likely because
  they break the flow of the story (Bus et al.).
- In an eye-tracking study of 4–5-year-olds, animation *unrelated* to the
  story lowered comprehension, while related animation did not.

**For EyeCanDo:** every animation in a procedure level should show
something about the procedure. Decorative effects, tap-for-a-surprise
hotspots, and quizzes that stop the story should be kept out of the parts
that teach. Rewards and customization belong *between* levels, not inside
them.

### 2.4 Realistic content transfers better than fantasy for this age

- Preschoolers learn and transfer less from fantastical stories than from
  realistic ones, possibly because working out fantasy uses mental resources
  that would otherwise go to the lesson (Richert et al. 2009; Walker et al.
  2015).
- Children aged 3–5 who read books with human-like animals learned fewer
  facts and reasoned less accurately than those who read realistic books
  (Ganea et al., 2014).
- Children aged under about 3 learn less from screens than from live
  demonstrations (the "video deficit" or "transfer deficit"). The gap closes
  with age, but 3–4-year-olds are still at the edge of it (Barr, 2010).

**For EyeCanDo:** the clinic, equipment, and people should be realistic
and recognisable. They can be friendly and simple in style, but the patient
should be a human child, and the slit lamp should look like a slit lamp, not
a spaceship. Because 3–4-year-olds are close to the transfer deficit, the game
should also ask the child to **do the action off-screen** with a parent
("Cover one eye with your hand, like this!").

### 2.5 The adult in the loop

- In a study of *Daniel Tiger's Neighborhood* (127 children aged 2–6),
  children's gains in empathy and emotion recognition depended on parents
  talking with them about the show. Watching alone was not enough, and the
  effect was strongest for younger children and children from low-income
  families (Rasmussen et al., 2016).
- A follow-up study found children who played the *Daniel Tiger* app used
  the show's coping strategies more often a month later than a control group.
- In the 2025 perioperative meta-analysis, 2D games lowered **parents'**
  anxiety too, and the authors suggest parent–child co-play may be part of
  why 2D games work.

**For EyeCanDo:** our personas already plan for co-play (Sarah plays with
Noah). The game should actively prompt it: "Ask your grown-up to tip their
head back too!" The caregiver guide should give the parent words to use.

### 2.6 Rewards, feedback and pressure

- **Overjustification:** In Lepper, Greene and Nisbett's (1973) classic
  study, 3–5-year-olds who were promised a reward for drawing later drew
  less on their own. The effect is strongest for *expected, tangible*
  rewards. Unexpected rewards and specific verbal praise do much less harm.
- **Manipulative design:** Radesky and colleagues found that 95% of 135 apps
  for children aged 5 and under contained advertising. Some used characters
  that looked sad when a child declined a purchase. Such design is more
  common in apps used by lower-income families.
- **Successful products:** *Khan Academy Kids* (no ads, no purchases,
  narrated, adaptive) improved pre-literacy in a small randomized trial of
  49 low-income 4–5-year-olds. *PBS Kids* games improved STEM skills in
  Ready To Learn studies. Both are character-led, free of ads, and built on
  a curriculum. Toca Boca's success with open-ended, no-fail play shows how
  appealing exploration can be for young children, though it is not
  designed to teach specific content.

**For EyeCanDo:** our "no pressure" and "no real money" requirements are
right. Rewards should be **unexpected or symbolic** (a sticker that matches
the real one at the clinic), and praise should be **specific to the action**
("You held so still!"), not general ("Great job!") or tied to being right.
The coin-and-shop economy deserves a second look (see Section 7.2).

### 2.7 What does not work: summary

| Strategy | Why it fails |
| --- | --- |
| Quiz with a game wrapped around it | Low motivation; learning is separate from play. |
| Decorative animation, hotspots, surprises | Pulls attention from the content; lowers comprehension. |
| Fantasy settings and talking animals for factual content | Weaker transfer to real life for preschoolers. |
| Solitary design with no adult role | Misses the strongest amplifier of learning at this age. |
| Expected tangible rewards for learning | Can lower intrinsic motivation. |
| Pressure loops (streaks, timers, sad characters, ads) | Manipulative; banned or restricted by app store children's policies. |
| Reading-dependent instructions | Most 3–7-year-olds cannot read independently. |
| Long sessions | Preschool attention spans are short; child-life guides suggest about 10–15 minutes at most. |

---

## 3. Games and media in health care

### 3.1 Perioperative preparation games

| Product / study | What it is | Result | What it does right | What it does wrong / unknowns |
| --- | --- | --- | --- | --- |
| **Network meta-analysis** (Luo et al., *JAMA Pediatrics*, 2025) | 49 RCTs, 4,535 children | Against control: VR SMD −1.14, 2D videos −1.08, 2D games −1.02. 2D games also reduced parents' anxiety (SMD −1.59). | Shows 2D games are nearly as good as VR, so our stretch goal (AR) is not needed to be effective. | Most studies treat games as *distraction*, not preparation. No age breakdown, and the mean age is 7.4, above our core range. |
| **CliniPup** (Netherlands, 2019 pilot RCT) | Web game played before surgery | Lower anxiety right after playing, which did not persist. No effect on pain. | Played at home days before the procedure. | Very small (20 children, aged 6–10). The short-lived effect suggests replay closer to the day is needed. |
| **Little Journey** (UK, phase III RCT under way) | Smartphone app plus cardboard VR tour of the hospital, aged 3–12 | Protocol published 2025. Company reports a 32% reduction at one site (not peer reviewed). | Starts in the weeks before surgery; shows the real hospital; covers our age range. | No published results yet. |
| **Video distraction in ophthalmic day surgery** (RCT, ages 2–8) | Video or video game before anaesthesia | Lower preoperative anxiety at all time points than parental presence alone. | Eye-specific setting; includes young children. | Distraction on the day, not preparation. |
| **Video games vs midazolam** (meta-analysis, 2025, 6 RCTs, 612 children) | Games vs sedative premedication | Games lowered anxiety at parent separation more than midazolam. | Shows games can replace some sedation. | Day-of-surgery distraction only. |

### 3.2 Modelling and medical play

- **Filmed modelling:** In Melamed and Siegel (1975), children facing surgery
  who watched a film of a peer going through hospitalization, *Ethan Has an
  Operation*, were less anxious than children who watched an unrelated film.
  Films with puppet models have also reduced anxiety (Shapiro, 1995).
  *(Classic, widely replicated.)*
- **Therapeutic play:** Systematic reviews (2017, 2021) found that play-based
  preparation tends to reduce anxiety and improve cooperation, though many
  trials are small and at high risk of bias.
- **Child-life sessions:** At St. Jude, each extra child-life preparation
  session raised the odds of a child having radiation with only partial
  sedation by 23% (observational).
- **Child-life practice guidance** is consistent across hospitals:
  - Tell the child what they will **see, hear, feel, and smell**.
  - Keep explanations short (preschoolers can attend for about 10–15
    minutes).
  - Give **safe choices** to provide control, and give the child small jobs
    to do.
  - Make clear the procedure is **not a punishment**. Preschoolers may
    believe it is.
  - Act it out with a doll. This can surface worries the child cannot name.
  - Teach distraction and coping, such as blowing bubbles, counting, or
    squeezing a hand.

### 3.3 Adherence games

- **Re-Mission** (Kato et al., *Pediatrics*, 2008): 375 adolescents and young
  adults with cancer. Players took 16% more of their prescribed antibiotic
  doses and had higher chemotherapy drug levels than a control group playing
  an entertainment game. Players also gained self-efficacy and knowledge.
  This shows a game can change real-world health behaviour, though its age
  group is much older than ours.

### 3.4 Lessons from health care

**What they do right:**

- They show the real setting and its sequence.
- They use a model character.
- They are played at home before the day.
- Many involve the parent.

**What they do wrong or leave open:**

- Effects are short-lived without replay.
- Many are distraction tools used on the day rather than preparation.
- Few are tested with 3–5-year-olds.
- Few teach a coping skill the child can use during the procedure.

---

## 4. Games and media in ophthalmology

### 4.1 Treatment games (amblyopia)

| Product | What it is | Result | Right | Wrong |
| --- | --- | --- | --- | --- |
| **Luminopia** | Dichoptic TV shows in a VR headset; FDA-cleared, ages 4–12 | One line better visual acuity than glasses alone in a phase 3 trial. | Uses content children already want (shows) as the treatment. Low effort for the child. | Not preparation; medical device. |
| **Dig Rush / falling-blocks iPad games** (PEDIG trials) | Binocular iPad games | Did not beat patching. Gains in 4–6-year-olds at 4 weeks were gone by 8 weeks. Only 22–58% of children played most of the prescribed time. | Rigorous trials with objective play logging. | Investigators blamed **poor comprehension and poor adherence**: the games did not hold young children's attention. |

**Lesson:** a game that young children do not understand or do not want to
replay fails, however sound its purpose. Engagement and comprehension must
be tested with real children aged 3–7.

### 4.2 Vision-screening games

| Product | Result | Right | Wrong |
| --- | --- | --- | --- |
| **Space Vision** (Bristol, ages 3–5) | Engagement too low to hold attention over repeated use. Being redesigned after watching preschoolers play commercial games. | Turns a real acuity test into a game. | A bare "hidden object" mechanic was not engaging enough for preschoolers. |
| **PlayWithEyes** (Padua, 2011) | Touchscreen Lea-symbol test with cartoons | Deliberately kept simple so the game does not interfere with the test. | Focused on holding attention, not on anxiety. |
| **City, University of London contrast test** (ages 4–9) | Children "pop" bouncing gratings | Even the youngest children understood it and enjoyed it. | Feasibility study only. |

**Lesson:** the matching-symbol task that real vision tests use, such as Lea
symbols or Kay pictures, can be made into play young children enjoy. We
should practise the *real* format the clinic uses, so the child recognises
it. Our SRS (FR_NoClaims) rightly forbids presenting it as an actual vision
test.

### 4.3 Commercial "eye doctor" apps

Several apps exist, for example *Eye Doctor – Kids games* (Bluebear),
*I am Eye Doctor – Eye Care and Makeup*, the eye room in *Doctor Games for
Kids*, and *Toca Doctor* (whole body).

- **Right:** They are appealing, use simple tap-and-drag play, and some have
  no ads. Toca's no-fail open play is well liked.
- **Wrong:** They are fantasy makeovers with patients such as sorceresses and
  aliens, unrealistic "surgery", and tools unlocked with gems. Some have
  in-app purchases. They are made for entertainment, not preparation, so they
  may give children *wrong* expectations.

### 4.4 Hospital preparation media

| Source | What it offers |
| --- | --- |
| **Moorfields** (London) | Child videos: check-in, play area, picture or letter vision games, a video on dilating drops. |
| **SickKids** (Toronto) | Orthoptist checks vision and places drops. Drops take 30–45 min to work. Light sensitivity afterwards. Captioned video. |
| **Lurie Children's** (Chicago) | Preparation video in English and Spanish; checklist of what to bring. |
| **Texas Children's** | Bring a favourite video for distraction; plan about 1.5 hours for a dilated exam. |
| **Royal Victoria Eye and Ear Hospital** (Dublin) | Social stories with **real photos** of the clinic and staff, in six predictable "stops". Staff reported more examinations completed (conference report). |

- **Right:** They are accurate, specific to the clinic, and show real places.
- **Wrong:** They are passive and not interactive. They do not let the child
  practise anything, and they usually target the parent more than the child.

### 4.5 The Patchbook: the strongest ophthalmology evidence

Tjiam et al. randomised 120 children aged 3–6 who were starting patching for
amblyopia, in a low-income area of The Hague. A simple educational cartoon
story that explained *why* the patch matters, from a child character's point
of view, raised compliance to about **89%**, against **55%** for the control
group. A reward calendar (67%) and a parent leaflet (73%) did not reach
significance.

**Lessons:**

1. A story told from the point of view of a child character is effective
   for 3–6-year-olds in eye care.
2. It worked better than a reward chart, so rewards are not the active
   ingredient.
3. It worked better than parent-only information, so the child must be
   the audience.

*(Moderate evidence: an RCT, but small, with compliance measured in only 88
of 120 children.)*

### 4.6 What the eye clinic itself tells us

- **Our clinical visit (ER5):** The observed order was:
  1. History
  2. Vision test (one eye covered)
  3. Pupil check with a headlamp
  4. Eye drops
  5. Slit lamp
  6. Finger-following test
  7. About a 20-minute wait for dilation
  8. Headlamp examination

  Drops burn and are a major source of anxiety. Covering one eye is the best
  way for children to test one eye. The slit lamp needs the head held very
  still.
- **Chia et al. (2015):** 26% of 298 children were significantly distressed
  before eye drops. Uncooperative children were more anxious and took longer.
- **Orthoptist interviews** (Chauhan, 2024):
  - Children under 5 are the most distressed.
  - Children often **expect the drops to be worse than they are**.
  - Orthoptists use soft wording ("tingly", "funny") and show the drop on the
    child's hand first.
  - Some apply the drop to closed lids so the child blinks it in.
  - Parents are asked to "cradle" the child.
  - Orthoptists recommended child-friendly pre-procedure videos and play
    specialists. Only one department surveyed had a video.

**A tension to resolve:** Some orthoptists avoid the word "sting". Our SRS
(FR_DescribeSensations) forbids implying it will not hurt. Child-life
practice reconciles the two: be **honest but gentle**. For example: "The
drops feel cold, and a little stingy, for a few seconds. Then it goes away.
Blinking helps." Avoid threatening words (hurt, shot, needle) and avoid
false reassurance ("you won't feel anything"). The exact vocabulary should
match what our partner clinic says, so the game and the clinician use the
same words.

### 4.7 Gap

We found no published, evaluated game that prepares children for the
*experience* of a paediatric eye examination. The existing pieces are
treatment games, screening games, fantasy apps, passive hospital videos, and
one cartoon for patching. None of them is an interactive rehearsal of the
visit.

---

## 5. Design principles for teaching ages 3 to 7 about eye care

These principles combine Sections 2–4. Each traces to a source and to the
requirement changes in Section 7.

| # | Principle | Source | Recommended changes (§7) |
| --- | --- | --- | --- |
| P1 | **Rehearse the body actions.** The child's touch input should mirror what the clinic will ask: cover one eye, point to the matching picture, tip the head back and look up, blink, rest the chin and forehead, hold still, follow a light, look at a big light. | Intrinsic integration (§2.1); ER5; medical play (§3.2) | New FR_PracticeActions |
| P2 | **Take it off the screen.** End each level with a short "try it with your grown-up" activity done in the real world. | Transfer deficit (§2.4); Four Pillars: social (§2.2) | New FR_RealWorldPractice |
| P3 | **Show the visit as a story in order.** Children cope better when they know what comes first, next, and last, including the waiting. | Social stories (§4.4); ER5 sequence | New FR_VisitSequence; change FR_JourneyMap |
| P4 | **Use a peer model who copes.** A child character goes through each step, shows a small, realistic worry, uses a coping strategy, and is fine. | Filmed modelling (§3.2); Patchbook (§4.5) | New FR_PeerModel |
| P5 | **Teach coping tools, let the child choose a plan.** Squeeze a hand, take a balloon breath, count to three, hold a comfort toy. The chosen plan goes with the child to the clinic. | Child-life practice (§3.2); Daniel Tiger strategies (§2.5) | New FR_CopingTools, FR_ShowMyDoctor |
| P6 | **Honest, gentle, full sensory information.** What the child will see, hear, feel, and smell, how long it lasts, and what helps. | Child-life practice (§3.2); orthoptists (§4.6); HA H2-6 | Change FR_DescribeSensations; new NFR_WordChoice |
| P7 | **Realistic, recognisable world.** Human characters, real equipment shapes, the real room. Friendly style, but no fantasy. Optionally end with a real photo. | Realism research (§2.4); Dublin social stories (§4.4) | New FR_RealPhotos; change NFR_ClinicalAccuracy |
| P8 | **Give the child control.** Small choices, a "be the doctor" role-play mode, skip and exit at any time. | Child-life practice (§3.2); Daniel Tiger resources (§2.5) | New FR_BeTheDoctor; existing FR_NoPenalty, FR_ExitLevel |
| P9 | **Short, focused, no clutter.** A few minutes per level. Every animation teaches something. No hotspots or decorative effects in teaching parts. Rewards only between levels. | Distraction research (§2.3); attention spans | New NFR_LevelLength, NFR_RelevantMedia |
| P10 | **Prompt the caregiver to join in** and give them the words to use. | Rasmussen (§2.5); 2D games lower parent anxiety (§3.1) | New FR_CoPlayPrompts; change FR_ProcedureGuide |
| P11 | **Specific praise, symbolic rewards, no pressure.** Praise the action ("You held so still!"). Rewards mirror the real clinic (a sticker). Never compare with others. | Overjustification (§2.6); Patchbook vs reward chart (§4.5) | New FR_ProcessPraise; change rewards (§7.2) |
| P12 | **Adapt to age, not reading ability.** Narration first for *both* age groups. Fewer steps and choices for 3–4; more "why" and sequencing for 5–7. | User characteristics; FR_SpokenInstructions | Change FR_AgeGroup; PoC plan |
| P13 | **Design for the waiting room after drops.** Dilated children are light-sensitive and blurry up close for 20–45 minutes. That is also when they are bored and anxious. | SickKids, UCSF (§4.4); SRS user characteristics | New FR_AfterDropsMode |
| P14 | **Repeat close to the day.** Effects fade (CliniPup), so the game should invite replay a few days before and on the day itself. Do this through caregiver guidance, not notifications or streaks. | §3.1; NFR_NoPressure | Change FR_ProcedureGuide |
| P15 | **Test with real children.** Games that children do not understand or replay fail (PEDIG, Space Vision). | §4.1, §4.2 | New NFR_Learning, NFR_Engagement |

---

## 6. How our current documents compare

### 6.1 What the documents already get right

The SRS and Hazard Analysis already follow much of the research:

- **Honest sensations that cannot be skipped** (FR_DescribeSensations, SR-8).
  This is what child-life practice recommends.
- **No failure states; hints and answer reveal; skip anything**
  (FR_HintOnMiss, FR_RevealAnswer, FR_NoPenalty, ULC_NoFail).
- **All levels open** (FR_StartLevel, SR-11). This is correct for both
  clinical and motivational reasons.
- **Narration plus visual demonstration, no reading** (FR_SpokenInstructions).
- **No pressure design** (NFR_NoPressure). This matches the manipulative
  design research and app store policy.
- **Clinician and child-life review** (NFR_ClinicalAccuracy, NFR_ChildSafe).
- **"Your doctor may do it a little differently"** (FR_ClinicsDiffer) and
  **"you can tell the doctor"** (FR_SpeakUp). Both give the child control.
- **Accessibility for dilated, patched, and colour-blind eyes**, diverse
  characters, and privacy by design.

### 6.2 Gaps

The current requirements are mostly about **safety** (do no harm) and
**structure** (journey, status, rewards). They say little about **how the
child learns**:

1. **FR_LevelContent** requires only that a level presents *at least one
   part* of a procedure. Nothing requires the child to *practise the
   actions* they will need at the clinic. Covering one eye, tipping the head
   back, and holding still are the very behaviours that make or break
   cooperation (Goal 2).
2. **No coping strategies** are taught. FR_SpeakUp is the only coping
   content.
3. **No visit sequence.** The journey is a set of separate procedures. The
   child never sees the whole visit in order, including check-in and the
   20–45 minute dilation wait.
4. **No real-world bridge.** Nothing asks the child to do anything off-screen.
5. **The caregiver is a supervisor, not a co-player.** The caregiver guide
   explains procedures to the adult but gives no prompts during play and no
   "what to say".
6. **No peer model.** The use cases have a virtual patient, but the
   requirements say nothing about modelling a coping response.
7. **No learning or engagement fit criteria.** Goals 1–3 have no
   requirement that would show children actually learned the steps or wanted
   to replay. NFR_Usability measures whether tasks can be completed, not
   whether children learned.
8. **The after-drops waiting period** is mentioned in the user
   characteristics but not designed for.
9. **Rewards may distract.** The coin economy, shop, and purchase rules
   (FR_Shop, FR_Purchase, FR_PurchaseRefused) are the most detailed part of
   the FRs. Currency arithmetic is hard for 3–4-year-olds, and research
   suggests rewards are not the active ingredient (Patchbook, §4.5).

### 6.3 Inconsistencies between documents

| PoC plan (Development Plan, this branch) | SRS (upstream/main) | Research suggests |
| --- | --- | --- |
| "The next level unlocking after the previous one is completed" | FR_StartLevel: any level at any time; SR-11 | Follow the SRS. Show a *suggested* order (the visit order), but lock nothing. |
| "Leaderboard" | FR_Scoreboard: personal only, no comparison | Follow the SRS. Rename to avoid confusion (for example "My Visit Book"). |
| 5–7 version "will use simple graphics and short text for the child to read" | FR_SpokenInstructions: no level requires reading | Narration first for both groups. Text can appear as optional captions only. |
| "Two versions of the game" | FR_AgeGroup: one app, caregiver-set age group | One app with age-tiered content (§7.2). |
| Quiz at the end of each level | FR_RevealAnswer, FR_ValidQuestions | Keep retrieval, but make it play: "What happens next?" picture sequencing and "show me" actions, not a text quiz. |
| PoC leaves out narration and sound | Narration is the main instruction channel | Fine for a technical PoC. Note that PoC testing then cannot show whether pre-readers learn. |

### 6.4 Hazards the analysis does not yet cover

| Proposed hazard | Component | Effect | Mitigation |
| --- | --- | --- | --- |
| **Wrong action practised.** The game rewards an action that is wrong or unhelpful at the clinic (rubbing eyes after drops, squeezing eyes shut, moving during the slit lamp, looking at the wrong target). | Gameplay | Child does the wrong thing at the clinic; examination harder. | Clinician approval of each *practised action*, not only of what is shown (extend SR-5). |
| **Coping tool clashes with the procedure** (for example "close your eyes" during the slit lamp). | Educational Content | Child uses the coping tool and blocks the examination. | Each coping tool is approved for each procedure it is offered with. |
| **Words in the game differ from the clinic's words** ("special drops" vs "magic water" vs "stingy drops"). | Educational Content | Confusion and mistrust when words don't match. | Use the partner clinic's vocabulary; review with child-life specialists. |
| **Engaging without teaching.** Rewards, customization, or decoration take most of the play time. | Progress and Rewards | Child is entertained but not prepared (the "loss of preparation" in HA §2). | NFR_RelevantMedia; NFR_Learning fit criterion; rewards between levels only. |
| **Bright screen after dilation.** | Audio and Visuals / Device | Discomfort for light-sensitive eyes in the waiting room. | FR_AfterDropsMode. |
| **Child imitates drops at home** with real medicine or objects. | Educational Content | Physical harm. | The "Be the doctor" mode uses only a toy patient and pretend drops. Narration says only doctors and grown-ups use real drops. Extends the UC_EyeDropGame postcondition. |
| **Game used as a threat or bribe** ("If you don't cooperate…") or as punishment. | External (caregiver) | Child links the clinic with punishment, a known preschool belief. | Caregiver guide states the visit is never a punishment and gives positive wording. |

---

## 7. Recommended requirement changes

IDs below are proposed labels in the SRS style. Numbering will be assigned
when they are added. Each new requirement still needs a rationale, a trace,
and (for NFRs) a fit criterion when written into the SRS. The suggested
values (constants) are initial estimates for usability testing to confirm.

### 7.1 Requirements to add

#### Stakeholder requirements

| Proposed ID | Statement | Rationale |
| --- | --- | --- |
| STR_PracticeActions | Children shall be able to practise the actions they will be asked to do at the appointment. | ER5 (cover one eye, hold still); P1 |
| STR_Coping | Children shall learn at least one way to cope with feeling worried or uncomfortable during the examination. | Child-life practice; P5 |
| STR_VisitOrder | Children shall be able to learn the order of the visit, including waiting. | ER5 sequence; social stories; P3 |
| STR_CaregiverSupport | Caregivers shall be supported in preparing their child alongside the game, including what to say and how to help at the clinic. | Rasmussen; Chauhan; P10 |

#### Functional requirements

| Proposed ID | Requirement (summary) | Principle |
| --- | --- | --- |
| **FR_PracticeActions** | Each level shall include at least one exercise in which the child's input stands for a physical action the clinic will ask of them. Examples: holding a finger on the screen to "keep still", dragging a cover over one eye, tilting the device or dragging to tip the character's head back. Each practised action shall be approved by a clinician. | P1 |
| **FR_RealWorldPractice** | Each level shall end with a narrated, pictured "Try it with your grown-up" activity that the child does off-screen. Examples: cover one eye with a hand, put the chin on the hands and be a statue for a count of five, tip the head back and look at the ceiling. The activity can be skipped. | P2 |
| **FR_VisitSequence** | The system shall provide a "My Visit" story that shows the whole visit in order: arriving, waiting, each procedure, the dilation wait, and going home. It shall use the same pictures as the levels. | P3 |
| **FR_WhatComesNext** | In the 5–7 age content, at least one exercise per level shall ask the child to order pictures of the steps. A wrong order is handled by FR_RevealAnswer. | P3, P12 |
| **FR_PeerModel** | In each level, a child character shall first show each step. The character may show a small, realistic worry and then use a coping tool, and shall finish calm. The character shall show no distress beyond the approved script (NFR_ChildSafe). | P4 |
| **FR_CopingTools** | The system shall teach a set of clinician- and child-life-approved coping tools, such as balloon breathing, counting to three, squeezing a hand, or holding a comfort toy. The child shall be able to pick their favourites as "My Plan". Levels that show a sensation shall offer the child's plan just before that sensation. | P5 |
| **FR_ShowMyDoctor** | The system shall show a "Show My Doctor" card, made only of pictures, with the levels the child practised and their chosen coping tools, so the child or caregiver can show it to the clinician. It shall store no identifying information. | P5; STR_9 |
| **FR_RealPhotos** | When a clinician-approved photo is available, each level shall end with a "This is the real one!" photo of the equipment or room. The photo shall contain no identifiable people without consent. | P7 |
| **FR_BeTheDoctor** | After a level is played once, the system shall offer a role-reversal mode in which the child performs the procedure on a toy patient. The mode shall use pretend equipment only, and the narration shall say that only doctors and grown-ups use real drops. | P8 |
| **FR_CoPlayPrompts** | Levels shall include short narrated prompts that invite the caregiver to take part, such as "Ask your grown-up to try it too!". The caregiver can turn them off in the settings. | P10 |
| **FR_ProcessPraise** | Feedback shall praise the specific action or effort ("You kept your chin so still!") rather than correctness or ability. It shall never compare the child with others. | P11 |
| **FR_AfterDropsMode** | The caregiver area shall offer an "After Drops" mode with a dim, low-glare display, larger and higher-contrast visuals, and calm, low-effort activities for the dilation wait. | P13 |
| **FR_CaregiverWordGuide** | The procedure guide shall give, for each procedure: words to use and words to avoid, how to help at the clinic (for example how to cradle the child for drops), and when to play (a few days before and on the day). | P6, P10, P14 |
| **FR_NotPunishment** | The game shall tell the child, and the guide shall tell the caregiver, that eye tests are to help their eyes and are never a punishment. | Child-life practice |

#### Non-functional requirements

| Proposed ID | Requirement (summary) | Fit criterion (suggested) |
| --- | --- | --- |
| **NFR_LevelLength** | A level shall take at most `LEVEL_MAX_MIN` minutes to play once without skips. | Median play time in testing ≤ 3 min (3–4 content), ≤ 5 min (5–7 content). |
| **NFR_RelevantMedia** | Every animation, sound effect, and interactive element in a procedure level shall show or support procedure content. Rewards and customization shall appear only between levels. | Design review with a checklist finds no decorative or unrelated element inside procedure levels. |
| **NFR_WordChoice** | Narration shall use the approved vocabulary and avoid a list of threat and false-reassurance words (for example hurt, shot, needle, cut, "won't feel anything"). | Script scan finds no listed word; child-life review finds no unresolved item. |
| **NFR_Learning** | After playing a level, children shall be able to show its actions and say or point to what happens next. | In testing, ≥ `LEARN_RATE_MIN`% of children (suggested 70%) correctly show the practised action on request, and ≥ 60% of 5–7-year-olds order the steps. Ties to Goals 1–2. |
| **NFR_Engagement** | Children shall enjoy the game and choose to keep playing. | In testing, the median child-rated enjoyment on a 5-face scale is ≥ 4, and ≥ `REPLAY_RATE_MIN`% choose another level when offered a free choice. Ties to Goal 3. |

### 7.2 Requirements to change

| Requirement | Current | Recommended change | Why |
| --- | --- | --- | --- |
| **FR_LevelContent** | Present at least one part of the procedure. | Present each step the child will experience, *in order*, with what the child sees, hears, and feels, and what the child does. | "At least one part" allows a level that teaches very little. |
| **FR_DescribeSensations** | Describe sensations; cannot be skipped; no "won't hurt". | Add how long a sensation lasts, what helps (link to FR_CopingTools), and what the child will **see, hear, and smell** as well as feel (dark room, bright light, the machine's hum, the drop bottle). Wording must be honest but gentle (NFR_WordChoice). | Child-life practice; orthoptists' soft wording; children expect drops to be worse than they are. |
| **FR_JourneyMap** | Levels in order of level number. | Lay the map out as the clinic visit, with levels in typical visit order (ER5). Keep every level open (FR_StartLevel unchanged). | Teaches the sequence as a side effect of navigation. |
| **FR_AgeGroup** | Content per age group *if a level provides it*. | Every level shall provide content for both groups, with defined differences: for 3–4, fewer steps per level, at most 2 choices, single-step gestures, more repetition; for 5–7, "why" explanations, 3–4 choices, sequencing. Narration first for both. | Developmental differences across 3–7 are large; PoC plan conflict (§6.3). |
| **FR_SpeakUp** | Traced only to the eye-drop use case. | Trace and apply to every level. Add "It's OK to feel worried. Lots of kids do." | Normalizing feelings is part of coping. |
| **FR_Scoreboard** | Personal scoreboard of points and coins. | Make it a picture "My Visit Book": the steps practised, the stickers earned, and the "Show My Doctor" card. Points can stay, but not as the main element. | Persona Noah wants to "show his mother"; pictures suit pre-readers better than totals. |
| **FR_FirstReward / FR_Shop / FR_Purchase / FR_PurchaseRefused** | Coins, a shop, and purchase rules. | *Superseded by [GameDesign.md §7](GameDesign.md#7-characters-outfits-coins-and-the-shop), which keeps coins and the shop under stated limits.* Original recommendation: **Discuss with the supervisor (ER8).** The recommendation is to replace the coin economy with a directly earned sticker and accessory for each level, matching the sticker children get at the real clinic. If the shop stays, place it outside levels and test it with 3–4-year-olds. | Currency arithmetic is hard for 3–4-year-olds. The shop adds screens that do not prepare the child. Rewards are not the active ingredient. |
| **NFR_ClinicalAccuracy** | Approve levels and guide entries. | Also approve **each practised action and each coping tool, for each procedure** it appears with. Equipment depictions shall be recognisable as the partner clinic's equipment. | Proposed hazards in §6.4. |
| **NFR_Usability** | Core tasks completed without help. | Keep, and add NFR_Learning and NFR_Engagement alongside it, since being able to complete a level does not show that the child learned or enjoyed it. | Goals 1–3 need measurable fit criteria. |
| **FR_ProcedureGuide** | Explains the procedure and how the level shows it. | Merge in FR_CaregiverWordGuide, and add "when to play" guidance (P14). | P10, P14. |
| **LC_Procedures** (likely change) | Levels may be added. | Name likely future levels from ER5: pupil light check, headlamp exam, finger-following, colour vision book, eye pressure, eye photos/OCT, glasses fitting, and **eye patching**. | Patchbook evidence (§4.5) makes patching a high-value future level. |

**Development Plan:** Update the PoC plan to match the SRS:

- Remove level unlocking.
- Change "leaderboard" to the personal "My Visit Book".
- Make narration the main channel, even for 5–7 content.
- Plan one app with age tiers rather than two games.

**Hazard Analysis:** Add the hazards in §6.4 and trace them to the new
requirements.

---

## 8. What the game will look like

> This describes the experience, not the implementation. All content,
> wording, and order must be approved by our clinician and child-life
> reviewers.

### 8.1 The concept

**EyeCanDo is a practice visit to the eye clinic.** The child picks a
character who looks like them, possibly with glasses, a patch, or a
wheelchair. That character is the patient. A friendly clinic helper (a human
nurse or orthoptist, not a talking animal) guides them through a realistic
eye clinic, one stop at a time. At every stop the child does what the
character must do, by touch on screen and then for real with their grown-up.
The child leaves with a picture card showing what they practised and their
own coping plan to show the doctor.

### 8.2 The clinic map (journey)

The journey map is a drawing of the clinic, laid out in visit order. Every
stop is open from the start, and a dotted path shows the suggested order.

| # | Stop | What the child practises | Sensations described |
| --- | --- | --- | --- |
| 0 | **My Visit** (story) | Watch the whole visit in about 2 minutes, start to finish. | None in detail; a preview only. |
| 1 | **Waiting Room** | Check in, wait, choose a comfort item to bring. | Can be busy and noisy. |
| 2 | **Picture Matching** (vision test) | Cover one eye with the paddle, then point to the matching picture far away. Then switch eyes. | It's dark; pictures get smaller; one eye is covered. |
| 3 | **Little Light** (pupil check) | Look at a spot on the wall while the helper shines a small light. | A bright light for a moment. |
| 4 | **Special Drops** | Pick a coping tool, tip the head back, look up, the drop goes in, blink, wipe the tear with a tissue. | Cold, then a little stingy for a few seconds, then it goes away. Things may look blurry and bright afterwards. |
| 5 | **Drop Wait** | Wait for the drops to work, with calm activities. | Blurry up close; lights feel bright. |
| 6 | **Microscope** (slit lamp) | Chin on the chin rest, forehead on the band, look at the doctor's ear, statue still while the light looks. | A bright light; a hum. It doesn't touch your eye. |
| 7 | **Big Light** (headlamp exam) | Lie or sit back, look up, down, and sideways when asked. | The doctor wears a light on their head. It's bright. |
| 8 | **All Done!** | Get a sticker. Look at the "Show My Doctor" card. | None. |

Stops 3, 6, and 7 depend on what the partner clinic does and in what order.
The table follows the sequence observed in ER5. The three SRS levels (vision,
drops, slit lamp) are stops 2, 4, and 6. The others can be added later under
LC_Procedures.

### 8.3 Inside one stop: "Special Drops"

Content for the 3–4 age group:

1. **Peek.** The helper holds up the little bottle and narrates: "These are
   special eye drops. They help the doctor see inside your eye." A drop is
   shown landing on the character's hand: "It's just like water."
   *(FR_LevelContent, FR_PeerModel)*
2. **What it feels like** (cannot be skipped). "The drops feel cold. They
   might sting a little, for a few seconds. Then it stops." *(FR_DescribeSensations)*
3. **My Plan.** "What helps you feel brave?" The child picks from three
   pictured tools, such as a balloon breath, squeezing a hand, or holding
   Teddy. *(FR_CopingTools)*
4. **Model.** The character looks a little worried ("I feel a bit
   nervous"), does the chosen tool, and the drop goes in. The character
   blinks and says "That was cold! It's okay now." *(FR_PeerModel)*
5. **Your turn.** The child drags the character's head back and holds a
   finger on the ceiling star to "look up". The drop falls. The child taps
   to blink, then wipes the tear with a tissue. The narration praises
   the action: "You looked up so well!" A hint appears if needed; any step
   can be skipped. *(FR_PracticeActions, FR_ProcessPraise, FR_HintOnMiss, FR_NoPenalty)*
6. **Real one.** A photo of a real drop bottle at the clinic: "This is
   what it really looks like!" *(FR_RealPhotos)*
7. **Try it for real.** "Tip your head back and look at the ceiling. Can
   your grown-up do it too?" *(FR_RealWorldPractice, FR_CoPlayPrompts)*
8. **Close.** "Your doctor might do it a little differently. You can always
   tell them if you feel worried." Then a sticker appears.
   *(FR_ClinicsDiffer, FR_SpeakUp, FR_FirstReward)*

In the 5–7 content, step 1 adds *why* ("They make the black circle in your
eye bigger, like opening a window"), step 5 adds keeping the eyes open while
the drop goes in, and a "What comes next?" picture-ordering exercise
replaces step 7 for those who want it *(FR_WhatComesNext)*.

Target play time is about 3 minutes for 3–4-year-olds and about 5 minutes
for 5–7-year-olds *(NFR_LevelLength)*.

### 8.4 Other modes

- **Be the Doctor** *(FR_BeTheDoctor)*: after playing a stop, the child can
  give pretend drops to a teddy bear, shine the little light, or ask the
  teddy to hold still. Teddy gets a little nervous, and the child can choose
  a coping tool for Teddy. This turns the child from the one things happen
  to into the one who knows what to do.
- **My Visit Book** *(FR_Scoreboard, FR_ShowMyDoctor)*: a picture book of
  stops visited, stickers, and the child's character. The last page is the
  "Show My Doctor" card: pictures of what the child practised, plus "My
  Plan" (for example 🎈 balloon breath, 🧸 hold Teddy).
- **After Drops** mode *(FR_AfterDropsMode)*: a dim, calm, large-target mode
  for the dilation wait, for example dressing the character or sorting big
  shapes, with no bright flashes.
- **Caregiver area** *(behind the parental gate)*: the medical notice, the
  procedure guide with "Say this / not this" (for example "cold and a little
  stingy" rather than "it won't hurt", and "cradle" rather than "hold down"),
  tips for the clinic (bring a comfort item, plan about 1.5 hours for a
  dilated exam), when to play (a couple of days before, and again on the
  day), age group, volume, and co-play prompts on or off.

### 8.5 Look and sound

- **Visual style:** friendly, rounded, and simple, but realistic in content.
  Human characters, recognisable equipment, the real room layout. The
  partner clinic's real photos appear at the end of stops.
- **Colour and contrast:** high contrast, with no meaning shown by colour
  alone. Large targets of at least 15 mm (NFR_TouchTarget). The game must
  work with one eye patched or with blurry vision.
- **Sound:** one warm narrator voice, sentences of at most 10 words, soft
  music lower than the voice, and no sudden sounds (NFR_AudioSafety). Every
  instruction is also shown as a picture or demonstration (FR_NarrationFallback).
- **What it does *not* have:** ads, real-money purchases, timers, streaks,
  lives, notifications, leaderboards, scary music, needles, crying patients,
  looming machines, or fantasy "surgery".

### 8.6 How a family uses it

1. **At the referral or a few days before:** the parent installs the app,
   reads the notice, sets the age group, and plays *My Visit* together with
   the child.
2. **On the days before:** the child plays a stop or two at a time, about
   10 minutes per session, and does the "try it for real" moves with the
   parent.
3. **On the morning of the visit:** the child replays *Special Drops* and
   picks "My Plan".
4. **In the waiting room:** the child plays a favourite stop. After the
   drops, the parent switches to *After Drops* mode.
5. **In the exam room:** the child or parent shows the clinician the "Show
   My Doctor" card. The clinician uses the same words the game used.

---

## 9. Open questions for the supervisor and child-life specialists

1. **Vocabulary.** What words does our partner clinic use for the drops,
   the slit lamp, and the headlamp? Should we say "sting" (honest), use
   softer words like "tingly", or both?
2. **Visit order.** Is the ER5 order typical for children aged 3–7? Which
   stops apply to most first visits?
3. **Rewards.** The supervisor asked for points, a scoreboard, and a
   character (ER8). Is a coin shop part of that request, or would directly
   earned stickers and accessories meet it?
4. **Coping tools.** Which tools are safe and helpful for each procedure?
   For example, is "close your eyes and count" acceptable before drops,
   given the closed-lid technique? It is clearly wrong during the slit lamp.
5. **Photos.** Can we photograph the clinic's equipment and rooms, and
   under what consent?
6. **"Show My Doctor" card.** Would clinicians actually look at it? What
   should it show?
7. **Timing.** What lead time do child-life specialists recommend for
   preparing 3–4-year-olds compared with 5–7-year-olds?
8. **Testing.** Can usability and learning testing (NFR_Learning,
   NFR_Engagement) be run with children before ethics approval, through
   child-life proxies, or only after approval?
9. **Patching and glasses.** Should a future section cover living with
   glasses or a patch, given the Patchbook evidence?

---

## 10. Sources

### Educational games and learning science

- Hirsh-Pasek K. et al. (2015). *Putting Education in "Educational" Apps.* Psychological Science in the Public Interest. Summary: [APS press release](https://www.psychologicalscience.org/news/releases/science-of-learning-can-help-parents-developers-grade-educational-apps.html); [PubMed](https://pubmed.ncbi.nlm.nih.gov/35282402/)
- Meyer M. et al. (2021). *How educational are "educational" apps for young children?* Journal of Children and Media. [PDF](https://kathyhirshpasek.com/wp-content/uploads/sites/9/2021/03/How-educational-are-educational-apps-for-young-children-App-store-content-analysis-using-the-Four-Pillars-of-Learning-framework.pdf)
- Habgood M.P.J. (2007). *The Effective Integration of Digital Games and Learning Content.* PhD thesis, University of Nottingham. [eprints](https://eprints.nottingham.ac.uk/10385); [KQED MindShift summary](https://www.kqed.org/mindshift/20765/whats-the-secret-sauce-to-a-great-educational-game)
- Game vs bare quiz follow-up (2021). Frontiers in Psychology. [Article](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2021.678380/pdf)
- Takacs Z., Swart E., Bus A. (2015). *Benefits and pitfalls of multimedia and interactive features in technology-enhanced storybooks: a meta-analysis.* Review of Educational Research. [PDF](https://journals.sagepub.com/doi/pdf/10.3102/0034654314566989)
- *Which Interactive Features in Children's Digital Picture Books Promote Reading Comprehension? A Meta-Analysis.* [Open University](https://oro.open.ac.uk/107389)
- Eye-tracking study of animation relevance in preschool e-books. [JEMR](https://bop.unibe.ch/JEMR/article/download/11237/14708/54338)
- Ganea P. et al. (2014). *Do cavies talk? The effect of anthropomorphic picture books on children's knowledge about animals.* Frontiers in Psychology. [Article](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2014.00283/full); [U of T news](https://www.utoronto.ca/news/when-kids-books-feature-animals-human-traits)
- Realistic vs fantastical stories and transfer (Richert; Walker). [NSF PAR](https://par.nsf.gov/servlets/purl/10309037)
- Barr R. (2010). *Transfer of learning between 2D and 3D sources during infancy.* Developmental Review. [PubMed](https://pubmed.ncbi.nlm.nih.gov/20563302)
- Rasmussen E. et al. (2016). *Relation between active mediation, exposure to Daniel Tiger's Neighborhood, and US preschoolers' social and emotional development.* [TTU](https://scholars.ttu.edu/en/publications/relation-between-active-mediation-exposure-to-daniel-tigers-neigh-12); [news](https://www.depts.ttu.edu/comc/research/news/empathy-daniel-tiger.php)
- Daniel Tiger doctor strategy resource. [PBS / TEKS guide](https://teksguide.org/resource/when-we-do-something-new-doctor-strategy-song-and-activity-daniel-tigers-neighborhood)
- Lepper, Greene & Nisbett (1973), overjustification effect. [SAGE encyclopedia entry](https://sk.sagepub.com/ency/edvol/embed/socialpsychology/chpt/overjustification-effect)
- Radesky J. et al., advertising and manipulative design in children's apps. [Michigan Medicine](https://www.michiganmedicine.org/health-lab/design-tricks-commonly-used-monetize-young-childrens-app-use); [PopSci](https://www.popsci.com/ads-kids-apps-games/)
- Khan Academy Kids pre-literacy RCT (company summary). [Khan Academy blog](https://blog.khanacademy.org/khan-academy-kids-improves-pre-literacy-skills-in-preschoolers-research-confirms/)
- PBS KIDS STEM learning research. [SRI](https://sri.com/?p=96179)

### Health care games and preparation

- Luo et al. (2025). *Digital Health Interventions in Pediatric Perioperative Care: A Network Meta-Analysis.* JAMA Pediatrics. [PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC12439185/); [PubMed](https://pubmed.ncbi.nlm.nih.gov/40952714/)
- Video games vs midazolam meta-analysis (2025). JMIR Serious Games. [Article](https://games.jmir.org/2025/1/e67007)
- *A Web-Based Serious Game for Health to Reduce Perioperative Anxiety and Pain in Children (CliniPup): Pilot Randomized Controlled Trial* (2019). JMIR Serious Games. [PMC](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6592396/)
- Little Journey phase III RCT protocol (2025). [PMC](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC11865764/); [ISRCTN](https://www.isrctn.com/ISRCTN23410641)
- Video distraction in paediatric ophthalmic day surgery RCT. [PubMed](https://pubmed.ncbi.nlm.nih.gov/38666175/)
- Melamed B. & Siegel L. (1975). *Reduction of anxiety in children facing hospitalization and surgery by use of filmed modeling.* J Consult Clin Psychol 43:511–521. Cited in [PMC review table](https://pmc.ncbi.nlm.nih.gov/articles/PMC4884908/table/jsv094-T2)
- Puppet modelling for medical stressors. [Penn State](https://pure.psu.edu/en/publications/puppet-modeling-technique-for-children-undergoing-stressful-medic/)
- *Therapeutic play to prepare children for invasive procedures: a systematic review* (2017). [Jornal de Pediatria](https://jped.elsevier.es/en-therapeutic-play-prepare-children-for-articulo-S0021755716300961)
- *Play and medical play in teaching pre-school children to cope with needle procedures: a systematic review* (2021). [ResearchGate](https://researchgate.net/profile/Ioannis-Kyriakidis-3/publication/350009984_Play_and_medical_play_in_teaching_pre-school_children_to_cope_with_medical_procedures_involving_needles_A_systematic_review/links/63484c87ff870c55ce216bc1/Play-and-medical-play-in-teaching-pre-school-children-to-cope-with-medical-procedures-involving-needles-A-systematic-review.pdf)
- Child-life play preparation and radiation sedation (St. Jude). [PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC5053392)
- Preschooler test or procedure preparation. [UF Health / MedlinePlus](https://ufhealth.org/conditions-and-treatments/preschooler-test-or-procedure-preparation); [Nationwide Children's](https://www.nationwidechildrens.org/-/media/nch/your-visit/documents/supporting-your-child-during-medical-procedures.pdf); [Children's Mercy](https://libguides.childrensmercy.org/childlife/preparation)
- Kato P. et al. (2008). *A video game improves behavioral outcomes in adolescents and young adults with cancer.* Pediatrics. [ClinicalTrials.gov](https://clinicaltrials.gov/study/NCT00425139)

### Ophthalmology

- Luminopia phase 3 and label expansion. [Ophthalmology Web](https://www.ophthalmologyweb.com/1315-News/579583-Novel-Digital-Therapeutic-for-Amblyopia-Demonstrates-Safety-and-Efficacy-in-Phase-3-Trial); [BioSpace](https://www.biospace.com/press-releases/luminopia-announces-fda-clearance-for-patients-with-amblyopia-aged-8-to-12-years)
- PEDIG binocular game trials (Dig Rush, falling blocks). [Harvard](https://eye.hms.harvard.edu/publications/randomized-trial-binocular-dig-rush-game-treatment-amblyopia-children-aged-7-12); [PMC commentary](https://pmc.ncbi.nlm.nih.gov/articles/PMC8919092); [Ophthalmology Times](https://ophthalmologytimes.com/pedig-focusing-binocular-activities-amblyopia)
- Gray S. et al. (2018). *Designing Games for Vision Screening: Lessons Learned from Observing Preschool Video Game Play.* CHI PLAY. [Bristol](https://research-information.bris.ac.uk/en/publications/designing-games-for-vision-screening-lessons-learned-from-observi/)
- Gamified contrast sensitivity test for amblyopia (2020). Frontiers in Medicine. [Article](https://www.frontiersin.org/journals/medicine/articles/10.3389/fmed.2020.00469/full)
- *PlayWithEyes: a new way to test children eyes* (2011). [PDF](https://www.math.unipd.it/~gaggi/doc/segah11.pdf)
- Tjiam A. et al. (2013). *Randomised comparison of three tools for improving compliance with occlusion therapy.* [Erasmus](https://pure.eur.nl/en/publications/randomised-comparison-of-three-tools-for-improving-compliance-wit/); [AAO summary](https://www.aao.org/editors-choice/educational-cartoon-improves-occlusion-therapy-res)
- Chauhan H. (2024). *An Investigation Into the Orthoptist Experience of Instilling Eye Drops in Children Attending the Eye Clinic.* British and Irish Orthoptic Journal 20:171–182. [PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC11328678/)
- Cyclopentolate on closed vs open eyelids RCT. [PubMed](https://pubmed.ncbi.nlm.nih.gov/27799585/)
- Chia et al. (2015), cited in our Problem Statement (`refs/References.bib`).
- Moorfields paediatric videos. [Moorfields](https://moorfields.nhs.uk/children-and-young-people/paediatric-videos)
- SickKids Eye Clinic. [SickKids](https://www.sickkids.ca/en/care-services/clinics/eye-clinic/)
- Lurie Children's: how to prepare for an ophthalmology visit. [Lurie](https://www.luriechildrens.org/en/specialties-conditions/pediatric-ophthalmologist/how-to-prepare-for-an-ophthalmology-visit-and-what-to-expect/)
- Texas Children's: your child's next ophthalmology visit. [Texas Children's](https://www.texaschildrens.org/content/wellness/your-childs-next-ophthalmology-visit-what-expect-how-prepare)
- UCSF: slit lamp exam. [UCSF Benioff](https://www.ucsfbenioffchildrens.org/medical-tests/slit-lamp-exam)
- Social stories for autistic patients at the Royal Victoria Eye and Ear Hospital. [Medical Independent](https://www.medicalindependent.ie/in-the-news/conference/new-project-helps-autistic-patients-prepare-for-eye-appointments/); [EyeWiki: Autism](https://eyewiki.aao.org/Autism_Spectrum_Disorder)
- Anxiety and compliance in the ophthalmology clinic (registered study). [ClinicalTrials.gov NCT02704442](https://clinicaltrials.gov/study/NCT02704442)
- Commercial eye-doctor apps. [Eye Doctor – Kids games](https://apps.apple.com/us/app/eye-doctor-kids-games/id725520278); [I am Eye Doctor](https://apps.appfollow.io/ios/i-am-eye-doctor-eye-care-and-makeup/1044147911?country=us); [Toca Doctor](https://apps.appfollow.io/ios/toca-doctor/424209938?country=cy)
