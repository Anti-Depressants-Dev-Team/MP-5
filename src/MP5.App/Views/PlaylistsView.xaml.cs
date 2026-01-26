using MP5.Core.ViewModels;

namespace MP5.App.Views;

public partial class PlaylistsView : ContentView
{
    public PlaylistsView(PlaylistsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Loaded += (s, e) => viewModel.LoadCommand.Execute(null);
    }
}
