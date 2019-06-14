using System;

namespace Promises
{
    public interface ITimers
    {
        float GetTime();
        float GetTimeUnscaled();
        IPromise WaitOneFrame();
        IPromise Wait(float seconds, Action<float> progressCallback = null);
        IPromise WaitUnscaled(float seconds, Action<float> progressCallback = null);
        IPromise WaitForTrue(Func<bool> condition);
        void WaitForMainThread(Action action);
    }
}
