using AutoMapper;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Enrollments;
using UniversityApi.DTO.Students;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;

namespace UniversityApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Student, StudentDto>();
        CreateMap<StudentCreateDto, Student>();
        CreateMap<StudentUpdateDto, Student>();
        CreateMap<Student, StudentWithCoursesDto>()
            .ForMember(d => d.Courses, o => o.MapFrom(s => s.Enrollments));
        CreateMap<Enrollment, StudentCourseDto>()
            .ForMember(d => d.EnrollmentId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name))
            .ForMember(d => d.Credits, o => o.MapFrom(s => s.Course.Credits));

        CreateMap<Teacher, TeacherDto>();
        CreateMap<TeacherCreateDto, Teacher>();
        CreateMap<TeacherUpdateDto, Teacher>();

        CreateMap<Course, CourseDto>()
            .ForMember(d => d.TeacherName, o => o.MapFrom(s => s.Teacher.FirstName + " " + s.Teacher.LastName));
        CreateMap<CourseCreateDto, Course>();
        CreateMap<CourseUpdateDto, Course>();

        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.FirstName + " " + s.Student.LastName))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name));
        CreateMap<EnrollmentCreateDto, Enrollment>();
        CreateMap<EnrollmentUpdateDto, Enrollment>();
    }
}
