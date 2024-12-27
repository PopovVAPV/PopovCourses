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
        private User CurrentUser;

        public ProfileWindow(User user)
        {
            InitializeComponent();
            CurrentUser = user;

            LoadUserProfile();
            LoadUserCourses();
        }

        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesListBox.SelectedItem is UserCourse selectedCourse)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        bool newStatus = selectedCourse.Status == "Не оплачен";
                        string updateQuery = "UPDATE Enrollments SET IsPaid = @IsPaid WHERE UserId = @UserId AND CourseId = @CourseId";

                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@IsPaid", newStatus);
                            command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                            command.Parameters.AddWithValue("@CourseId", selectedCourse.CourseId);

                            command.ExecuteNonQuery();
                        }

                        selectedCourse.Status = newStatus ? "Оплачен" : "Не оплачен";
                        selectedCourse.StatusColor = newStatus ? Brushes.Green : Brushes.Red;
                        CoursesListBox.Items.Refresh();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


     
        private void LoadUserProfile()
        {
            try
            {
                FullNameTextBlock.Text = CurrentUser.FullName;
                BirthDateTextBlock.Text = CurrentUser.BirthDate != DateTime.MinValue
                    ? CurrentUser.BirthDate.ToShortDateString()
                    : "Не указана";
                GenderTextBlock.Text = !string.IsNullOrEmpty(CurrentUser.Gender)
                    ? CurrentUser.Gender
                    : "Не указан";
                LoginTextBlock.Text = CurrentUser.Login;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    
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
                        command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);

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

                if (courses.Count == 0)
                {
                    MessageBox.Show("Вы не подписаны на курсы.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BackToMainPage_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetCurrentUser(null);
            MessageBox.Show("Вы вышли из профиля.", "Выход", MessageBoxButton.OK, MessageBoxImage.Information);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }

    public class UserCourse
    {
        public int CourseId { get; set; } 
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Instructor { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public Brush StatusColor { get; set; }
        public DateTime EndDate { get; set; }
    }
}
