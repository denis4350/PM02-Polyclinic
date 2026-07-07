using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HospitalSystem
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogError(e.Exception);

            MessageBox.Show(
                "Произошла ошибка:\n\n" + e.Exception.Message +
                "\n\nПриложение продолжит работу. Если ошибка повторится — сообщите об этом.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Без этого одно необработанное исключение в любом окне
            // валило бы всё приложение целиком
            e.Handled = true;
        }

        private void LogError(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string entry = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n";
                File.AppendAllText(logPath, entry);
            }
            catch
            {
                // если не получилось даже записать в лог — молча игнорируем
            }
        }
    }
}