using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[Guid("EAEC7A8F-27A0-4DDC-8675-14726A01A38A")]
[CoClass(typeof(TaskHandlerStatusPSClass))]
public interface TaskHandlerStatusPS : ITaskHandlerStatus
{
}
