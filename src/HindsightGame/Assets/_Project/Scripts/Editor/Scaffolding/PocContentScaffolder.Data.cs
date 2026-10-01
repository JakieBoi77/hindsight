using Hindsight.Procedures;
using Hindsight.Procedures.Steps;
using UnityEngine;
using static Hindsight.Editor.Scaffolding.UiBuilder;

namespace Hindsight.Editor.Scaffolding
{
    /// <summary>
    /// The Eye Drops script. Wording follows the Development Plan's PoC goals: short sentences,
    /// familiar words, honest about sensations (cold, brief sting, blur) while staying reassuring.
    /// All copy is DRAFT pending paediatric ophthalmology clinician review.
    /// </summary>
    public static partial class PocContentScaffolder
    {
        private const string EyeDropsFolder = DataFolder + "EyeDrops/";
        private const string EyeDropsStepsFolder = EyeDropsFolder + "Steps/";

        private static DialogueLine Say(string text, DoctorPose pose = DoctorPose.Smile)
        {
            return new DialogueLine(text, pose);
        }

        private static ProcedureCatalog BuildProcedureData(StepPrefabs prefabs)
        {
            EnsureFolder(EyeDropsStepsFolder.TrimEnd('/'));
            var eyeDrops = BuildEyeDrops(prefabs);

            // Locked placeholders: named after real clinic experiences; content comes later.
            var catalog = CreateAsset<ProcedureCatalog>(DataFolder + "ProcedureCatalog.asset");
            SerializedWriter.For(catalog).Refs("procedures", ListOf(
                eyeDrops,
                ComingSoon("vision_test", "Vision Test", "Icons/level_vision_test.svg", "#CDE8FF"),
                ComingSoon("slit_lamp", "Slit Lamp Exam", "Icons/level_slit_lamp.svg", "#CFF3EC"),
                ComingSoon("eye_pressure", "Eye Pressure Check", "Icons/level_eye_pressure.svg", "#E1F1FF"),
                ComingSoon("eye_photos", "Eye Photos", "Icons/level_eye_photo.svg", "#EBDDFB"),
                ComingSoon("glasses_fitting", "Glasses Fitting", "Icons/level_glasses.svg", "#FFE0E0"))).Apply();
            return catalog;
        }

        private static ProcedureDefinition ComingSoon(string id, string displayName, string icon, string accent)
        {
            var procedure = CreateAsset<ProcedureDefinition>($"{DataFolder}{displayName.Replace(" ", string.Empty)}.asset");
            SerializedWriter.For(procedure)
                .String("id", id)
                .String("displayName", displayName)
                .Ref("icon", ArtLibrary.Art(icon))
                .Color("accentColor", Hex(accent))
                .Bool("isPlayable", false)
                .Apply();
            return procedure;
        }

        private static ProcedureDefinition BuildEyeDrops(StepPrefabs prefabs)
        {
            var steps = ListOf<StepDefinition>(
                Dialogue(prefabs, "01_Greeting", null,
                    Say("Hi there! I'm Dr. Iris. I'm an eye doctor.", DoctorPose.Wave),
                    Say("Today we're going to practise getting eye drops. Let's do it together!")),
                Dialogue(prefabs, "02_WhyDrops", ArtLibrary.Art("Clinic/eye_drop_bottle.svg"),
                    Say("These are special eye drops.", DoctorPose.Point),
                    Say("They help me see all the way inside your eyes, so I can check that they're healthy."),
                    Say("Your job is easy: tilt your head back and keep your eyes open.")),
                Dilation(prefabs),
                SitInChair(prefabs),
                Quiz(prefabs, "05_QuizWhatDropsDo",
                    Say("Quick question! What do the eye drops do?", DoctorPose.Point),
                    0,
                    Say("Good try! Here's a hint: the drops help me see inside your eye."),
                    Say("Yes! The drops make the black circle in your eye bigger, so I can see inside.", DoctorPose.Cheer),
                    new QuizAnswer("Make the black circle bigger", ArtLibrary.Art("Icons/quiz_big_pupil.svg")),
                    new QuizAnswer("Change your eye colour", ArtLibrary.Art("Icons/quiz_rainbow_eye.svg")),
                    new QuizAnswer("Make you fall asleep", ArtLibrary.Art("Icons/quiz_sleepy.svg"))),
                Quiz(prefabs, "06_QuizYourJob",
                    Say("What is your job when the drops go in?", DoctorPose.Point),
                    1,
                    Say("Good try! Think about keeping your eyes nice and open."),
                    Say("That's right! Head back and eyes wide open. You've got this!", DoctorPose.Cheer),
                    new QuizAnswer("Squeeze your eyes shut", ArtLibrary.Art("Icons/quiz_eyes_shut.svg")),
                    new QuizAnswer("Head back, eyes open", ArtLibrary.Art("Icons/quiz_head_back.svg")),
                    new QuizAnswer("Wiggle and look around", ArtLibrary.Art("Icons/quiz_wiggle.svg"))),
                Dialogue(prefabs, "07_HeadsUp", ArtLibrary.Art("Clinic/snowflake.svg"),
                    Say("Here's something to know: the drops might feel cold, like a tiny raindrop."),
                    Say("They might sting a little for a few seconds. That's normal, and it goes away fast."),
                    Say("Lots of kids say it's not so bad. You're going to do great!", DoctorPose.Cheer)),
                HoldStill(prefabs),
                Blink(prefabs),
                Examine(prefabs),
                Reward(prefabs));

            var procedure = CreateAsset<ProcedureDefinition>(EyeDropsFolder + "EyeDrops.asset");
            SerializedWriter.For(procedure)
                .String("id", "eye_drops")
                .String("displayName", "Eye Drops")
                .Ref("icon", ArtLibrary.Art("Icons/level_eye_drops.svg"))
                .Color("accentColor", Hex("#FFE7A8"))
                .Bool("isPlayable", true)
                .String("stickerId", "sticker_brave_eyes")
                .Ref("stickerSprite", ArtLibrary.Art("Rewards/sticker_brave_eyes.svg"))
                .Refs("steps", steps)
                .Apply();
            return procedure;
        }

        private static T Step<T>(string fileName, StepView view)
            where T : StepDefinition
        {
            var step = CreateAsset<T>(EyeDropsStepsFolder + fileName + ".asset");
            SerializedWriter.For(step).Ref("viewPrefab", view).Apply();
            return step;
        }

        private static StepDefinition Dialogue(StepPrefabs prefabs, string fileName, Sprite illustration, params DialogueLine[] lines)
        {
            var step = Step<DialogueStepDefinition>(fileName, prefabs.Dialogue);
            SerializedWriter.For(step).Ref("illustration", illustration).Lines("lines", lines).Apply();
            return step;
        }

        private static StepDefinition Dilation(StepPrefabs prefabs)
        {
            var step = Step<DilationDemoStepDefinition>("03_DilationDemo", prefabs.Dilation);
            SerializedWriter.For(step)
                .Line("tapPrompt", Say("Tap the eye drop bottle to see what the drops do!", DoctorPose.Point))
                .Lines("explanationLines",
                    Say("Look! The black circle in the middle of your eye got bigger. It's called your pupil.", DoctorPose.Cheer),
                    Say("A bigger pupil is like opening a window, so I can see inside."),
                    Say("Afterwards, things may look blurry and extra bright for a few hours. Sunglasses can help!"))
                .Float("startPupilScale", 0.4f)
                .Float("endPupilScale", 0.85f)
                .Float("dilationSeconds", 2.5f)
                .Apply();
            return step;
        }

        private static StepDefinition SitInChair(StepPrefabs prefabs)
        {
            var step = Step<TapTargetStepDefinition>("04_SitInChair", prefabs.TapTarget);
            SerializedWriter.For(step)
                .Line("prompt", Say("Can you hop up into the big chair? Tap the chair!", DoctorPose.Point))
                .Lines("successLines", Say("Great sitting! You look comfy up there.", DoctorPose.Cheer))
                .Apply();
            return step;
        }

        private static StepDefinition Quiz(StepPrefabs prefabs, string fileName, DialogueLine question, int correctIndex, DialogueLine tryAgain, DialogueLine correct, params QuizAnswer[] answers)
        {
            var step = Step<QuizStepDefinition>(fileName, prefabs.Quiz);
            SerializedWriter.For(step)
                .Line("question", question)
                .Answers("answers", answers)
                .Int("correctIndex", correctIndex)
                .Line("tryAgainLine", tryAgain)
                .Line("correctLine", correct)
                .Apply();
            return step;
        }

        private static StepDefinition HoldStill(StepPrefabs prefabs)
        {
            var step = Step<HoldStillStepDefinition>("08_EyeDropHoldStill", prefabs.HoldStill);
            SerializedWriter.For(step)
                .Lines("introLines",
                    Say("First, tilt your head back and look up at the ceiling.", DoctorPose.Point),
                    Say("Keep your eyes open and try not to blink."),
                    Say("If you blink, the drop lands on your eyelid and I have to do it again. So let's keep them open!"))
                .Line("holdPrompt", Say("Press and hold the big button to keep your eye open!", DoctorPose.Point))
                .Float("holdSeconds", 3f)
                .Line("interruptedLine", Say("Oops, a blink! The drop landed on your eyelid, so we need to try again. You can do it!"))
                .Lines("successLines",
                    Say("You did it! The drop went right in.", DoctorPose.Cheer),
                    Say("Did it feel cold? That's just the drop doing its job."))
                .String("sensationCaption", "Brrr! Cold!")
                .Apply();
            return step;
        }

        private static StepDefinition Blink(StepPrefabs prefabs)
        {
            var step = Step<TapCountStepDefinition>("09_BlinkItIn", prefabs.TapCount);
            SerializedWriter.For(step)
                .Line("prompt", Say("Now blink, blink, blink! Tap the eye to blink 5 times.", DoctorPose.Point))
                .Int("requiredTaps", 5)
                .Lines("successLines", Say("Super blinking! Blinking spreads the drops all around your eye.", DoctorPose.Cheer))
                .Apply();
            return step;
        }

        private static StepDefinition Examine(StepPrefabs prefabs)
        {
            var step = Step<ExamineStepDefinition>("10_DoctorLooksInside", prefabs.Examine);
            SerializedWriter.For(step)
                .Lines("introLines",
                    Say("The drops need some time to work, so we wait a little while."),
                    Say("Now I'll look inside your eye with my special light.", DoctorPose.Point))
                .Line("tapToolPrompt", Say("Tap my light to turn it on!", DoctorPose.Point))
                .Line("holdPrompt", Say("The light is bright, but it won't hurt. Hold the button and stay still!"))
                .Float("holdSeconds", 3f)
                .Line("interruptedLine", Say("Oops! Let's stay still a little longer. Try again!"))
                .Lines("successLines", Say("All done! I could see inside your eye. Great job staying still!", DoctorPose.Cheer))
                .Apply();
            return step;
        }

        private static StepDefinition Reward(StepPrefabs prefabs)
        {
            var step = Step<RewardStepDefinition>("11_Reward", prefabs.Reward);
            SerializedWriter.For(step)
                .String("headline", "You did it!")
                .Line("celebrationLine", Say("You were so brave! Here's your Brave Eyes sticker!", DoctorPose.Cheer))
                .String("finishLabel", "Finish")
                .Apply();
            return step;
        }
    }
}
