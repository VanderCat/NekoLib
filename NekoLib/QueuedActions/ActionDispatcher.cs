namespace NekoLib.QueuedActions;

public static class ActionDispatcher {
    private static Queue<IQueuedAction> _taskQueue = [];
    
    public static void ExecuteQueuedActions() {
        while (_taskQueue.TryDequeue(out var task))
            task.Execute();
    }

    public static void QueueAction(IQueuedAction action) {
        _taskQueue.Enqueue(action);
    }
}