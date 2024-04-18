namespace pm02Ivan
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Views.PageListMemory());
        }
    }
}
