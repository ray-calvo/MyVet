using Android.App;
using Android.OS;

namespace MyVet.Prism.Droid
{
    [Activity(
        Theme = "@style/Theme.Splash",
        MainLauncher = true,
        NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            System.Threading.Thread.Sleep(900);//Tiempo que tarda el splash
            StartActivity(typeof(MainActivity));
        }
    }
}
