using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[CoClass(typeof(TaskSchedulerClass))]
[Guid("2FABA4C7-4DA9-4013-9697-20CC3FD40F85")]
public interface TaskScheduler : ITaskService
{
}
