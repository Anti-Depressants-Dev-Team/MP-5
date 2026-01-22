using MP5.Core.ViewModels;

namespace MP5.App.Views;

public partial class SearchView : ContentView
{
    private readonly SearchViewModel _viewModel;

	public SearchView(SearchViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
	}
}
