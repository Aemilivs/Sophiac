namespace Sophiac.UI;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		InitializeComponent();
		MainPage = new MainPage();
	}

	protected override void OnStart()
	{
		var path = Path.Combine(FileSystem.Current.AppDataDirectory, "com.github.aemilivs.sophiac");
		var info = Directory.CreateDirectory(path);
		base.OnStart();
	}
}
