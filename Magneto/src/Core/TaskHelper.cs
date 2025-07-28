using Core.Define;

namespace Core;

public static class TaskHelper
{
    public static TaskLevel GetTaskLevel(int priority)
    {
        if (priority == 0) return TaskLevel.Level0;
        if (priority <= 99)
            return TaskLevel.Level1;
        if (priority <= 199) return TaskLevel.Level2;
        return TaskLevel.Level3;
    }
}