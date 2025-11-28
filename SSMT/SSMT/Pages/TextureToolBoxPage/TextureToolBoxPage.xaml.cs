using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SSMT.ViewModels;

namespace SSMT
{
    public sealed partial class TextureToolBoxPage : Page
    {
        public TextureToolBoxPageViewModel ViewModel { get; }

        public TextureToolBoxPage()
        {
            this.InitializeComponent();
            ViewModel = new TextureToolBoxPageViewModel();
            DataContext = ViewModel;
        }
    }
}
