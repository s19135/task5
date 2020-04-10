using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using lesson5.Models;
using ControllerBase = Microsoft.AspNetCore.Mvc.ControllerBase;

namespace lesson5.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/students")]
    public class StudentsController : ControllerBase
    {
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            var students = new List<Student>();
            using(var sqlConnection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s19135;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlConnection;
                    command.CommandText = "SELECT FirstName,LastName,BirthDate,Name,Semester " +
                                          "FROM Enrollment, Studies, Student " +
                                          "WHERE Studies.IdStudy = Enrollment.IdStudy " +
                                          "AND Enrollment.IdEnrollment = Student.IdEnrollment";
                    sqlConnection.Open();
                    var response = command.ExecuteReader();
                    while (response.Read())
                    {
                        var st = new Student
                        {
                            FirstName = response["FirstName"].ToString(),
                            LastName = response["LastName"].ToString(),
                            Studies = response["Name"].ToString(),
                            BirthDate = DateTime.Parse(response["BirthDate"].ToString()),
                            Semester = int.Parse(response["Semester"].ToString())
                        };
                        students.Add(st);
                    }
                }
            }
            return Ok(students);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            if (AddEnrolment.GetEnrollmentIdWithSemOne(id) == 13)
                return Ok("YES");
            return NotFound("NO");
        }
        
    }
}
