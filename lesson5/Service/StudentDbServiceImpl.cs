using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using lesson5.Models;

namespace lesson5.Service
{
    public class StudentDbServiceImpl : IStudentDbService
        {
            public AddEnrolment EnrollStudent(AddEnrolment student)
            {
                var random = new Random();
                var idStudent = student.IdStudent;
                var firstName = student.FirstName;
                var lastName = student.LastName;
                var birthDate = student.BirthDate;
                var studies = student.Studies;

                var isNumeric = int.TryParse(idStudent, out _);

                if (string.IsNullOrEmpty(idStudent) || !isNumeric)
                    return new AddEnrolment("Invalid Data In The Field IdStudent");
                if (string.IsNullOrEmpty(firstName) || !Regex.Match(firstName,
                    "^([A-Z])([a-z]{1,})$").Success)
                    return new AddEnrolment("Invalid Data In The Field FirstName");
                if (string.IsNullOrEmpty(lastName) || !Regex.Match(lastName,
                    "^([A-Z])([a-z]{1,})$").Success)
                    return new AddEnrolment("Invalid Data In The Field LastName");
                if (string.IsNullOrEmpty(birthDate) || !Regex.Match(birthDate,
                    @"^(0[1-9]|1[012])[\-](0[1-9]|[12][0-9]|3[01])[\-](19|20)\d\d$").Success)
                    return new AddEnrolment("Invalid Data In The Field BirthDate");
                if (string.IsNullOrEmpty(studies) || !Regex.Match(studies,
                    "^([A-Z]{1})([A-Za-z]{1,})$").Success)
                    return new AddEnrolment("Invalid Data In The Field Studies");

                if (AddEnrolment.IsThereStudentWithId(idStudent))
                    return new AddEnrolment("Provided IdStudent Is Already In Use.");
                if (!AddEnrolment.IsThereStudy(studies))
                    return new AddEnrolment("There Is No Such Study");

                var createCustomEnrollment = !AddEnrolment.DoesStudyHaveSemesterOne(studies);
                var studyId = AddEnrolment.GetStudyId(studies);
                int customEnrollmentId;
                while (true)
                {
                    customEnrollmentId = random.Next(0, 2000);
                    if (!AddEnrolment.IsThereEnrollmentWithId(customEnrollmentId.ToString())) break;
                }

                var enrollmentId = AddEnrolment.GetEnrollmentIdWithSemOne(studyId);
                using (var sqlConnection =
                    new SqlConnection(
                        "Data Source=db-mssql;Initial Catalog=s19135;Integrated Security=True"))
                {
                    sqlConnection.Open();
                    SqlTransaction transaction = sqlConnection.BeginTransaction("SampleTransaction");
                    if (createCustomEnrollment)
                    {
                        using (var customEnrollmentCommand = new SqlCommand())
                        {
                            customEnrollmentCommand.Connection = sqlConnection;
                            customEnrollmentCommand.CommandText = "INSERT INTO Enrollment" +
                                                                  "(IdEnrollment, Semester, IdStudy, StartDate) " +
                                                                  "VALUES (@IdEnrollment, @Semester, @IdStudy, @StartDate)";
                            customEnrollmentCommand.Parameters.AddWithValue("idEnrollment", customEnrollmentId);
                            customEnrollmentCommand.Parameters.AddWithValue("Semester", 1);
                            customEnrollmentCommand.Parameters.AddWithValue("IdStudy", studyId);
                            customEnrollmentCommand.Parameters.AddWithValue("StartDate", DateTime.Now);
                            customEnrollmentCommand.Transaction = transaction;
                            try
                            {
                                customEnrollmentCommand.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Commit Exception Type: {0}", e.GetType());
                                Console.WriteLine("  Message: {0}", e.Message);
                                try
                                {
                                    transaction.Rollback();
                                    return new AddEnrolment(e.Message);
                                }
                                catch (Exception e2)
                                {
                                    Console.WriteLine("Rollback Exception Type: {0}", e2.GetType());
                                    Console.WriteLine("  Message: {0}", e2.Message);
                                    return new AddEnrolment(e.Message);
                                }
                            }
                        }
                    }

                    using (var mainCommand = new SqlCommand())
                    {
                        mainCommand.Connection = sqlConnection;
                        mainCommand.CommandText = "INSERT INTO Student " +
                                                  "(IdStudent, FirstName, LastName, BirthDate, IdEnrollment) " +
                                                  "VALUES (@IdStudent, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                        mainCommand.Parameters.AddWithValue("idStudent", idStudent);
                        mainCommand.Parameters.AddWithValue("FirstName", firstName);
                        mainCommand.Parameters.AddWithValue("LastName", lastName);
                        if (!DateTime.TryParseExact(birthDate, "MM-dd-yyyy", null, DateTimeStyles.None,
                            out var simpleDate))
                            return new AddEnrolment("Problem with Parsing the Date");
                        mainCommand.Parameters.AddWithValue("BirthDate", simpleDate);
                        mainCommand.Parameters.AddWithValue("IdEnrollment", enrollmentId);
                        mainCommand.Transaction = transaction;
                        try
                        {
                            mainCommand.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Commit Exception Type: {0}", e.GetType());
                            Console.WriteLine("  Message: {0}", e.Message);
                            try
                            {
                                transaction.Rollback();
                                return new AddEnrolment(e.Message);
                            }
                            catch (Exception e2)
                            {
                                Console.WriteLine("Rollback Exception Type: {0}", e2.GetType());
                                Console.WriteLine("  Message: {0}", e2.Message);
                                return new AddEnrolment(e.Message);
                            }
                        }
                    }

                    transaction.Commit();
                }

                AddEnrolment result = AddEnrolment.GetStudent(student.IdStudent);
                return result;
            }

            public PromoteStudents Promote(PromoteStudents study)
            {
                var studies = study.Studies;
                var semester = study.Semester;
                var isNumeric = int.TryParse(semester, out _);

                if (string.IsNullOrEmpty(studies) || !Regex.Match(studies,
                    "^([A-Z]{1})([A-Za-z]{1,})$").Success)
                    return new PromoteStudents("Invalid Data In The Field Studies");
                if (string.IsNullOrEmpty(semester) || !isNumeric)
                    return new PromoteStudents("Invalid Data In The Field IdStudent");

                if (!PromoteStudents.IsThereStudy(studies))
                    return new PromoteStudents("There Is No Such Study");

                using (var sqlConnection =
                    new SqlConnection(
                        "Data Source=db-mssql;Initial Catalog=s19135;Integrated Security=True"))
                {
                    sqlConnection.Open();
                    SqlTransaction transaction = sqlConnection.BeginTransaction("Transaction");
                    using (var mainCommand = new SqlCommand())
                    {
                        mainCommand.Connection = sqlConnection;
                        mainCommand.CommandText = "EXEC PromoteProcedure @Name, @Semester";
                        mainCommand.Parameters.AddWithValue("Name", study.Studies);
                        mainCommand.Parameters.AddWithValue("Semester", study.Semester);
                        mainCommand.Transaction = transaction;
                        mainCommand.ExecuteNonQuery();
                        try
                        {
                            mainCommand.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Commit Exception Type: {0}", e.GetType());
                            Console.WriteLine("  Message: {0}", e.Message);
                            try
                            {
                                transaction.Rollback();
                                return new PromoteStudents(e.Message);
                            }
                            catch (Exception e2)
                            {
                                Console.WriteLine("Rollback Exception Type: {0}", e2.GetType());
                                Console.WriteLine("  Message: {0}", e2.Message);
                                return new PromoteStudents(e.Message);
                            }
                        }

                        transaction.Commit();
                    }
                }

                var result = PromoteStudents.GetEnrolment(study.Studies);
                return result;
            }
        }
    }