using System;

namespace lesson5.Models
{
    public class Student
    {
        public Student(string firstName, string lastName, string Studies, int Semester, DateTime birthDate)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Studies = Studies;
            this.Semester = Semester;
            this.BirthDate = birthDate;
        }

        public Student() { }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Studies { get; set; }
        public int Semester { get; set; }
        public DateTime BirthDate { get; set; }
    }
}