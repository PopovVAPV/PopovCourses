using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace PopovCourses
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CoursesBase;Integrated Security=True";

        public static User CurrentUser { get; private set; } = null;

        public MainWindow()
        {
            InitializeComponent();
            LoadCoursesFromDatabase();
        }

        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        private void LoadCoursesFromDatabase()
        {
            try
            {
                var courses = new List<Course>();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT CourseId, Title, Duration, Description, Price, Seats, Instructor, ImagePath FROM Courses";

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
                                Instructor = reader.GetString(6),
                                ImagePath = reader.IsDBNull(7) ? null : reader.GetString(7)  
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
                if (CurrentUser == null)
                {
                    LoginRegisterWindow loginWindow = new LoginRegisterWindow();
                    bool? result = loginWindow.ShowDialog();
                    if (result == true && CurrentUser != null)
                    {
                        SubscribeToCourse(selectedCourse);
                    }
                }
                else
                {
                    SubscribeToCourse(selectedCourse);
                }
            }
        }

        private void SubscribeToCourse(Course selectedCourse)
        {
            try
            {
                if (selectedCourse.AvailableSeats > 0)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        string insertQuery = @"
                            INSERT INTO Enrollments (UserId, CourseId, EndDate)
                            VALUES (@UserId, @CourseId, @EndDate)";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
                            command.Parameters.AddWithValue("@CourseId", selectedCourse.CourseId);
                            command.Parameters.AddWithValue("@EndDate", DateTime.Now.AddMonths(1)); 

                            command.ExecuteNonQuery();
                        }

                        string updateSeatsQuery = @"
                            UPDATE Courses 
                            SET Seats = Seats - 1 
                            WHERE CourseId = @CourseId";

                        using (SqlCommand updateCommand = new SqlCommand(updateSeatsQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@CourseId", selectedCourse.CourseId);
                            updateCommand.ExecuteNonQuery();
                        }

                        MessageBox.Show($"Вы успешно подписались на курс: {selectedCourse.Title}", "Успешная подписка", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadCoursesFromDatabase();
                    }
                }
                else
                {
                    MessageBox.Show("Нет доступных мест на этот курс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подписке на курс: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser == null)
            {
                LoginRegisterWindow loginWindow = new LoginRegisterWindow();
                bool? result = loginWindow.ShowDialog();
                if (result == true && CurrentUser != null)
                {
                    MessageBox.Show($"Добро пожаловать, {CurrentUser.FullName}!", "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                ProfileWindow profileWindow = new ProfileWindow(CurrentUser);
                this.Close();
                profileWindow.Show();
            }
        }

    }

    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime BirthDate { get; set; } 
        public bool IsBlocked { get; set; }
        public string Gender { get; set; } 
    }

    public class Course
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public string Instructor { get; set; }
        public string ImagePath { get; set; }  

    }
}
