using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PopovCourses
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CoursesBase;Integrated Security=True";

        public MainWindow()
        {
            InitializeComponent();
            LoadCoursesFromDatabase();
        }

        private void LoadCoursesFromDatabase()
        {
            try
            {
                var courses = new List<Course>();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT CourseId, Title, Duration, Description, Price, Seats, Instructor FROM Courses";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new Course
                            {
                                CourseId = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Duration = reader.GetString(2),
                                Description = reader.GetString(3),
                                Price = reader.GetDecimal(4),
                                AvailableSeats = reader.GetInt32(5),
                                Instructor = reader.GetString(6)
                            });
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

        private void CoursesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoursesListBox.SelectedItem is Course selectedCourse)
            {
                // Проверка авторизации пользователя
                bool isLoggedIn = IsUserLoggedIn();

                if (!isLoggedIn)
                {
                    LoginRegisterWindow loginWindow = new LoginRegisterWindow();
                    bool? result = loginWindow.ShowDialog();
                    if (result == true)
                    {
                        MessageBox.Show($"Вы выбрали курс: {selectedCourse.Title}");
                    }
                }
                else
                {
                    MessageBox.Show($"Вы выбрали курс: {selectedCourse.Title}");
                }
            }
        }

        private bool IsUserLoggedIn()
        {
            // Заглушка: тут можно проверять статический объект CurrentUser
            return false; // TODO: Реализовать проверку авторизации
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
