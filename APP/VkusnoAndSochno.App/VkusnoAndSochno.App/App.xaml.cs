namespace VkusnoAndSochno.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Задаем заголовок главного окна приложения
            return new Window(new AppShell()) { Title = "Вкусно и сочно" };
        }
    }
}