using Windows.UI.Xaml.Controls;
using CameraPi.Server.Services;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace CameraPi.Server
{
	/// <summary>
	///     Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			InitializeComponent();
			var camera = new Camera();
			camera.Emit();
		}
	}
}