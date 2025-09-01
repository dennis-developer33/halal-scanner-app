using HalalScanner.Services;

namespace HalalScanner
{
    public partial class MainPage : ContentPage
    {
        private readonly CameraService _cameraService;
        private readonly HalalCheckerService _halalCheckerService;

        public MainPage()
        {
            InitializeComponent();

            _cameraService = new CameraService();
            _halalCheckerService = new HalalCheckerService();


            // Initialize the service
            Task.Run(async () => await _halalCheckerService.InitializeAsync());

        }

        private async void OnCameraButtonClicked(object sender, EventArgs e)
        {
            await ProcessImageAsync(() => _cameraService.CaptureAndExtractTextAsync());
        }

        private async void OnGalleryButtonClicked(object sender, EventArgs e)
        {  
            await ProcessImageAsync(() => _cameraService.PickImageAndExtractTextAsync());
        }

        private async void OnCheckManualButtonClicked(object sender, EventArgs e)
        {
            var text = ManualInputEditor.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                await DisplayAlert("Input Required", "Please enter some ingredients to check.", "OK");
                return;
            }

            ShowLoading(true);
            await CheckHalalStatus(text);
            ShowLoading(false);
        }

        private async Task ProcessImageAsync(Func<Task<string>> extractTextFunc)
        {
            ShowLoading(true);

            try
            {
                var extractedText = await extractTextFunc();

                if (string.IsNullOrEmpty(extractedText))
                {
                    await DisplayAlert("No Text Found", "Could not extract text from the image. Please try again.", "OK");
                    ShowLoading(false);
                    return;
                }

                // Show extracted text
                ExtractedTextLabel.Text = extractedText;
                ExtractedTextFrame.IsVisible = true;

                // Check halal status
                await CheckHalalStatus(extractedText);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task CheckHalalStatus(string text)
        {
            var result = _halalCheckerService.CheckIngredients(text);

            // Update UI
            ResultSummary.Text = result.Summary;
            ResultSummary.TextColor = result.IsHalal ? Colors.Green : Colors.Red;

            HaramIngredientsView.ItemsSource = result.FoundHaramIngredients;
            ResultFrame.IsVisible = true;

            // Update result frame border color
            ResultFrame.BorderColor = result.IsHalal ? Colors.Green : Colors.Red;
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;

            CameraButton.IsEnabled = !isLoading;
            GalleryButton.IsEnabled = !isLoading;
            CheckManualButton.IsEnabled = !isLoading;
        }
    }
}
