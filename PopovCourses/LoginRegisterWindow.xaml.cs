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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (IsValidUser(login, password))
            {
                MessageBox.Show("Вы успешно вошли!");

                if (login == "Admin")
                {
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Close(); 
                }
                else
                {
                    User currentUser = GetUserData(login); 
                    MainWindow.SetCurrentUser(currentUser); 
                    this.DialogResult = true; 
                    this.Close();

                    if (MainWindow.CurrentUser != null)
                    {
                        ProfileWindow profileWindow = new ProfileWindow(currentUser); 
                        profileWindow.Show();
                    }
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.");
            }
        }



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
                    this.DialogResult = true; 
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

        private bool IsValidUser(string login, string password)
        {
            if (login == "Admin" && password == "admin")
            {
                return true; 
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT Password, IsBlocked FROM Users WHERE Login = @Login AND IsBlocked = 0";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);

                        string storedPassword = command.ExecuteScalar() as string;
                        if (storedPassword != null && storedPassword == password)
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
        private bool IsValidRegistrationData(string fullName, DateTime? birthDate, string gender, string login, string password)
        {
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

        // Получение данных о пользователе по логину
        private User GetUserData(string login)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT UserId, FullName, Login, BirthDate, Gender FROM Users WHERE Login = @Login";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserId = reader.GetInt32(0),
                                    FullName = reader.GetString(1),
                                    Login = reader.GetString(2),
                                    BirthDate = reader.GetDateTime(3), // Добавьте дату рождения
                                    Gender = reader.GetString(4) // Добавьте пол
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }



        // Переключение на вкладку входа
        private void SwitchToLogin(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 0;
        }
    }
}
