
using System.Windows.Media;
using System;

public class UserCourse
{
    public string Title { get; set; }
    public string Duration { get; set; }
    public string Instructor { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
    public Brush StatusColor { get; set; }
    public DateTime EndDate { get; set; }
}