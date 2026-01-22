namespace MP5.App.Views;

public partial class HomeView : ContentView
{
	public HomeView()
	{
		InitializeComponent();
        UpdateGreeting();
	}
    
    private void UpdateGreeting()
    {
        var hour = DateTime.Now.Hour;
        var greeting = hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            < 21 => "Good evening",
            _ => "Good night"
        };
        
        GreetingLabel.Text = greeting;
    }
}
