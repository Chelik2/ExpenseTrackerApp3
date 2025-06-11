using System.Data.Entity;
using System.Windows;

namespace ExpenseTrackerApp
{
    public partial class CategoryWindow : Window
    {
        private readonly ExpenseTrackerDBEntities _context;
        private readonly Users _user;
        private readonly Categories _category;

        public CategoryWindow(ExpenseTrackerDBEntities context, Users user, Categories category = null)
        {
            InitializeComponent();
            _context = context;
            _user = user;
            _category = category ?? new Categories { UserId = user.UserId };
            NameTextBox.Text = _category.Name;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                _category.Name = NameTextBox.Text;
                if (_category.CategoryId == 0)
                    _context.Categories.Add(_category);
                else
                    _context.Entry(_category).State = EntityState.Modified;

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите название категории");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}