using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;

namespace PopovCourses
{
    public partial class AdminWindow : Window
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CoursesBase;Integrated Security=True";

        public AdminWindow()
        {
            InitializeComponent();
            LoadUsersData();
        }

        private void LoadUsersData()
        {
            try
            {
                List<User> users = new List<User>();
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT UserId, FullName, Login, BirthDate, Gender, IsBlocked FROM Users";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                UserId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                Login = reader.GetString(2),
                                BirthDate = reader.GetDateTime(3),
                                Gender = reader.GetString(4),
                                IsBlocked = reader.GetBoolean(5)
                            });
                        }
                    }
                }
                UsersDataGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BlockUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (selectedUser.IsBlocked)
                {
                    MessageBox.Show("Этот пользователь уже заблокирован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                        {
                            connection.Open();
                            string query = "UPDATE Users SET IsBlocked = 1 WHERE UserId = @UserId";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@UserId", selectedUser.UserId);
                                command.ExecuteNonQuery();
                            }
                        }
                        MessageBox.Show("Пользователь заблокирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsersData(); 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при блокировке пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для блокировки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UnblockUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (!selectedUser.IsBlocked)
                {
                    MessageBox.Show("Этот пользователь не заблокирован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                        {
                            connection.Open();
                            string query = "UPDATE Users SET IsBlocked = 0 WHERE UserId = @UserId";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@UserId", selectedUser.UserId);
                                command.ExecuteNonQuery();
                            }
                        }
                        MessageBox.Show("Пользователь разблокирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsersData(); 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при разблокировке пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для разблокировки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Users WHERE UserId = @UserId";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UserId", selectedUser.UserId);
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Пользователь удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsersData(); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
