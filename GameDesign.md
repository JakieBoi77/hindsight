# EyeCanDo Game Design

This document describes what EyeCanDo is as a *game*: what the child does,
how each stop plays, how progression, characters, outfits, coins, the shop,
and high scores work, and how all of it stays safe and educational.

It builds on [EducationalGameResearch.md](EducationalGameResearch.md) (the
research) and responds to the supervisor's interest in character swapping,
outfit swapping, leaderboards, and high scores. It is a design proposal for
the team and supervisor to discuss. No implementation is proposed, and all
procedure content still needs clinician and child-life approval.

---

## Contents

1. [Summary](#1-summary)
2. [Should an educational game have coins, shops, and leaderboards?](#2-should-an-educational-game-have-coins-shops-and-leaderboards)
3. [The game at a glance](#3-the-game-at-a-glance)
4. [Basic interactions](#4-basic-interactions)
5. [The stops and their games](#5-the-stops-and-their-games)
6. [Progression and difficulty](#6-progression-and-difficulty)
7. [Characters, outfits, coins, and the shop](#7-characters-outfits-coins-and-the-shop)
8. [High scores and leaderboards](#8-high-scores-and-leaderboards)
9. [Guardrails: keeping the game educational and safe](#9-guardrails-keeping-the-game-educational-and-safe)
10. [A play session, start to finish](#10-a-play-session-start-to-finish)
11. [Impact on the SRS and Hazard Analysis](#11-impact-on-the-srs-and-hazard-analysis)
12. [What the PoC could show](#12-what-the-poc-could-show)
13. [Open questions](#13-open-questions)
14. [Sources](#14-sources)

---

## 1. Summary

**Yes, EyeCanDo can and should have real game features**: character
swapping, outfits, coins, a shop, stars, and high scores. The research does
not say these features are bad. It says they go wrong when they are
**separate from the learning**, **reward being right instead of taking part**,
**put pressure on the child**, or **compare children with each other**.
Designed with those limits, they make the game something children choose to
come back to, and repetition is what prepares them.

**The core idea:** every stop at the eye clinic has two parts.

1. **Story** (first visit, about 2–3 minutes). A guided walkthrough of the
   real step, with honest sensations. This is the content the clinicians
   approve.
2. **Challenge** (replayable, about 1–2 minutes a round). A real game whose
   mechanic *is* the clinic behaviour. You hold still to play "Statue
   Still", look up to play "Look Up & Blink", and breathe slowly to play
   "Balloon Breath". Challenges have stars, rising difficulty, and personal
   high scores.

Between stops, the child spends coins on outfits for their character and
their Teddy in the **Dress-Up Shop**, and compares scores with their grown-up
on the **Me vs. My Grown-Up** board.

**What we take from Prodigy:** a world to explore, a character you
customise, difficulty that grows with you, and short, satisfying rounds.
**What we leave out:**

- Battles. Fighting framing around a medical procedure would add threat.
- The "answer a question, then the game happens" split. This is the
  chocolate-covered broccoli pattern.
- Reading-heavy prompts.
- Paid memberships and upselling.

---

## 2. Should an educational game have coins, shops, and leaderboards?

### 2.1 What the evidence says

| Feature | Evidence | Verdict for EyeCanDo |
| --- | --- | --- |
| **Character and avatar customisation** | Customising an avatar increases identification with it, which raised autonomy, effort, enjoyment, and time played (Birk et al., 2016). It also increased engagement and training benefit in a three-week health-game study (Birk & Mandryk, 2019). | **Yes.** It is one of the best-supported game features, and it fits STR_12 and FR_DiverseCharacters. |
| **Outfits and cosmetic items** | Cosmetic items do not change learning content, so they cannot distort it. They become a problem when tied to payment (the Prodigy complaint) or to pressure. | **Yes,** cosmetic only, earned only by playing. |
| **Coins and a shop** | Expected, tangible rewards can weaken motivation for the activity itself (overjustification). The effect is weaker for unexpected rewards and rewards for taking part. Currency arithmetic is hard for 3–4-year-olds. | **Yes, with limits.** Coins for taking part and for new stars, never for correct answers. Prices are small and visual. Coins can never be lost. |
| **Stars and difficulty levels** | Adaptive challenge is one of Prodigy's real strengths. Being challenged at the right level supports a feeling of competence. | **Yes.** Stars are personal, and difficulty rises and falls automatically. |
| **High scores (personal best)** | Measures progress against yourself. No comparison with other children. | **Yes,** especially for 5–7-year-olds. |
| **Leaderboards against other children** | In a study of 434 primary children, whether children enjoyed a leaderboard depended on their rank and their maths anxiety. Infinite leaderboards demotivated some players, and team boards did not fully fix this. Our users are younger and already anxious. EyeCanDo also has no network, so a shared leaderboard is not possible anyway (C_NoNetworkCommunications). | **No** global or between-children leaderboard. **Yes** to a cooperative board on the same device with a grown-up (Section 8). |

### 2.2 Prodigy: what to take and what to leave

Prodigy is a fantasy role-playing game in which children answer maths
questions to cast spells in turn-based battles. It has outfits, wands,
pets, and questions that get harder as the child plays.

| Prodigy does well | Why it works | How EyeCanDo uses it |
| --- | --- | --- |
| A world and a character that are *yours* | Identification and ownership. | Your character and Teddy go through the clinic; Dress-Up Shop. |
| Difficulty that adapts | Keeps every child in the "just right" zone. | Each challenge has three star tiers with automatic adjustment (Section 6). |
| Short rounds with instant feedback | Fits short attention spans; many repetitions. | Challenge rounds are 1–2 minutes long. |
| Collecting things | A reason to come back. | Stickers, outfits, stars, Teddy outfits. |

| Prodigy does poorly (for us) | Why it matters | What EyeCanDo does instead |
| --- | --- | --- |
| The question is separate from the action (answer, *then* attack) | Habgood's "chocolate-covered broccoli": less learning and motivation than intrinsic integration. | The action **is** the skill. Holding still *is* the game. |
| Battle and combat framing | For a medical setting, fighting adds threat. A child should never think of drops or a doctor as an enemy. | Helping, practising, and "showing what you can do". Nothing is attacked or dodged. |
| Reading-heavy | Most 3–7-year-olds cannot read. | Narration, pictures, and demonstrations (FR_SpokenInstructions). |
| Paid membership; best items locked behind payment | 22 advocacy groups complained to the FTC in 2021 that Prodigy markets memberships to children and shows them more ads than maths. | No real money, no ads, everything earnable (FR_NoRealMoney, NFR_StoreCompliance). |
| Effectiveness claims not independently substantiated | The FTC complaint raised this. | We measure learning and engagement ourselves (NFR_Learning, NFR_Engagement). |

---

## 3. The game at a glance

**Premise (narrated at first launch):** "You have an eye visit coming up!
Let's practise every stop at the eye clinic with Teddy. Earn stars, dress
up, and show everyone: *Eye Can Do it!*"

**Cast:**

- **You:** the child's chosen character, the patient. Pick from a diverse
  cast or build your own. Swap at any time.
- **Teddy:** a comfort toy that comes everywhere with you. Kids really do
  bring a stuffy to appointments. Teddy can be dressed up and is the
  patient in **Be the Doctor** mode. Teddy can be swapped for another
  comfort buddy.
- **Sam the clinic helper:** a friendly human guide (an orthoptist or
  nurse) who narrates and demonstrates. Not a talking animal, because
  realistic people transfer better (research §2.4).
- **Dr. Lee:** the eye doctor who appears at the later stops.

**World:** a **clinic map** with stops in the usual order of a visit. Every
stop is open from the start (FR_StartLevel). Off the map are the **Dress-Up
Shop**, the **My Visit Book** (stickers, stars, high scores, Show My Doctor
card), and **Be the Doctor**.

**The art direction is split on purpose:**

| Area | Style |
| --- | --- |
| Inside a stop (what is taught) | Realistic and calm: real equipment shapes and the real room layout. Ends with a real photo of the equipment. |
| Outside stops (map, shop, wardrobe, rewards) | Playful and bright: dress-up, sparkles, celebrations. This is where the "game" excitement lives. |

This gives the supervisor the fun game layer and keeps the clinical
content accurate and uncluttered (NFR_RelevantMedia).

### 3.1 Screen flow

```text
First launch ─► Caregiver notice ─► Pick age group ─► Pick your character & Teddy
                                                                │
                                                                ▼
        ┌──────────────────────────── CLINIC MAP ────────────────────────────┐
        │  0 My Visit   1 Waiting Room   2 Picture Match   3 Follow the Light │
        │  4 Special Drops   5 Drop Wait   6 Microscope   7 Big Light   8 Done│
        └───────┬─────────────────┬───────────────┬──────────────┬──────────┘
                ▼                 ▼               ▼              ▼
          Stop: Story  ─►  Stop: Challenge   Dress-Up Shop   My Visit Book
          (first time)     (replay any time)  & Wardrobe     (stickers, stars,
                │                 │                          high scores,
                └──► Rewards ◄────┘                          Show My Doctor card,
                     (sticker, coins, stars)                 Me vs. My Grown-Up)
                                                                 │
                                                       Be the Doctor (per stop)
```

Every screen inside a stop has a **Home** button (back to the map) and a
**Restart** button (FR_ExitLevel, FR_RestartLevel, NFR_Exit).

---

## 4. Basic interactions

The game uses only a few gestures, each taught once by Sam with an animated
hand and then reused across stops.

| Gesture | What it means in the game | Clinic behaviour it stands for | Age |
| --- | --- | --- | --- |
| **Tap** | Choose a picture, a coping tool, an outfit. | Pointing to the matching picture. | 3–7 |
| **Drag** | Move a cover over one eye; move the chin onto the chin rest; tip the head back. | Covering an eye; positioning for the slit lamp; tilting the head back for drops. | 3–7 |
| **Press and hold** | Keep a finger on a target; inflate a balloon. | Looking up and holding still; taking a slow breath. | 3–7 |
| **Hold still** | Keep the finger on a dot without lifting it while something happens. | Keeping the head still at the slit lamp. | 3–7 |
| **Follow** | Keep a finger on a moving light. | Following the light or the doctor's finger with the eyes. | 3–7 (slower for 3–4) |
| **Swipe in a direction** | Make the character look up, down, left, or right. | "Look up! Now look at my ear." | 5–7 (taps on arrows for 3–4) |
| **Quick double tap** | Blink, blink! | Blinking after the drop. | 3–7 |
| **Off-screen move** | Do it for real, with your grown-up. | All of the above, in the real world (FR_RealWorldPractice). | 3–7 |

**Rules that apply everywhere:**

- One finger at a time. No multi-touch is required.
- No device tilting is required. Tilt may be offered as an optional extra,
  because it is not accessible to every child.
- Large targets of at least 15 mm (NFR_TouchTarget).
- Lifting a finger early *pauses* the game; it never fails it.
- Every action has a narrated hint if the child is stuck for a few
  seconds (FR_HintOnMiss).
- Skip is a press-and-hold button, so repeated tapping doesn't skip
  content (FR_IgnoreRepeatedTouches). Sensation descriptions cannot be
  skipped (FR_DescribeSensations).

---

## 5. The stops and their games

Each stop has a **Story** and a **Challenge**. The SRS's three required
levels are stops 2, 4, and 6. Stops 3, 5, and 7 come from the visit we
observed (ER5) and can be added later (LC_Procedures).

Stars come from the Challenge. Tier 1 is the easiest and tier 3 the hardest
(Section 6).

### Stop 0: My Visit (story only)

A 2-minute picture story of the whole visit, from arriving to going home,
narrated by Sam. Each page uses the same picture as its stop's icon on the
map, so the map *becomes* the visit's schedule (FR_VisitSequence).

### Stop 1: Waiting Room

- **Story:** check in, find a seat, it might be busy and a bit noisy, and
  you might wait.
- **Challenge, Pack Your Bag:** drag what you'd like to bring into the bag:
  Teddy, glasses (if you wear them), a book, a snack. There is no wrong
  answer except things that don't belong, which bounce back gently
  ("Hmm, the goldfish stays home!").
- **Teaches:** what to bring, and that waiting is part of the visit.
- **Stars:** tier 1 is 3 items, tier 2 is 5 items, tier 3 is "pack for Teddy
  too".

### Stop 2: Picture Match (vision test)

- **Story:**
  - Sam shows the picture chart on the far wall and the matching card in
    your lap.
  - "One eye gets covered, so each eye gets a turn."
  - It might be dark in the room.
  - The pictures get smaller. "That's okay! Just try your best."
- **Challenge, Cover & Match:**
  1. **Cover:** drag the cover (paddle or sticky patch, matching what our
     clinic uses) over one of your character's eyes.
  2. **Match:** Sam points to a picture on the far chart. Tap the same
     picture on your matching card.
  3. Halfway through, swap the cover to the other eye.
  4. A wrong tap makes the right picture glow and wiggle, and Sam says,
     "This one! Can you tap it?" (FR_RevealAnswer).
- **Difficulty (choices, not eyesight):**
  - Tier 1: 2 choices, very different pictures.
  - Tier 2: 3 choices.
  - Tier 3: 4 choices, including look-alike pictures, plus "now swap eyes".

  The pictures on *your card* never shrink below the touch target size.
  Only the far-wall picture is drawn as getting "farther away".
- **Score:** pictures matched. Wrong taps do not reduce it.
- **Never:** a vision score, "you see well/badly", or a timer
  (NFR_NoClaims). The game uses the real picture format the clinic uses
  (for example Lea symbols or Kay pictures, to be confirmed with our
  clinic), so the child recognises it on the day.
- **Off-screen:** "Cover one eye with your hand. What can your grown-up hold
  up for you to name?"

### Stop 3: Follow the Light (pupil check and finger-following test)

- **Story:** Sam shines a small light at each eye, quickly. Then the doctor
  moves a finger or a toy and you follow it with just your eyes.
- **Challenge, Firefly Follow:** a little light floats across the room. Keep
  your finger on it as it moves. While your finger is on it, the light
  glows brighter and leaves a sparkly trail. If you lose it, it slows down
  and waits for you.
- **Difficulty:**
  - Tier 1: slow, straight lines.
  - Tier 2: curves.
  - Tier 3: curves and stops ("Freeze!").
- **Score:** percentage of time on the light, shown as a filling jar of
  sparkles.
- **Off-screen:** "Grown-up, move your finger slowly. Follow it with just your
  eyes. Keep your head still!"

### Stop 4: Special Drops (eye drops)

- **Story** (the full walkthrough described in research §8.3):
  - Peek at the bottle.
  - The sensation, which cannot be skipped: "Cold, and a little stingy, for a
    few seconds. Then it goes away."
  - Pick **My Plan** (your coping tools).
  - Watch your character cope.
  - Your turn.
  - See the real photo.
  - "Your doctor might do it a little differently. You can always tell them
    if you feel worried."
- **Challenge A, Look Up & Blink:**
  1. Drag your character's head back.
  2. Press and hold the star on the ceiling to "look up". Keep holding.
  3. A drop wobbles on the bottle tip, then falls into the eye. A short
     reminder plays: "Cold and tingly, remember? It goes away."
  4. Let go, then double-tap to blink-blink.
  5. Wipe the tear with a tissue (drag).
  6. Do the other eye.
- **Difficulty:**
  - Tier 1: one eye, short hold.
  - Tier 2: both eyes, longer hold.
  - Tier 3: Sam counts "1, 2, 3" before the drop, as many clinicians do.
- **Score:** how long you kept looking up (the "Steady Star" meter).
- **Never:** the drop is *never* something to dodge, block, or defeat, and
  looking away is never punished. Letting go pauses the drop, and Sam says,
  "Ready when you are."
- **Challenge B, Balloon Breath** (a coping-tool game):
  - Press and hold to breathe in slowly; a balloon grows.
  - Let go to breathe out; the balloon floats up and pops into confetti.
  - A gentle guide ring shows the pace (about 3 seconds in, 3 seconds out).
  - The child is invited to breathe along for real.
  - Holding too long just makes the balloon wobble ("Let it out slowly!").
    It never bursts in a startling way.
- **Difficulty:**
  - Tier 1: 3 breaths.
  - Tier 2: 5 breaths.
  - Tier 3: balloon breaths while a pretend drop goes in.
- **Score:** smooth breaths in a row.
- **Teaches:** the actual behaviours (look up, stay still, blink) and a
  coping skill the child can use in the chair. Squeeze-a-hand and
  count-to-three are other coping tools taught in the Story. All coping
  tools need child-life approval.

### Stop 5: Drop Wait

- **Story:** the drops need about 20–45 minutes to work (timing to confirm
  with our clinic). Things may look blurry up close, and lights may feel
  bright. You can rest, read, or play.
- **Challenge, I Spy (After Drops mode):** a dim, low-glare waiting-room
  scene with large, high-contrast objects. Sam says "I spy something
  round!", and you tap it. There is no score, only stickers for finding
  things. This mode can also be opened from the caregiver area during the
  real wait (FR_AfterDropsMode).

### Stop 6: Microscope (slit-lamp examination)

- **Story:**
  - Sam shows the big microscope.
  - Chin goes on the chin rest; forehead touches the band.
  - It doesn't touch your eye.
  - There's a bright light and maybe a hum.
  - Look at the doctor's ear and stay like a statue for a little while.
- **Challenge, Statue Still:**
  1. **Get in position:** drag your character's chin onto the chin rest
     until it clicks, then the forehead to the band (FR_PracticeActions).
  2. **Statue time:** press and hold the dot (the doctor's ear). While you
     hold still, the microscope's light beam sweeps slowly across the eye
     and *paints* a hidden picture behind it, such as a rainbow, a rocket,
     or Teddy waving.
  3. Lift your finger and the beam pauses. Put it back to continue.
- **Difficulty:**
  - Tier 1: 3 seconds.
  - Tier 2: 6 seconds; the doctor says "Now look at my other ear" (move the
    finger once).
  - Tier 3: 10 seconds, with the light gently getting a bit brighter
    (gradual exposure).
- **Score:** longest statue time in seconds. This is the stop with the most
  satisfying **personal best** ("New best: 9 seconds!").
- **Off-screen:** "Put your chin on your hands. Be a statue! Can your grown-up
  beat you?" This leads into the **Me vs. My Grown-Up** board (Section 8).

### Stop 7: Big Light (headlamp examination)

- **Story:** the doctor wears a light on their head and may ask you to lean
  back and look in different directions. It is bright, and that's okay.
- **Challenge, Look Where I Say:** Dr. Lee calls out directions ("Look up!",
  "Look at your toes!", "Look at the window!"). You make your character
  look: swipe for 5–7, tap arrows for 3–4.
- **Difficulty:**
  - Tier 1: one direction at a time, with pictures.
  - Tier 2: two in a row.
  - Tier 3: three in a row ("Up, then left, then down!").

  This is memory and listening practice for following instructions.
- **Score:** directions followed. A wrong direction makes Dr. Lee point and
  say, "Over here!" (FR_RevealAnswer).

### Stop 8: All Done!

- Get your **visit sticker**, matching the one children get at the real
  clinic if our clinic gives one.
- See your **Show My Doctor** card, with pictures of what you practised and
  your coping plan (FR_ShowMyDoctor).

### Be the Doctor (unlocked per stop after its Story)

Flip roles. The child is the clinic helper and **Teddy is the patient**:

- Cover Teddy's eye for Picture Match.
- Tip Teddy's head back and give pretend drops.
- Help Teddy onto the chin rest.

Teddy gets a little nervous, and the child picks a coping tool to help ("Do
a balloon breath, Teddy!"). There is no score; each stop gives a "Junior
Helper" sticker. The narration makes clear that only doctors and grown-ups
use real drops (proposed hazard in research §6.4).

This gives the child control and mastery, which are central to medical play
(research §3.2).

---

## 6. Progression and difficulty

This is the "it gets harder as you go" part the user liked in Prodigy,
without locking any learning content.

### 6.1 Three layers of progress

| Layer | What it is | How you get it | Can it be lost? |
| --- | --- | --- | --- |
| **Stickers** | One per stop's Story, one for "All Done", one per stop in Be the Doctor. | Reaching the end, even if steps were skipped. | No. |
| **Stars** | Up to 3 per Challenge (tier 1, 2, 3). | Completing a challenge round at that tier. | No. |
| **Eye Can Do Champion** | A title and a golden outfit item. | Earning a sticker at every stop and at least one star in each challenge. | No. |

### 6.2 Adaptive difficulty

- The **starting tier** follows the age group the caregiver sets: tier 1 for
  3–4, tier 2 for 5–7 (FR_AgeGroup).
- After **3 rounds in a row** that go well, the game offers the next tier:
  "Want to try a bigger challenge?" The child can say no.
- After **2 rounds in a row** of struggle, the game quietly steps down a tier
  next time. It never says "too hard" or "you failed".
- The child can always choose any tier they have reached, from the stop's
  star picker.
- Stories always play at the age group's level. Only the Challenge adapts.

### 6.3 What is never locked

- Every stop and its Story (FR_StartLevel, SR-11).
- Every Challenge tier 1.
- Every character, and the free glasses, patch, and mobility aid options.

Only **higher star tiers** (by playing) and **shop items** (by coins) need
unlocking. Neither is needed to prepare for the visit.

---

## 7. Characters, outfits, coins, and the shop

### 7.1 Characters (character swapping)

- **Starting cast:** about 8 diverse kid characters, varying in skin tone,
  hair, body type, gender expression, and cultural dress
  (FR_DiverseCharacters). Some wear glasses or an eye patch, or use a
  wheelchair.
- **Make Your Own:** choose skin, hair, eyes, and face, plus **glasses or a
  patch** if the child wears one. Making the character look like the child
  strengthens identification (Birk et al.).
- **Swap anytime** from the map or the wardrobe. Outfits, stars, coins, and
  stickers all stay (FR_ChooseCharacter).
- **Comfort buddies:** Teddy by default. Other buddies, such as a bunny or a
  blanket, are free choices. Buddy outfits are in the shop.

### 7.2 Outfits (outfit swapping)

- **Categories** (one item per category at a time, FR_Equip):
  - Hats
  - Tops
  - Shoes
  - **Glasses frames**
  - **Patch designs**
  - Capes
  - Teddy outfits
- **Eye-themed items are the stars of the shop.** Decorated glasses frames
  (stars, rainbows, sports) and patch designs (pirate, butterfly, superhero)
  make glasses and patches *cool*. Real children decorate their patches, and
  the Patchbook evidence shows how much patching benefits from positive
  framing (research §4.5).
- **Fairness rule:** *plain* glasses, a plain patch, and mobility aids are
  **always free** in Make Your Own. A child who wears glasses never has to
  earn them. Only decorative versions cost coins.
- **Dress-up is free:** trying on any item in the shop is free and unlimited.
  Coins are only needed to keep it.

### 7.3 Coins

| You get coins for | Amount (initial estimate) | Notes |
| --- | --- | --- |
| Finishing a stop's Story the **first time** | 10 | The first-completion reward (FR_FirstReward). Skipped steps still count. |
| Earning a **new star** in a Challenge | 5 per star | Once per star (FR_NoDuplicateReward in spirit), so replay is for fun and personal bests, not grinding. |
| Finishing a Be the Doctor stop the first time | 5 | |
| Becoming Eye Can Do Champion | 20 | Plus the golden outfit. |

- **Never:** coins for correct answers, coins lost for anything, coins
  for real money, coins that expire, or random coin drops (NFR_NoPressure
  forbids randomised rewards).
- **Surprise, but predictable:** the first time a child opens the shop,
  Sam gives them a free item ("Here's a present for being brave!").
  It feels unexpected to the child but is always the same, which avoids
  randomised rewards while keeping the motivational benefit of unexpected
  rewards.

### 7.4 The Dress-Up Shop

- **Prices are small and visual:** 5 to 30 coins, shown as a little pile of
  coin pictures, not only a number. An item you can afford **glows**, and Sam
  says "You have enough!" For 3–4-year-olds, buying is one tap on a glowing
  item plus a big confirm button, with no counting needed
  (FR_Purchase, FR_PurchaseRefused).
- **Everything is reachable:** all items together cost about what a child
  earns by playing every stop once and earning about two stars per
  challenge. There is no endless grind (LC_Rewards tunes this).
- **The shop is outside stops.** It is never shown inside a Story or
  Challenge, and it is never advertised during play (NFR_RelevantMedia).
- **No upselling:** no "special offer", no sad character if you don't buy,
  no locked "premium" items (NFR_NoPressure, NFR_StoreCompliance).

---

## 8. High scores and leaderboards

EyeCanDo runs offline with no accounts (C_NoNetworkCommunications,
C_PatientPrivacy). That rules out a leaderboard shared between families.
The research also advises against ranking anxious young children against
each other. Instead there are three on-device boards, from simplest to most
social:

### 8.1 My Best (personal high scores)

Each Challenge records the child's best:

- most seconds in Statue Still;
- longest Steady Star in Look Up & Blink;
- most smooth breaths in Balloon Breath;
- best sparkle jar in Firefly Follow;
- longest direction chain in Look Where I Say;
- most matches in Cover & Match.

A new best gets a short celebration ("New best!"). A lower score is never
shown as a loss. The screen just says "Nice try! Your best is still 9
seconds." This works best for 5–7-year-olds, and it is shown more simply
(a growing trophy) for 3–4-year-olds.

### 8.2 Me vs. My Grown-Up (cooperative leaderboard)

A board with two columns, the **child** and **their grown-up**, for the
physical challenges (Statue Still, Look Up & Blink, Balloon Breath).

- The grown-up plays the same challenge on the same device, or does the
  off-screen version while the child counts.
- Both scores go on the board with the child's character and a grown-up
  icon. No name is needed.
- The goal is fun co-play, which is the strongest amplifier of learning at
  this age (research §2.5). Children love beating their parent at holding
  still, and the parent learns the same behaviours.
- Wording is always celebratory for both ("You both did it! Who's the
  stillest statue?").

### 8.3 Family Team Jar (later; optional)

If more than one child in a family uses the device, each child gets their
own character and progress, and their stars fill a shared **Team Jar**
together.

- The board is cooperative: stars are added together, not ranked.
- This needs multiple local profiles, which the SRS does not have yet.
  Each profile would store an optional nickname and gameplay data only, so
  data minimisation is kept (NFR_Minimization).

### 8.4 What we do not build

- Global or online leaderboards.
- Rankings between children.
- Daily or weekly competitions, streaks, or "beat it before it's gone"
  timers (NFR_NoPressure).

---

## 9. Guardrails: keeping the game educational and safe

| Guardrail | Why | Requirement |
| --- | --- | --- |
| The **Story comes first**. A stop's Challenge is suggested only after its Story has been seen once, but it is never forced. | The approved content (steps, sensations, "clinics differ") must reach the child before the game layer. | FR_LevelContent, FR_DescribeSensations |
| **Challenge mechanics are the clinic behaviour.** | Intrinsic integration: playing *is* practising. | FR_PracticeActions |
| **Nothing to fight, dodge, or defeat.** Drops, lights, and doctors are never enemies. | Combat framing adds threat and teaches avoidance. | NFR_ChildSafe |
| **Sensation reminders** whenever a Challenge shows something felt. | A replayed challenge must stay honest. | FR_DescribeSensations |
| **Rewards for taking part and new stars, never for correct answers.** | Avoids pressure and overjustification; no child is left behind. | FR_FirstReward, FR_NoPenalty |
| **Nothing earned is ever lost.** | Loss creates stress. | FR_ExitKeepsData, thm_NoLoss |
| **Shop, wardrobe, and boards live outside stops.** | Keeps teaching content free of distraction. | NFR_RelevantMedia |
| **Timers only inside exercises**, and only as the thing being practised (statue count, breath pace). | Countdown pressure is not allowed elsewhere. | NFR_NoPressure |
| **No comparison between children.** | Anxious children and low performers are harmed by ranking. | FR_Scoreboard |
| **All clinic actions and coping tools are clinician- and child-life-approved.** | Avoid practising the wrong behaviour. | NFR_ClinicalAccuracy (extended) |
| **Free basic medical aids** in character creation. | Inclusivity; no child pays for their own glasses. | FR_DiverseCharacters |
| **Sessions stay short.** The map suggests "One or two stops today?" after about 10 minutes, but never forces a stop. | Attention spans; preparation spread over days. | NFR_LevelLength |

---

## 10. A play session, start to finish

Noah is 4, and his mum Sarah set the age group to 3–4.

1. **Day 1 (3 days before).** Sarah reads the notice. Noah builds his
   character with glasses like his, and Teddy wears a cape. They watch **My
   Visit** together. Noah plays **Special Drops**: in the Story he picks
   "hold Teddy" and "balloon breath" as his plan. He earns a sticker and
   10 coins, and Sam gives him a free star hat in the shop. He plays
   **Balloon Breath** at tier 1 and earns a star and 5 coins. Sarah does the
   off-screen "tip your head back" with him. About 10 minutes in total.
2. **Day 2.** Noah plays **Microscope**, then three rounds of **Statue
   Still** and gets a new best of 4 seconds. Sarah tries and gets 12, Noah
   insists on a rematch, and the **Me vs. My Grown-Up** board fills up.
   He buys rainbow glasses frames with his coins.
3. **Day 3.** Noah plays **Be the Doctor** and gives Teddy pretend drops,
   telling Teddy to do a balloon breath. He plays **Picture Match**, and Sam
   offers tier 2 after he does well.
4. **Visit day, waiting room.** Noah replays **Look Up & Blink**. After the
   real drops, Sarah opens **After Drops** mode, and Noah plays I Spy on a
   dim screen while they wait.
5. **In the exam room.** Sarah shows Dr. Lee the **Show My Doctor** card:
   "Noah practised the microscope. His plan is balloon breathing and holding
   Teddy." The doctor says, "Let's do a balloon breath together."

---

## 11. Impact on the SRS and Hazard Analysis

This design is consistent with the recommendations in
[EducationalGameResearch.md §7](EducationalGameResearch.md#7-recommended-requirement-changes),
with these adjustments to reflect the supervisor's interest in game features.

### 11.1 Changes to the earlier recommendations

| Earlier recommendation | Updated recommendation |
| --- | --- |
| Consider replacing coins and the shop with direct stickers. | **Keep coins and the shop**, under the rules in Section 7 (participation-based, visual prices, everything reachable, outside stops). |
| Rename the scoreboard to "My Visit Book". | Keep "My Visit Book", which now holds stickers, stars, **My Best** high scores, **Me vs. My Grown-Up**, and the Show My Doctor card. |

### 11.2 New requirements

**Story and Challenge structure:**

- Each level has a Story part and a replayable Challenge part.
- The Challenge mechanic is a clinic behaviour.
- Challenges have three star tiers, with adaptive difficulty that starts at
  the age group's tier (Section 6).

**High scores and boards:**

- A **personal best** is recorded per Challenge and shown without loss
  wording.
- A **Me vs. My Grown-Up** board on the device, with no names.
- A **Family Team Jar** with multiple local profiles (later; it needs
  data-dictionary and NFR_Minimization changes).

**Shop and outfits:**

- **Free try-on** in the shop.
- **Basic medical aids are always free**; decorated versions may cost coins.
- **Shop pricing:** all items reachable within a defined amount of play
  (a new constant).

### 11.3 Changes to existing requirements

| Requirement | Change |
| --- | --- |
| **FR_Scoreboard** | Allow comparison with the child's *own grown-up* on the same device, and keep the ban on comparison between children. |
| **FR_NoDuplicateReward** | Apply to Story completion and to each star. Replaying a Challenge gives no coins, only possible personal bests. |
| **NFR_NoPressure** | Make the predictable "surprise" gift and in-exercise counts explicitly allowed. |
| **Data dictionary** | Add stars per tier, personal bests, and grown-up scores. |

### 11.4 New hazards for the Hazard Analysis

| Proposed hazard | Mitigation |
| --- | --- |
| A Challenge teaches avoidance (for example dodging the drop). | Design review: no avoid-or-defeat mechanics. |
| A child focuses on the shop and skips Stories. | Challenges are suggested only after the Story; coins come mostly from first Story completion. |
| A grown-up's higher score upsets a child. | Celebratory wording for both; the child's score is shown first. |
| A child with low vision struggles in Picture Match and the caregiver reads it as a vision result. | Difficulty is the number of choices, not picture size; the NFR_NoClaims notice. |

---

## 12. What the PoC could show

The current PoC plan covers one procedure, eye drops. With this design, the
smallest slice that shows *actual gameplay* is:

- **Special Drops Story:** with the sensation step and the My Plan picker.
- **Look Up & Blink** and **Balloon Breath** Challenges: two tiers each,
  with stars and a personal best.
- **Character picker** with 3 characters and a **mini Dress-Up Shop** with
  3–4 items and coins.

This would show the supervisor the game layer, test the gestures with
children or child-life proxies, and show that the core mechanic (hold
still, look up, breathe) is fun. It also brings narration back into scope.
It is suggested here for discussion, not as a plan change.

---

## 13. Open questions

1. **Supervisor:** does a cooperative **Me vs. My Grown-Up** board and
   **personal bests** cover what she meant by leaderboards and high scores,
   or did she picture something else?
2. **Supervisor:** should siblings have separate profiles (Family Team Jar)
   in the first release?
3. **Clinician:** which cover does our clinic use for vision tests (a
   paddle, a sticky patch, or glasses with an occluder)? Which picture set
   (Lea, Kay, or another)?
4. **Clinician:** does our clinic count "1, 2, 3" before drops? Does it give
   stickers?
5. **Child-life:** is a gradually brighter light in Statue Still tier 3 a
   helpful exposure, or too much?
6. **Child-life:** is Balloon Breath a suitable coping tool for 3-year-olds?
   Which other tools should be games?
7. **Team:** coin amounts and prices are first guesses. Should usability
   testing tune them (LC_Rewards)?

---

## 14. Sources

New sources for this document. All other sources are listed in
[EducationalGameResearch.md §10](EducationalGameResearch.md#10-sources).

- Birk M., Atkins C., Bowey J., Mandryk R. (2016). *Fostering Intrinsic Motivation through Avatar Identification in Digital Games.* CHI. Summarised in [Frontiers in Psychology (2021)](https://frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2021.770139/full) and [arXiv: Audio Matters Too](https://arxiv.org/pdf/2202.05315)
- Birk M., Mandryk R. (2019). *Improving the Efficacy of Cognitive Training for Digital Mental Health Interventions Through Avatar Customization.* JMIR. [PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC6329434)
- *Exploring the Impact of Player Traits on the Leaderboard Experience in a Digital Maths Game.* International Journal of Serious Games. [Article](https://journal.seriousgamessociety.org/index.php/IJSG/article/view/794); [University of Galway](https://research.universityofgalway.ie/en/publications/exploring-the-impact-of-player-traits-on-the-leaderboard-experien/)
- Leaderboards in educational contexts (GaLA 2020). [PDF](https://trepo.tuni.fi/bitstream/10024/130150/1/GaLA_Conf_2020_paper_leaderboard.pdf)
- *One Size Doesn't Fit All: Age-Aware Gamification Mechanics for Multimedia Learning Environments.* [arXiv](https://arxiv.org/pdf/2512.15630)
- Prodigy FTC complaint (2021). [EdWeek](https://www.edweek.org/technology/popular-interactive-math-game-prodigy-is-target-of-complaint-to-federal-trade-commission/2021/02); [Axios](https://www.axios.com/2021/02/19/prodigy-math-game-ftc-complaint); [MediaPost](https://www.mediapost.com/publications/article/360719/ftc-urged-to-investigate-prodigy-math-game-over-al.html); [Wikipedia: Prodigy Education](https://en.wikipedia.org/wiki/Prodigy_Education)
