using System.Collections;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>Checks understanding with a multiple-choice question. The player retries until correct.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Quiz", fileName = "QuizStep")]
    public sealed class QuizStep : ProcedureStep
    {
        [SerializeField] string shotId = "";
        [SerializeField, TextArea(2, 3)] string question = "";
        [SerializeField] string[] options = new string[0];
        [Tooltip("Optional picture for each option (same order as Options).")]
        [SerializeField] Sprite[] optionIcons = new Sprite[0];
        [SerializeField] int correctIndex;
        [SerializeField] string correctFeedback = "That's right!";
        [SerializeField] string wrongFeedback = "Not quite - try again!";

        public override IEnumerator Run(IProcedureContext ctx)
        {
            yield return ctx.Shots.CutAndWait(shotId);
            yield return ctx.Quiz.Ask(question, options, optionIcons, correctIndex, correctFeedback, wrongFeedback);
        }

#if UNITY_EDITOR
        public QuizStep EditorIcons(params Sprite[] icons)
        {
            optionIcons = icons;
            return this;
        }

        public QuizStep EditorInit(string shot, string q, int correct, string right, string wrong, params string[] choices)
        {
            shotId = shot;
            question = q;
            correctIndex = correct;
            correctFeedback = right;
            wrongFeedback = wrong;
            options = choices;
            return this;
        }
#endif
    }
}
