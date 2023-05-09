using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace linerider.Addons
{
    public class BaseAddon
    {
        protected static MainWindow window;
        public BaseAddon() {}

        public static void Initialize(MainWindow mainWindow)
        {
            BaseAddon.window = mainWindow;
        }
    }
}
