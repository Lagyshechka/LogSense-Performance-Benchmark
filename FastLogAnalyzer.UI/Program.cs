using System;
using System.Windows;

namespace FastLogAnalyzer.UI
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var app = new App();
                app.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "CRASH AT START", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    } 
}