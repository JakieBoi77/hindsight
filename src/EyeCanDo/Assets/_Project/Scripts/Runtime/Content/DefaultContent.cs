using System.Collections.Generic;
using EyeCanDo.Core.Content;
using UnityEngine;

namespace EyeCanDo.Content
{
    /// <summary>
    /// The default PoC content. The editor builder writes these into the Data assets, and scenes
    /// fall back to them if an asset is missing. All text is a DRAFT pending clinician and
    /// child-life review (Development Plan, PoC Risk 1).
    /// </summary>
    public static class DefaultContent
    {
        private static CaptionLine D(string text, StoryBeat beat = StoryBeat.None, CaptionIcon icon = CaptionIcon.None)
        {
            return new CaptionLine(text, icon, Speaker.Doctor, beat);
        }

        private static CaptionLine Mia(string text, StoryBeat beat = StoryBeat.None, CaptionIcon icon = CaptionIcon.None)
        {
            return new CaptionLine(text, icon, Speaker.Mia, beat);
        }

        private static CaptionLine You(string text, StoryBeat beat = StoryBeat.None, CaptionIcon icon = CaptionIcon.None)
        {
            return new CaptionLine(text, icon, Speaker.You, beat);
        }

        private static StoryStep Step(StoryStepKind kind, bool skippable, params CaptionLine[] lines)
        {
            return new StoryStep { kind = kind, skippable = skippable, lines = new List<CaptionLine>(lines) };
        }

        public static void Fill(StoryScript story)
        {
            story.steps = new List<StoryStep>
            {
                Step(StoryStepKind.Hello, true,
                    D("Hi! I'm Dr. Iris. I'm an eye doctor.", StoryBeat.DoctorWave),
                    D("Today we'll learn about special eye drops!")),
                Step(StoryStepKind.MeetMia, true,
                    D("Here comes my friend Mia!", StoryBeat.MiaWalksIn),
                    Mia("Hi! I'm getting eye drops today.", StoryBeat.MiaWaves),
                    Mia("You can watch me first!")),
                Step(StoryStepKind.WhatDropsDo, true,
                    D("These are my special eye drops.", StoryBeat.ShowBigEye, CaptionIcon.Drop),
                    D("Tap the bottle to see what they do!", StoryBeat.TapBottle),
                    D("Look! The black circle got bigger.", StoryBeat.None, CaptionIcon.Eye),
                    D("It's called the pupil. It's like a window."),
                    D("A big window helps me see inside your eye!", StoryBeat.HideBigEye)),
                new StoryStep
                {
                    kind = StoryStepKind.QuizDrops,
                    lines = new List<CaptionLine>
                    {
                        D("What do the drops do? Tap a picture!", StoryBeat.Quiz),
                        D("Yes! They make the pupil bigger.", StoryBeat.DoctorCheer, CaptionIcon.Star),
                    },
                    hint = D("Good try! The drops help me see inside."),
                },
                Step(StoryStepKind.Sensation, false,
                    D("Here's how the drops feel."),
                    D("They feel cold, like a raindrop.", StoryBeat.None, CaptionIcon.Cold),
                    D("They might sting a little, for a few seconds.", StoryBeat.None, CaptionIcon.Clock),
                    D("Then it goes away."),
                    D("After, things can look blurry and bright.", StoryBeat.None, CaptionIcon.Blurry),
                    D("Sunglasses help. That goes away later too.", StoryBeat.None, CaptionIcon.Sunglasses)),
                Step(StoryStepKind.WatchMia, true,
                    D("Watch Mia get her drops.", StoryBeat.MiaSits),
                    D("Head back, and look up at the star.", StoryBeat.MiaTipsHead),
                    Mia("I'm looking up!"),
                    D("Here comes the drop.", StoryBeat.DropOnMia, CaptionIcon.Drop),
                    Mia("Ooh, cold!", StoryBeat.MiaShivers, CaptionIcon.Cold),
                    D("Blink, blink. That spreads the drop around.", StoryBeat.MiaBlinks),
                    D("A tissue wipes away the extra.", StoryBeat.MiaWipes),
                    Mia("It feels better now. I did it!", StoryBeat.None, CaptionIcon.Star),
                    D("Great job, Mia! See you soon.", StoryBeat.MiaWalksOut)),
                new StoryStep
                {
                    kind = StoryStepKind.QuizJob,
                    lines = new List<CaptionLine>
                    {
                        D("What is your job when drops go in?", StoryBeat.Quiz),
                        D("That's right! Head back, eyes open.", StoryBeat.DoctorCheer, CaptionIcon.Star),
                    },
                    hint = D("Good try! Remember Mia. Head back, eyes open."),
                },
                Step(StoryStepKind.HopUp, true,
                    D("Now it's your turn!", StoryBeat.YouWalkIn),
                    D("Tap the chair to hop up.", StoryBeat.YouHopUp),
                    D("Great sitting!", StoryBeat.None, CaptionIcon.Star)),
                Step(StoryStepKind.TipHead, true,
                    D("Tip your head back. Push your chin up!", StoryBeat.YouTipHead),
                    D("You tipped your head back!", StoryBeat.None, CaptionIcon.Star)),
                Step(StoryStepKind.LookUp, true,
                    D("Look up at the star. Press and hold it!", StoryBeat.YouLookUp)),
                Step(StoryStepKind.DropIn, true,
                    You("Ooh, cold!", StoryBeat.YouShiver, CaptionIcon.Cold),
                    D("Cold is okay. It goes away soon.", StoryBeat.None, CaptionIcon.Clock)),
                Step(StoryStepKind.Blink, true,
                    D("Blink, blink, blink! Tap your eye.", StoryBeat.YouBlink),
                    D("Super blinking!", StoryBeat.None, CaptionIcon.Star)),
                Step(StoryStepKind.Wipe, true,
                    D("Wipe the drip with the tissue.", StoryBeat.YouWipe),
                    D("All clean!", StoryBeat.None, CaptionIcon.Star)),
                Step(StoryStepKind.Close, true,
                    D("You did it! You were so brave!", StoryBeat.DoctorCheer, CaptionIcon.Star),
                    D("Your doctor might do it a little differently."),
                    D("You can always tell them if you feel worried.", StoryBeat.None, CaptionIcon.Speak),
                    D("Here's your Brave Eyes sticker!", StoryBeat.Sticker, CaptionIcon.Star)),
            };
        }

        public static void Fill(CaptionBank bank)
        {
            bank.titleHello = D("Hi! Tap Play to start!");
            bank.titleReset = D("Everything was deleted. A brand new start!");

            bank.savesHello = D("Who's playing? Tap your save file!");
            bank.saveDeleted = D("That save file was deleted.");

            bank.dressUpHello = D("Make your friend look just right!", StoryBeat.None, CaptionIcon.Star);
            bank.dressUpTryOn = D("Looking good! Tap Buy to keep it.", StoryBeat.None, CaptionIcon.Coin);
            bank.dressUpNeedCoins = D("Play the games to earn more coins!", StoryBeat.None, CaptionIcon.Coin);
            bank.dressUpBought = D("It's yours! You look great!", StoryBeat.None, CaptionIcon.Star);

            bank.mapWelcome = D("Welcome to the eye clinic! Tap the drops.", StoryBeat.None, CaptionIcon.Drop);
            bank.comingSoon = D("This part is coming soon!");
            bank.pickActivity = D("Pick the story or a game!");
            bank.storyFirst = D("Start with the story! It opens the games.");
            bank.gamesLocked = D("Finish the story to open the games!", StoryBeat.None, CaptionIcon.Lock);
            bank.gamesUnlocked = D("The games are open! Earn coins and stars.", StoryBeat.None, CaptionIcon.Coin);

            bank.storyPaused = D("Ready when you are. Hold the star again.");
            bank.storyAgain = D("Great practising! This sticker is already yours.", StoryBeat.None, CaptionIcon.Star);
            bank.storyCoins = D("You earned coins! Spend them on new clothes.", StoryBeat.None, CaptionIcon.Coin);

            bank.pickLevel = D("Pick a level. Any level is fine!");
            bank.timeUp = D("Time's up! Let's see your stars.", StoryBeat.None, CaptionIcon.Star);
            bank.allFound = D("You found them all! The blur is gone.", StoryBeat.None, CaptionIcon.Star);
            bank.newBest = D("A new best! Wow!", StoryBeat.None, CaptionIcon.Star);
            bank.greatGame = D("Great playing!", StoryBeat.None, CaptionIcon.Star);

            bank.catchIntro = D("Lean back and catch the drops in your eyes!", StoryBeat.None, CaptionIcon.Drop);
            bank.catchHowTo = D("Slide the chair to catch them.");
            bank.catchCold = D("Brr, cold! That's okay.", StoryBeat.None, CaptionIcon.Cold);
            bank.catchBlink = D("Eyes full! Tap two times to blink.", StoryBeat.None, CaptionIcon.Eye);
            bank.catchBlinkDone = D("Blink, blink! Bonus points!", StoryBeat.None, CaptionIcon.Star);
            bank.catchGolden = D("A golden drop! Three points!", StoryBeat.None, CaptionIcon.Star);

            bank.spyIntro = D("After drops, things can look blurry and bright.", StoryBeat.None, CaptionIcon.Blurry);
            bank.spyHowTo = D("Find the thing in the picture!");
            bank.spyNotThis = D("Not that one. Keep looking!");
            bank.spyClearing = D("Look! The blur is going away.", StoryBeat.None, CaptionIcon.Clock);
        }

        public static void Fill(StopCatalog catalog)
        {
            catalog.stops = new List<StopInfo>
            {
                new StopInfo { id = "my_visit", title = "My Visit", icon = "Stops/visit" },
                new StopInfo { id = "waiting_room", title = "Waiting Room", icon = "Stops/waiting" },
                new StopInfo { id = "picture_match", title = "Picture Match", icon = "Stops/match" },
                new StopInfo { id = "follow_light", title = "Follow the Light", icon = "Stops/light" },
                new StopInfo { id = StopCatalog.EyeDropsId, title = "Special Drops", icon = "Stops/drops", playable = true },
                new StopInfo { id = "drop_wait", title = "Drop Wait", icon = "Stops/wait" },
                new StopInfo { id = "microscope", title = "Microscope", icon = "Stops/microscope" },
                new StopInfo { id = "big_light", title = "Big Light", icon = "Stops/biglight" },
                new StopInfo { id = "all_done", title = "All Done!", icon = "Stops/done" },
            };
        }

        public static StoryScript CreateStory()
        {
            var story = ScriptableObject.CreateInstance<StoryScript>();
            Fill(story);
            return story;
        }

        public static CaptionBank CreateCaptionBank()
        {
            var bank = ScriptableObject.CreateInstance<CaptionBank>();
            Fill(bank);
            return bank;
        }

        public static StopCatalog CreateCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<StopCatalog>();
            Fill(catalog);
            return catalog;
        }
    }
}
