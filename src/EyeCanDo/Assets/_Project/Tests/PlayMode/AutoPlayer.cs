using System;
using System.Collections;
using System.IO;
using System.Linq;
using EyeCanDo.App;
using EyeCanDo.Content;
using EyeCanDo.Games;
using EyeCanDo.Story;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EyeCanDo.Tests.PlayMode
{
    /// <summary>
    /// Plays the game like a child would, through the same handlers touches use. Optional hooks
    /// let the storyboard take a picture at each moment.
    /// </summary>
    internal static class AutoPlayer
    {
        public const float Timeout = 240f;

        /// <summary>No saves and default settings, as on a new install. No save is picked yet.</summary>
        public static void FreshData()
        {
            GameServices.UseTestData(Path.Combine(Application.temporaryCachePath, "playmode_data"));
        }

        /// <summary>Fresh data with a new player in Save 1, picked and ready to play.</summary>
        public static void FreshSave()
        {
            FreshData();
            GameServices.ChooseSave(0);
        }

        /// <summary>A save where the story is done, so the games are open.</summary>
        public static void StoryDone()
        {
            FreshSave();
            GameServices.Progress.FinishStory();
            GameServices.Save();
        }

        public static IEnumerator Load(string scene)
        {
            SceneManager.LoadScene(scene);
            yield return null;
            yield return null;
            GameServices.NavigationGate.Open();
        }

        public static IEnumerator WaitFor(Func<bool> condition, float timeout = Timeout, string what = "condition")
        {
            var start = Time.realtimeSinceStartup;
            while (!condition())
            {
                if (Time.realtimeSinceStartup - start > timeout)
                {
                    Assert.Fail($"Timed out waiting for {what}.");
                }

                yield return null;
            }
        }

        public static IEnumerator WaitForScene(string name)
        {
            yield return WaitFor(() => SceneManager.GetActiveScene().name == name && !GameServices.Root.IsLoading, 20f, "scene " + name);
            yield return null;
        }

        public static T Find<T>()
            where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindAnyObjectByType<T>();
        }

        /// <summary>Plays the whole story. Answers each quiz wrong once first, and lets go of the star once.</summary>
        public static IEnumerator PlayStory(EyeDropsStory story, Func<string, IEnumerator> capture = null, Func<bool> stopWhen = null)
        {
            var captions = story.Stage.Captions;
            var lastText = "";
            var wrongGiven = false;
            var pausedOnce = false;
            var start = Time.realtimeSinceStartup;
            while (!story.Finished && (stopWhen == null || !stopWhen()))
            {
                Assert.Less(Time.realtimeSinceStartup - start, Timeout, "Story took too long.");
                if (captions.IsWaitingForTap)
                {
                    if (capture != null && captions.CurrentText != lastText)
                    {
                        lastText = captions.CurrentText;
                        yield return capture($"story_{story.StepIndex:00}_{story.CurrentKind}");
                    }

                    captions.SimulateTap();
                    yield return null;
                    continue;
                }

                switch (story.CurrentAction)
                {
                    case StoryBeat.TapBottle:
                        yield return Settle(0.8f);
                        if (capture != null)
                        {
                            yield return capture("story_tap_bottle");
                        }

                        story.TapBottle();
                        yield return WaitFor(() => story.CurrentAction != StoryBeat.TapBottle, 20f, "bottle");
                        break;
                    case StoryBeat.Quiz:
                        yield return WaitFor(() => story.QuizCards.Count == 3, 10f, "quiz cards");
                        yield return Settle(0.6f);
                        if (capture != null)
                        {
                            yield return capture($"story_quiz_{story.CurrentKind}");
                        }

                        if (!wrongGiven)
                        {
                            wrongGiven = true;
                            story.AnswerQuiz(0);
                            yield return Settle(0.6f);
                            if (capture != null)
                            {
                                yield return capture("story_quiz_try_again");
                            }
                        }

                        story.AnswerQuiz(story.CurrentKind == Core.Content.StoryStepKind.QuizDrops ? 1 : 2);
                        yield return WaitFor(() => story.CurrentAction != StoryBeat.Quiz, 20f, "quiz");
                        break;
                    case StoryBeat.YouHopUp:
                        yield return Settle(0.8f);
                        if (capture != null)
                        {
                            yield return capture("story_your_turn_chair");
                        }

                        story.TapChair();
                        yield return WaitFor(() => story.CurrentAction != StoryBeat.YouHopUp, 20f, "hop up");
                        break;
                    case StoryBeat.YouTipHead:
                        yield return Settle(0.6f);
                        if (capture != null)
                        {
                            yield return capture("story_tip_head");
                        }

                        while (story.CurrentAction == StoryBeat.YouTipHead)
                        {
                            story.DragHead(25f);
                            yield return null;
                        }

                        break;
                    case StoryBeat.YouLookUp:
                        yield return Settle(0.6f);
                        story.PressStar();
                        if (!pausedOnce)
                        {
                            pausedOnce = true;
                            yield return Settle(1f);
                            story.ReleaseStar();
                            yield return Settle(0.5f);
                            if (capture != null)
                            {
                                yield return capture("story_look_up_paused");
                            }

                            story.PressStar();
                            yield return Settle(0.5f);
                            if (capture != null)
                            {
                                yield return capture("story_look_up_holding");
                            }
                        }

                        yield return WaitFor(() => story.CurrentAction != StoryBeat.YouLookUp, 20f, "look up");
                        story.ReleaseStar();
                        break;
                    case StoryBeat.YouBlink:
                        yield return Settle(0.6f);
                        if (capture != null)
                        {
                            yield return capture("story_blink");
                        }

                        while (story.CurrentAction == StoryBeat.YouBlink)
                        {
                            story.TapEye();
                            yield return Settle(0.4f);
                        }

                        break;
                    case StoryBeat.YouWipe:
                        yield return Settle(1.2f);
                        if (capture != null)
                        {
                            yield return capture("story_wipe");
                        }

                        story.WipeNow();
                        yield return WaitFor(() => story.CurrentAction != StoryBeat.YouWipe, 10f, "wipe");
                        break;
                    default:
                        yield return null;
                        break;
                }
            }
        }

        /// <summary>Plays a Drop Catch round, steering under the lowest drop and blinking when the eyes are full.</summary>
        public static IEnumerator PlayDropCatch(DropCatchGame game, Func<string, IEnumerator> capture = null)
        {
            yield return WaitFor(() => game.Round != null, 10f, "round");
            var shots = new[] { 4f, 12f, 22f };
            var shot = 0;
            var counted = false;
            var captions = Find<UI.Stage>().Captions;
            while (game.IsPlaying)
            {
                if (capture != null && !counted && game.CountingDown == 2)
                {
                    counted = true;
                    yield return capture("catch_countdown");
                }

                if (captions.IsWaitingForTap)
                {
                    if (capture != null && shot == 0)
                    {
                        yield return capture("catch_intro");
                    }

                    captions.SimulateTap();
                }

                var target = game.LowestDropX;
                if (target.HasValue)
                {
                    game.SteerTo(target.Value);
                }

                if (game.Round.EyesFull)
                {
                    game.TapToBlink();
                }

                if (capture != null && shot < shots.Length && game.Round.Elapsed >= shots[shot])
                {
                    yield return capture($"catch_play_{shot}");
                    shot++;
                }

                yield return null;
            }
        }

        /// <summary>Plays a Blurry Spy round: one wrong tap (or none, for a perfect round), then every target.</summary>
        public static IEnumerator PlayBlurrySpy(BlurrySpyGame game, Func<string, IEnumerator> capture = null, bool perfect = false)
        {
            yield return WaitFor(() => game.Round != null, 10f, "round");
            var captions = Find<UI.Stage>().Captions;
            yield return WaitFor(() => captions.IsWaitingForTap, 10f, "intro");
            if (capture != null)
            {
                yield return capture("spy_intro");
            }

            captions.SimulateTap();
            yield return WaitFor(() => game.Live, 10f, "countdown");
            yield return Settle(0.8f);
            if (capture != null)
            {
                yield return capture("spy_find");
            }

            if (!perfect)
            {
                var wrong = game.Items.First(i => i != game.CurrentTargetItem);
                game.Tap(wrong);
                Assert.AreEqual(1, game.Round.WrongTaps);
                Assert.AreEqual(0, game.Round.Found, "A wrong tap costs nothing.");
            }

            game.Tap(game.CurrentTargetItem);
            yield return Settle(0.3f);
            if (capture != null)
            {
                yield return capture("spy_found_one");
            }

            while (game.IsPlaying)
            {
                if (captions.IsWaitingForTap)
                {
                    captions.SimulateTap();
                }

                var target = game.CurrentTargetItem;
                if (target != null && !target.Found)
                {
                    game.Tap(target);
                    yield return Settle(perfect ? 0.2f : 0.7f);
                    if (capture != null && game.Round.Found == game.Round.Settings.Targets / 2)
                    {
                        yield return capture("spy_halfway");
                    }

                    continue;
                }

                yield return null;
            }
        }

        public static IEnumerator Settle(float seconds)
        {
            var end = Time.time + seconds;
            while (Time.time < end)
            {
                yield return null;
            }
        }
    }
}
