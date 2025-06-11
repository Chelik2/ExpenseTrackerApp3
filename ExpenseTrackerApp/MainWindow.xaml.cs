using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExpenseTrackerApp;

namespace ExpenseTrackerApp
{
    public partial class MainWindow : Window
    {
        private ExpenseTrackerDBEntities _context = new ExpenseTrackerDBEntities();
        private Users _currentUser;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                FilterCategoryComboBox.ItemsSource = _context.Categories.ToList();
                AnalyticsPeriodComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке окна: {ex.Message}");
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден");
                    return;
                }
                if (user.Password != password)
                {
                    MessageBox.Show($"Пароль не совпадает. В базе: '{user.Password}', введено: '{password}'");
                    return;
                }
                _currentUser = user;
                ExpensesTab.IsEnabled = true;
                CategoriesTab.IsEnabled = true;
                AnalyticsTab.IsEnabled = true;
                LoadExpenses();
                LoadCategories();
                LoadAnalytics();
                MessageBox.Show("Вход успешен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}");
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;
            if (_context.Users.Any(u => u.Username == username))
            {
                MessageBox.Show("Пользователь уже существует");
                return;
            }
            try
            {
                _context.Users.Add(new Users { Username = username, Password = password });
                _context.SaveChanges();
                MessageBox.Show("Регистрация успешна");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}");
            }
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new ExpenseWindow(_context, _currentUser);
                if (window.ShowDialog() == true)
                {
                    LoadExpenses();
                    LoadAnalytics();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении расхода: {ex.Message}");
            }
        }

        private void EditExpense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ExpensesDataGrid.SelectedItem is Expenses expense)
                {
                    var window = new ExpenseWindow(_context, _currentUser, expense);
                    if (window.ShowDialog() == true)
                    {
                        LoadExpenses();
                        LoadAnalytics();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании расхода: {ex.Message}");
            }
        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ExpensesDataGrid.SelectedItem is Expenses expense)
                {
                    _context.Expenses.Remove(expense);
                    _context.SaveChanges();
                    LoadExpenses();
                    LoadAnalytics();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении расхода: {ex.Message}");
            }
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new CategoryWindow(_context, _currentUser);
                if (window.ShowDialog() == true)
                {
                    LoadCategories();
                    LoadExpenses();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}");
            }
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CategoriesDataGrid.SelectedItem is Categories category)
                {
                    var window = new CategoryWindow(_context, _currentUser, category);
                    if (window.ShowDialog() == true)
                    {
                        LoadCategories();
                        LoadExpenses();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании категории: {ex.Message}");
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CategoriesDataGrid.SelectedItem is Categories category)
                {
                    _context.Categories.Remove(category);
                    _context.SaveChanges();
                    LoadCategories();
                    LoadExpenses();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении категории: {ex.Message}");
            }
        }

        private void LoadExpenses()
        {
            try
            {
                var query = _context.Expenses
                    .Include(e => e.Categories)
                    .Where(e => e.UserId == _currentUser.UserId);

                if (!string.IsNullOrWhiteSpace(FilterTextBox.Text))
                    query = query.Where(e => e.Description.Contains(FilterTextBox.Text));

                if (FilterCategoryComboBox.SelectedItem is Categories category)
                    query = query.Where(e => e.CategoryId == category.CategoryId);

                ExpensesDataGrid.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке расходов: {ex.Message}");
            }
        }

        private void LoadCategories()
        {
            try
            {
                CategoriesDataGrid.ItemsSource = _context.Categories
                    .Where(c => c.UserId == _currentUser.UserId)
                    .ToList();
                FilterCategoryComboBox.ItemsSource = _context.Categories
                    .Where(c => c.UserId == _currentUser.UserId)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}");
            }
        }

        private void LoadAnalytics()
        {
            try
            {
                var period = (AnalyticsPeriodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var expenses = _context.Expenses
                    .Include(e => e.Categories)
                    .Where(e => e.UserId == _currentUser.UserId);

                DateTime startDate;
                if (period == "День")
                    startDate = DateTime.Today;
                else if (period == "Неделя")
                    startDate = DateTime.Today.AddDays(-7);
                else
                    startDate = DateTime.Today.AddDays(-30);

                expenses = expenses.Where(e => e.Date >= startDate);

                var categories = expenses.GroupBy(e => e.Categories.Name)
                    .Select(g => new { Name = g.Key, Total = g.Sum(e => e.Amount) });
                CategoryPieChart.Series = new SeriesCollection();
                foreach (var cat in categories)
                {
                    CategoryPieChart.Series.Add(new PieSeries
                    {
                        Title = cat.Name,
                        Values = new ChartValues<decimal> { cat.Total }
                    });
                }

                var datesQuery = expenses.GroupBy(e => DbFunctions.TruncateTime(e.Date))
                    .Select(g => new { Date = g.Key, Total = g.Sum(e => e.Amount) })
                    .OrderBy(g => g.Date);

                var dates = datesQuery.ToList();

                DateLineChart.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Расходы",
                        Values = new ChartValues<decimal>(dates.Select(d => d.Total))
                    }
                };
                DateLineChart.AxisX.Clear();
                DateLineChart.AxisX.Add(new Axis
                {
                    Labels = dates.Select(d => d.Date?.ToString("d") ?? "").ToList()
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке аналитики: {ex.Message}");
            }
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadExpenses();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadExpenses();
        }

        private void AnalyticsPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentUser != null)
                LoadAnalytics();
        }
    }
}