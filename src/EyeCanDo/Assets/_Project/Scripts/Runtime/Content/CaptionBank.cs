using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EyeCanDo.Content
{
    /// <summary>
    /// Captions used outside the story: title, save files, clinic map and the two games. Every
    /// <see cref="CaptionLine"/> field is checked by the content validator.
    /// </summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Caption Bank", fileName = "CaptionBank")]
    public sealed class CaptionBank : ScriptableObject
    {
        [Header("Title")]
        public CaptionLine titleHello;
        public CaptionLine titleReset;

        [Header("Save files")]
        public CaptionLine savesHello;
        public CaptionLine saveDeleted;

        [Header("Dress up and shop")]
        public CaptionLine dressUpHello;
        public CaptionLine dressUpTryOn;
        public CaptionLine dressUpNeedCoins;
        public CaptionLine dressUpBought;

        [Header("Clinic map")]
        public CaptionLine mapWelcome;
        public CaptionLine comingSoon;
        public CaptionLine pickActivity;
        public CaptionLine storyFirst;
        public CaptionLine gamesLocked;
        public CaptionLine gamesUnlocked;

        [Header("Story")]
        public CaptionLine storyPaused;
        public CaptionLine storyAgain;
        public CaptionLine storyCoins;

        [Header("Games")]
        public CaptionLine pickLevel;
        public CaptionLine timeUp;
        public CaptionLine allFound;
        public CaptionLine newBest;
        public CaptionLine greatGame;

        [Header("Drop Catch")]
        public CaptionLine catchIntro;
        public CaptionLine catchHowTo;
        public CaptionLine catchCold;
        public CaptionLine catchBlink;
        public CaptionLine catchBlinkDone;
        public CaptionLine catchGolden;

        [Header("Blurry Spy")]
        public CaptionLine spyIntro;
        public CaptionLine spyHowTo;
        public CaptionLine spyNotThis;
        public CaptionLine spyClearing;

        /// <summary>Every caption in the bank, with its field name, for the validator.</summary>
        public IEnumerable<KeyValuePair<string, CaptionLine>> All()
        {
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(CaptionLine))
                {
                    yield return new KeyValuePair<string, CaptionLine>(field.Name, (CaptionLine)field.GetValue(this));
                }
            }
        }
    }
}
