namespace linerider.Addons
{
    public class BaseAddon
    {
        protected static MainWindow window;
        public BaseAddon() { }

        public static void Initialize(MainWindow mainWindow) => window = mainWindow;
    }
}