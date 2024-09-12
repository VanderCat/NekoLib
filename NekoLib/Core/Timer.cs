namespace NekoLib.Core; 

public class Timer {
    public static Timer Global = new();
    private HashSet<Handle> _allHandles = new();
    private float _time;

    public delegate void TimerFunction(TimerFunction function);

    public void Update(float dt) {
        _time += dt;
        var timerCopy = new HashSet<Handle>();
        foreach (var timer in _allHandles) {
            timerCopy.Add(timer);
        }
        foreach (var timer in timerCopy) {
            if (timer.Time > _time)
                continue;
            _allHandles.Remove(timer);
            timer.After.Invoke(timer.After);
        }
    }

    public Handle After(float delay, TimerFunction action) {
        var handle = new Handle {
            Time = _time + delay,
            After = action
        };
        _allHandles.Add(handle);
        return handle;
    }

    public void Remove(Handle handle) {
        _allHandles.Remove(handle);
    }

    public class Handle {
        public float Time;
        public TimerFunction After;
        internal Handle() {}
    }
}