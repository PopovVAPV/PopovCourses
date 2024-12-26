using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Media;

namespace PopovCourses
{
    public partial class ProfileWindow : Window
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CoursesBase;Integrated Security=True";
        private int CurrentUserId; // ID текущего пользователя

        public ProfileWindow(int userId)
        {
            InitializeComponent();
            CurrentUserId = userId;

            LoadUserProfile();
            LoadUserCourses();
        }

        // Загрузка данных профиля пользователя
        private void LoadUserProfile()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT FullName, BirthDate, Gender, Login FROM Users WHERE UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", CurrentUserId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                FullNameTextBlock.Text = reader.GetString(0);
                                BirthDateTextBlock.Text = reader.GetDateTime(1).ToShortDateString();
                                GenderTextBlock.Text = reader.GetString(2);
                                LoginTextBlock.Text = reader.GetString(3);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Загрузка курсов пользователя
        private void LoadUserCourses()
        {
            try
            {
                var courses = new List<UserCourse>();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT c.Title, c.Duration, c.Instructor, c.Price, e.IsPaid, e.EndDate
                        FROM Enrollments e
                        INNER JOIN Courses c ON e.CourseId = c.CourseId
                        WHERE e.UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", CurrentUserId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                courses.Add(new UserCourse
                                {
                                    Title = reader.GetString(0),
                                    Duration = reader.GetString(1),
                                    Instructor = reader.GetString(2),
                                    Price = reader.GetDecimal(3),
                                    Status = reader.GetBoolean(4) ? "Оплачен" : "Не оплачен",
                                    StatusColor = reader.GetBoolean(4) ? Brushes.Green : Brushes.Red,
                                    EndDate = reader.GetDateTime(5)
                                });
                            }
                        }
                    }
                }

                CoursesListBox.ItemsSource = courses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToMainPage_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
