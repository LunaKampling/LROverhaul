using System;
using System.Diagnostics;

namespace TestApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));
            using (OpenTK.Toolkit.Init(new OpenTK.ToolkitOptions() { Backend = OpenTK.PlatformBackend.Default }))
            {
                Window w = new Window();
                w.Run(0,30);
                w.Dispose();
            }
        }
    }
}
