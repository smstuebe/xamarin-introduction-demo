using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace XamarinDemo
{
    public partial class MainPage : ContentPage
    {
        private string _status;
        private ImageSource _source;

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public ImageSource Source
        {
            get { return _source; }
            set
            {
                _source = value;
                OnPropertyChanged();
            }
        }

        public ICommand TakePhotoCommand { get; }

        public MainPage()
        {
            InitializeComponent();
            Status = "Select an Image!";
            TakePhotoCommand = new Command(() => TakePhotoAsync());
            BindingContext = this;
        }

        private async Task TakePhotoAsync()
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "test.jpg",
                DefaultCamera = CameraDevice.Front
            });

            if (file == null)
                return;

            Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });

            var faces = await UploadAndDetectFaces(file);

            if (faces.Any())
            {
                var face = faces.First();
                Status = $"{face.FaceAttributes.Gender}, {face.FaceAttributes.Age} years";
            }
            else
            {
                Status = "no face found";
            }
        }

        private async Task<Face[]> UploadAndDetectFaces(MediaFile file)
        {
            Status = "uploading + recognizing";
            try
            {
                using (var imageFileStream = file.GetStream())
                {
                    var faces = await _faceServiceClient.DetectAsync(imageFileStream, returnFaceAttributes: new[] { FaceAttributeType.Age, FaceAttributeType.Gender });
                    return faces.ToArray();
                }
            }
            catch (Exception e)
            {
                return new Face[0];
            }
        }
    }
}
