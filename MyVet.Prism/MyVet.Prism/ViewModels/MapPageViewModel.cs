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
    public class MapPageViewModel : ViewModelBase
    {
        public MapPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Title = "Map";
        }
        public ICommand ClickCommand => new Command<string>((url) =>
        {
            var uri = new Uri("https://www.google.com.mx/maps/place/Veterinaria+Nuske/@19.6826521,-101.1710732,17z/data=!3m1!4b1!4m5!3m4!1s0x842d0dfa753bf283:0xd6c79de17f8c52d0!8m2!3d19.6826521!4d-101.1688845");
            Device.OpenUri(uri);
        });
    }
}
