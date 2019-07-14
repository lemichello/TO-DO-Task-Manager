using System;
using System.Windows;

namespace DAO
{
    public static class ErrorHandler
    {
        public static void Handle(Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}