using System;
using System.Linq;
using System.Windows;
using ExpenseTrackerApp;

namespace ExpenseTrackerApp
{
    public partial class ExpenseWindow : Window
    {
        private ExpenseTrackerDBEntities _context;
        private Users _currentUser;
        private Expenses _expense;

        public ExpenseWindow(ExpenseTrackerDBEntities context, Users currentUser, Expenses expense = null)
        {
            InitializeComponent();
            _context = context;
            _currentUser = currentUser;
            _expense = expense ?? new Expenses { UserId = _currentUser.UserId, Date = DateTime.Today };

            CategoryComboBox.ItemsSource = _context.Categories
                .Where(c => c.UserId == _currentUser.UserId)
                .ToList();

            if (_expense.ExpenseId != 0)
            {
                AmountTextBox.Text = _expense.Amount.ToString();
                DatePicker.SelectedDate = _expense.Date;
                DescriptionTextBox.Text = _expense.Description;
                CategoryComboBox.SelectedItem = _context.Categories
                    .FirstOrDefault(c => c.CategoryId == _expense.CategoryId);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CategoryComboBox.SelectedItem is Categories selectedCategory &&
                    decimal.TryParse(AmountTextBox.Text, out decimal amount) &&
                    DatePicker.SelectedDate.HasValue)
                {
                    _expense.CategoryId = selectedCategory.CategoryId;
                    _expense.Amount = amount;
                    _expense.Date = DatePicker.SelectedDate.Value;
                    _expense.Description = DescriptionTextBox.Text;

                    if (_expense.ExpenseId == 0)
                    {
                        _context.Expenses.Add(_expense);
                    }
                    _context.SaveChanges();
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Пожалуйста, заполните все поля корректно.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении расхода: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}