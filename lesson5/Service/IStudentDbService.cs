using lesson5.Models;

namespace lesson5.Service
{
    public interface IStudentDbService
    {
        AddEnrolment EnrollStudent(AddEnrolment student);
        PromoteStudents Promote(PromoteStudents study);
    }
}