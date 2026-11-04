using VkusnoAndSochno.App.ViewModels;

namespace VkusnoAndSochno.App.Views
{
    public partial class CashierPage : ContentPage
    {
        public CashierPage(CashierViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}