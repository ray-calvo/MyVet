﻿using MyVet.Common.Helpers;
using MyVet.Common.Models;
using MyVet.Common.Services;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace MyVet.Prism.ViewModels
{
    public class AddEditPetViewModel : ViewModelBase
    {
        private PetResponse _pet;
        private ImageSource _imageSource;
        private bool _isRunning;
        private bool _isEnabled;
        private bool _isEdit;
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private ObservableCollection<PetTypeResponse> _petTypes;
        private PetTypeResponse _petType;

        public AddEditPetViewModel(
            INavigationService navigationService,
            IApiService apiService) :base(navigationService)
        {
            IsEnabled = true;
            _navigationService = navigationService;
            _apiService = apiService;


        }
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public PetResponse Pet
        {
            get => _pet;
            set => SetProperty(ref _pet, value);
        }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }
        public ObservableCollection<PetTypeResponse> PetTypes
        {
            get => _petTypes;
            set => SetProperty(ref _petTypes, value);
        }

        public PetTypeResponse PetType
        {
            get => _petType;
            set => SetProperty(ref _petType, value);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.ContainsKey("pet"))
            {
                Pet = parameters.GetValue<PetResponse>("pet");
                ImageSource = Pet.ImageUrl;
                IsEdit = true;
                Title = "Edit Pet";
            }
            else
            {
               
                Pet = new PetResponse { Born = DateTime.Today };
                ImageSource = "noimagen";
                IsEdit = false;
                Title = "New Pet";
            }
            LoadPetTypes();

        }
        private async void LoadPetTypes()
        {
           
            IsEnabled = false;

            var url = App.Current.Resources["UrlAPI"].ToString();
            var connection = await _apiService.CheckConnection(url);
            if (!connection)
            {
                IsEnabled = true;
                IsRunning = false;
                await App.Current.MainPage.DisplayAlert("Error", "Check the internet connection.", "Accept");
                await _navigationService.GoBackAsync();
                return;
            }
            var token = JsonConvert.DeserializeObject<TokenResponse>(Settings.Token);

            var response = await _apiService.GetListAsync<PetTypeResponse>(url,
                "api",
                "/PetTypes",
                "bearer",
                token.Token);

           
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert("Error", response.Message, "Accept");
                await _navigationService.GoBackAsync();
                return;
            }

            var petTypes = (List<PetTypeResponse>)response.Result;
            PetTypes = new ObservableCollection<PetTypeResponse>(petTypes);

            if (!string.IsNullOrEmpty(Pet.PetType))
            {
                PetType = PetTypes.FirstOrDefault(pt => pt.Name == Pet.PetType);
            }
        }

    }
}

    