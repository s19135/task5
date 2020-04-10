using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using lesson5.Models;
using lesson5.Service;
using ControllerBase = Microsoft.AspNetCore.Mvc.ControllerBase;

namespace lesson5.Controllers
{
    
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentController : ControllerBase
    {
        private IStudentDbService _service;
        public EnrollmentController(IStudentDbService service)
        {
            _service = service;
        }

        [HttpPost(Name = nameof(EnrollStudent))]
        [Route("enroll")]
        [SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
        public IActionResult EnrollStudent(AddEnrolment student)
        {
            var result = _service.EnrollStudent(student);
            if (result.Studies != null) return CreatedAtAction(nameof(EnrollStudent), result);
            return BadRequest(result.IdStudent);
        }


        [HttpGet("{idStudent}", Name = "StudentGetter")]
        public IActionResult GetStudent(string idStudent)
        {
            var sqlConnection =
                new SqlConnection("Data Source=db-mssql;Initial Catalog=s19135;Integrated Security=True");
            var command = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = "SELECT  Semester " +
                              "FROM Student, Enrollment " +
                              "WHERE IdStudent=@idStudent " +
                              "AND Student.IdEnrollment = Enrollment.IdEnrollment"
            };
            SqlParameter parameter = new SqlParameter();
            command.Parameters.AddWithValue("idStudent", idStudent);
            sqlConnection.Open();
            SqlDataReader dataReader = command.ExecuteReader();
            while(dataReader.Read()) 
                return Ok("Student(" + idStudent + ") started his/her studies in " +
                          Int32.Parse(dataReader["Semester"].ToString()) + ".");
            return NotFound("Invalid Input Provided");
        }
        
        [Microsoft.AspNetCore.Mvc.HttpPost(Name = nameof(Promote))]
        [Microsoft.AspNetCore.Mvc.Route("promote")]
        public IActionResult Promote(PromoteStudents study)
        {
            var result = _service.Promote(study);
            if (result.Semester != null) return CreatedAtAction(nameof(Promote), result);
            return BadRequest(result.Studies);
        }
    }
}
