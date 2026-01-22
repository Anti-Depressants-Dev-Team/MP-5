using MP5.Core.ViewModels;

namespace MP5.App.Views;

public partial class SettingsView : ContentView
{
	public SettingsView(SettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
        
        // Load settings when view is created
        viewModel.LoadCommand.Execute(null);
	}
}
