using LCSC.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LCSC.App.Views
{
    public sealed partial class TournamentsUserControl : UserControl
    {
        public TournamentsUserControl()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService<TournamentsViewModel>();
        }

        public TournamentsViewModel? ViewModel { get; }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}