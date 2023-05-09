using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace linerider.Addons
{
    public class AddonManager
    {
        public static void Initialize(MainWindow mainWindow)
        {
            MagicAnimator.Initialize(mainWindow);
        }
    }
}
