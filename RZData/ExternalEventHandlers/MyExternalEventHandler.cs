using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

//todo: 将保存功能放到外部事件处理程序中
public class MyExternalEventHandler : IExternalEventHandler
{
    private Action Action { get; set; }
    public void Execute(UIApplication app)
    {
        // 在这里放置您的事务代码
        using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "修改族参数"))
        {
            transaction.Start();
            Action?.Invoke();
            transaction.Commit();
        }
    }
    public string GetName()
    {
        return "My External Event Handler";
    }
}

//MyExternalEventHandler handler = new MyExternalEventHandler();
//ExternalEvent exEvent = ExternalEvent.Create(handler);
//exEvent.Raise();
