using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MyVet.Prism.ViewModels
{
    public class WhatsappPageViewModel : ViewModelBase
    {
        public WhatsappPageViewModel(INavigationService navigationService) : base(navigationService)
        {

            Title = "Whatsapp";
        }
        public ICommand ClickCommand => new Command<string>((url) =>
        {
            var uri = new Uri("https://api.whatsapp.com/send?phone=524434920998&text=Necesito%20Ayuda");
            Device.OpenUri(uri);
        });
    }
}
