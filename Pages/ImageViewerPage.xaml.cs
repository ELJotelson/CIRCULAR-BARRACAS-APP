namespace Circulacion_Barracas.Pages;

public partial class ImageViewerPage : ContentPage
{
    double currentScale = 1;
    double startScale = 1;

    public ImageViewerPage(string imagePath)
    {
        InitializeComponent();

        FullImage.Source = imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? ImageSource.FromUri(new Uri(imagePath))
            : ImageSource.FromFile(imagePath);
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (sender is not Image image)
            return;

        switch (e.Status)
        {
            case GestureStatus.Started:
                startScale = image.Scale;
                break;

            case GestureStatus.Running:
                currentScale = Math.Max(1, startScale * e.Scale);
                image.Scale = currentScale;
                break;
        }
    }
}
