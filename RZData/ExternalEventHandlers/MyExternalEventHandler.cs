using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

//todo: �����湦�ܷŵ��ⲿ�¼����������
public class MyExternalEventHandler : IExternalEventHandler
{
    private Action Action { get; set; }
    public void Execute(UIApplication app)
    {
        // ��������������������
        using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "�޸������"))
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
