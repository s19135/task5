using System.Data.SqlClient;

namespace lesson5.Models
{
    public class PromoteStudents
    {
        public string Studies { get; set; }
        public string Semester { get; set; }

        public PromoteStudents()
        {
        }

        public PromoteStudents(string studies, string semester)
        {
            Studies = studies;
            Semester = semester;
        }

        public PromoteStudents(string studies)
        {
            Studies = studies;
        }

        public static bool IsThereStudy(string studyName)
        {
            var sqlConnection =
                new SqlConnection("Data Source=db-mssql;Initial Catalog=s19135;Integrated Security=True");
            var command = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = "SELECT IdStudy " +
                              "FROM Studies " +
                              "WHERE Name=\'" +
                              studyName + "\'"
            };
            sqlConnection.Open();
            SqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                if (int.TryParse(dataReader["IdStudy"].ToString(), out _)) return true;
            }
            return false;
        }
        
        public static PromoteStudents GetEnrolment(string studiesName)
        {
            using (var sqlConnection =
                new SqlConnection("Data Source=db-mssql;Initial Catalog=s19135;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlConnection;
                    command.CommandText = "SELECT Name, MAX(Semester) AS MaxSem " +
                                          "FROM Enrollment, Studies " +
                                          "WHERE Enrollment.IdStudy = Studies.IdStudy " +
                                          "AND Name ='" + studiesName + "' " +
                                          "GROUP BY Name";
                    sqlConnection.Open();
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        return new PromoteStudents()
                        {
                            Studies = dataReader["Name"].ToString(),
                            Semester = dataReader["MaxSem"].ToString()
                        };
                    }
                    return null;
                }
            }
        }
    }
}