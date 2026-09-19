using System.Runtime.InteropServices;

namespace sergiye.Common;

[ComImport]
[Guid("839D7762-5121-4009-9234-4F0D19394F04")]
[CoClass(typeof(TaskHandlerPSClass))]
public interface TaskHandlerPS : ITaskHandler
{
}
