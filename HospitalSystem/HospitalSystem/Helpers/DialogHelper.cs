using System.Windows;

namespace HospitalSystem.Helpers
{
    public static class DialogHelper
    {
        public static bool Confirm(string message)
        {
            return MessageBox.Show(message, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}