using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace PopovCourses
{
    public partial class LoginRegisterWindow : Window
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CoursesBase;Integrated Security=True";

        public LoginRegisterWindow()
        {
            InitializeComponent();
        }

        // Логика для входа
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (IsValidUser(login, password))
            {
                MessageBox.Show("Вы успешно вошли!");
                this.DialogResult = true; // Закрытие окна с результатом
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.");
            }
        }

        // Логика для регистрации
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text;
            DateTime? birthDate = BirthDatePicker.SelectedDate;
            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string login = RegisterLoginTextBox.Text;
            string password = RegisterPasswordBox.Password;

            if (IsValidRegistrationData(fullName, birthDate, gender, login, password))
            {
                if (RegisterUser(fullName, birthDate.Value, gender, login, password))
                {
                    MessageBox.Show("Регистрация прошла успешно!");
                    this.DialogResult = true; // Закрытие окна с результатом
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка регистрации. Возможно, логин уже используется.");
                }
            }
            else
            {
                MessageBox.Show("Заполните все поля корректно.");
            }
        }

        // Проверка данных пользователя (например, из базы данных)
        private bool IsValidUser(string login, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT Password FROM Users WHERE Login = @Login AND IsBlocked = 0";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);

                        string storedPassword = command.ExecuteScalar() as string;
                        if (storedPassword != null && storedPassword == password) // Сравниваем пароли напрямую
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        // Проверка корректности данных при регистрации
        // Проверка корректности данных при регистрации
        private bool IsValidRegistrationData(string fullName, DateTime? birthDate, string gender, string login, string password)
        {
            // Проверяем, что данные заполнены корректно и выводим информацию, если что-то не так
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Поле 'ФИО' не может быть пустым.");
                return false;
            }
            if (!birthDate.HasValue)
            {
                MessageBox.Show("Поле 'Дата рождения' должно быть заполнено.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(gender))
            {
                MessageBox.Show("Поле 'Пол' должно быть заполнено.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Поле 'Логин' должно быть заполнено.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Поле 'Пароль' должно быть заполнено.");
                return false;
            }

            // Если все проверки пройдены, возвращаем true
            return true;
        }


        // Добавление нового пользователя в базу данных
        private bool RegisterUser(string fullName, DateTime birthDate, string gender, string login, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Login = @Login";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Login", login);

                        int userCount = (int)checkCommand.ExecuteScalar();
                        if (userCount > 0)
                        {
                            return false; // Логин уже существует
                        }
                    }

                    string insertQuery = "INSERT INTO Users (FullName, BirthDate, Gender, Login, Password) VALUES (@FullName, @BirthDate, @Gender, @Login, @Password)";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@FullName", fullName);
                        insertCommand.Parameters.AddWithValue("@BirthDate", birthDate);
                        insertCommand.Parameters.AddWithValue("@Gender", gender);
                        insertCommand.Parameters.AddWithValue("@Login", login);
                        insertCommand.Parameters.AddWithValue("@Password", password); // Храним пароль в открытом виде

                        insertCommand.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        // Переключение на вкладку регистрации
        private void SwitchToRegistration(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 1;
        }

        // Переключение на вкладку входа
        private void SwitchToLogin(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 0;
        }
    }
}
