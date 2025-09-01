using Plugin.Maui.OCR;

namespace HalalScanner.Services
{
    public class CameraService
    {
        public async Task<string> CaptureAndExtractTextAsync()
        {
            try
            {
                // Take photo
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo == null)
                    return string.Empty;

                // Convert to stream
                using var stream = await photo.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                byte[] imageBytes = ms.ToArray();

                // Perform OCR
                var ocrResult = await OcrPlugin.Default.RecognizeTextAsync(imageBytes);
                return ocrResult.AllText;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Failed to capture and process image: {ex.Message}", "OK");
                return string.Empty;
            }
        }

        public async Task<string> PickImageAndExtractTextAsync()
        {
            try
            {
                // Pick photo from gallery
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo == null)
                    return string.Empty;

                // Convert to stream
                using var stream = await photo.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                byte[] imageBytes = ms.ToArray();

                // Perform OCR
                var ocrResult = await OcrPlugin.Default.RecognizeTextAsync(imageBytes);
                return ocrResult.AllText;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Failed to process image: {ex.Message}", "OK");
                return string.Empty;
            }
        }
    }
}
