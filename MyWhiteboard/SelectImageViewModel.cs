using MyWhiteboard.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MyWhiteboard
{
    public class SelectImageViewModel : INotifyPropertyChanged
    {
        private bool isLoading;

        public SelectImageViewModel()
        {
            Images = new ObservableCollection<BackgroundImageDescription>();

            LoadImages();
        }

        public ObservableCollection<BackgroundImageDescription> Images { get; }

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (value == isLoading)
                {
                    return;
                }
                isLoading = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void LoadImages()
        {
            IsLoading = true;
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(new Uri($"{Consts.ApiControllerBaseUrl}GetImageDescriptions"));
            var backgroundImages = await response.Content.ReadAsAsync<List<BackgroundImageDescription>>();
            foreach (var backgroundImage in backgroundImages)
            {
                Images.Add(backgroundImage);
                await Task.Delay(500);
            }

            IsLoading = false;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}