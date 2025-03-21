using Module1.Views;
using Prism.DryIoc;
using Prism.Ioc;
using System.Windows;

namespace RZData
{
    public class PrismApp : PrismApplication
    {
        protected override Window CreateShell()
        {
            return null;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ViewA>();
        }
    }
}
