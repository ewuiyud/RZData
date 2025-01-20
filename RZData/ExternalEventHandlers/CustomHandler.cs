using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ExternalEventHandlers
{
    public class CustomHandler : IExternalEventHandler
    {
        public static CustomHandler Instance;
        private Func<UIApplication, Task> action;
        private TaskCompletionSource<Task> taskCompletionSource;
        private readonly ExternalEvent externalEvent;

        public CustomHandler()
        {
            externalEvent = ExternalEvent.Create(this);
        }
        public static async Task Run(Action<UIApplication> action)
        {
            Instance.action = uiApp => { action.Invoke(uiApp); return Task.CompletedTask; };
            Instance.taskCompletionSource = new TaskCompletionSource<Task>();
            Instance.externalEvent.Raise();
            await Instance.taskCompletionSource.Task;
        }
        public void Execute(UIApplication uiApp)
        {
            taskCompletionSource.SetResult(action.Invoke(uiApp));
        }

        public string GetName()
        {
            return nameof(CustomHandler);
        }
    }
}
