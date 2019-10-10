using MyVet.Common.Helpers;
using MyVet.Common.Models;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyVet.Prism.ViewModels
{
    public class ProfilePageViewModel : ViewModelBase
    {
        private bool _isRunning;
        private bool _isEnabled;
        private OwnerResponse _owner;
        private DelegateCommand _saveCommand;
        private DelegateCommand _changePasswordCommand;
        private readonly INavigationService _navigationService;

        public ProfilePageViewModel(
            INavigationService navigationService) : base(navigationService)
        {
            Title = "Modify Profile";
            IsEnabled = true;
            Owner = JsonConvert.DeserializeObject<OwnerResponse>(Settings.Owner);
            _navigationService = navigationService;
        }
        public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(Save));
        public DelegateCommand ChangePasswordCommand => _changePasswordCommand ?? (_changePasswordCommand = new DelegateCommand(ChangePassword));


        public OwnerResponse Owner
        {
            get => _owner;
            set => SetProperty(ref _owner, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private async void Save()
        {
            var isValid = await ValidateData();
            if (!isValid)
            {
                return;
            }
        }

        private async Task<bool> ValidateData()
        {
            if (string.IsNullOrEmpty(Owner.Document))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a document","Accept");
                return false;
            }

            if (string.IsNullOrEmpty(Owner.FirstName))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a firstname", "Accept");
                return false;
            }

            if (string.IsNullOrEmpty(Owner.LastName))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a lastname", "Accept");
                return false;
            }

            if (string.IsNullOrEmpty(Owner.Address))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a Address", "Accept");
                return false;
            }

            return true;
        }
        private async void ChangePassword()
        {
            await _navigationService.NavigateAsync("ChangePasswordPage");
        }

    }
}

