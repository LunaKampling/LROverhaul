using Gwen;
using System;

namespace linerider.UI
{
    public class Fonts(Font defaultf, Font boldf) : IDisposable
    {
        public readonly Font Default = defaultf;
        public readonly Font DefaultBold = boldf;

        public void Dispose()
        {
            Default.Dispose();
            DefaultBold.Dispose();
        }
    }
}