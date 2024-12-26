using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PopovCourses
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();

            // Пример данных пользователей
            var users = new List<UserAccount>
            {
                new UserAccount { FullName = "Иван Иванов", Login = "ivanov", Status = "Активен", StatusColor = Brushes.Green },
                new UserAccount { FullName = "Мария Петрова", Login = "petrova", Status = "Заблокирован", StatusColor = Brushes.Red }
            };

            // Привязываем список пользователей
            UsersListBox.ItemsSource = users;
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Изменение данных пользователя.");
            // Логика изменения данных пользователя
        }

        private void BlockUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Пользователь заблокирован.");
            // Логика блокировки пользователя
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Пользователь удален.");
            // Логика удаления пользователя
        }
    }
}
