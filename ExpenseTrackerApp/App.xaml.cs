using System.Windows;

namespace ExpenseTrackerApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Инициализация базы данных не требуется, так как EDMX работает с существующей базой
        }
    }
}