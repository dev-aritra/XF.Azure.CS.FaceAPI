using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XF.Azure.CS.FaceAPI.View
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CaptureImagePage : ContentPage
	{
		public CaptureImagePage ()
		{
			InitializeComponent ();
		}
	}
}