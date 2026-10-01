using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Hindsight.Tweening
{
    public static class AwaitableExtensions
    {
        /// <summary>
        /// Runs an Awaitable without awaiting it (e.g. from a button callback). Cancellation is
        /// expected when a scene unloads mid-animation and is ignored; anything else is logged
        /// rather than silently swallowed.
        /// </summary>
        public static async void Forget(this Awaitable awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public static async Awaitable WaitForClickAsync(this Button button, CancellationToken cancellationToken)
        {
            await WaitForAnyClickAsync(new[] { button }, cancellationToken);
        }

        /// <returns>Index of the button that was clicked first.</returns>
        public static async Awaitable<int> WaitForAnyClickAsync(IReadOnlyList<Button> buttons, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var completion = new AwaitableCompletionSource<int>();
            var handlers = new UnityAction[buttons.Count];
            for (var i = 0; i < buttons.Count; i++)
            {
                var index = i;
                handlers[i] = () => completion.TrySetResult(index);
                buttons[i].onClick.AddListener(handlers[i]);
            }

            try
            {
                using (cancellationToken.Register(() => completion.TrySetCanceled()))
                {
                    return await completion.Awaitable;
                }
            }
            finally
            {
                for (var i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i] != null)
                    {
                        buttons[i].onClick.RemoveListener(handlers[i]);
                    }
                }
            }
        }
    }
}
