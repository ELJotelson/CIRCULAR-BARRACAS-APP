namespace Circulacion_Barracas;

public partial class ImageViewerPage : ContentPage
{
    public ImageViewerPage(string imagePath)
    {
        InitializeComponent();

        if (!string.IsNullOrWhiteSpace(imagePath))
        {
            if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                FullImage.Source = ImageSource.FromUri(new Uri(imagePath));
            else
                FullImage.Source = ImageSource.FromFile(imagePath);
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}