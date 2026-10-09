# University Course Management API

REST Web API для системы управления университетскими курсами. Хранит информацию о студентах, преподавателях, учебных курсах и записях студентов на курсы. Клиентами API могут выступать Web-, Mobile- и Desktop-приложения.

## Технологии

- .NET 10, ASP.NET Core Web API (контроллеры)
- Entity Framework Core 10 + PostgreSQL (Npgsql)
- AutoMapper
- Swagger / OpenAPI (Swashbuckle)
- ILogger (консольный лог)

## Запуск

1. Должен быть запущен PostgreSQL. Строка подключения находится в `UniversityApi/appsettings.json`:
   ```
   Host=localhost;Port=5432;Database=university_db;Username=postgres;Password=postgres
   ```
   При необходимости поменяйте пароль.
2. Запустите проект:
   ```bash
   dotnet run --project UniversityApi
   ```
3. Откройте Swagger: http://localhost:5175/swagger

При старте приложение само применяет миграции EF Core (`Database.Migrate()`) и заполняет пустую БД тестовыми данными (`Data/DbSeeder.cs`).

Вместо этого базу можно восстановить из дампа `database/university_db.sql` (схема и тестовые данные):
```bash
createdb -U postgres university_db
psql -U postgres -d university_db -f database/university_db.sql
```

> AutoMapper 16 при запуске пишет в лог предупреждение об отсутствии лицензионного ключа. Для учебного проекта это допустимо.

## Архитектура

```
Client (Web / Mobile / Desktop / Swagger / Postman)
  ↓ HTTP (JSON)
Controller            — StudentsController, TeachersController, CoursesController, EnrollmentsController
  ↓ DTO
Service               — бизнес-правила, логирование, формирование ReturnResult<T>
  ↓
Repository            — IStudentRepository → StudentRepository и т.д.
  ↓
Entity Framework Core — ApplicationDbContext (DbSet<Student>, DbSet<Teacher>, DbSet<Course>, DbSet<Enrollment>)
  ↓
Database              — PostgreSQL
```

Все запросы проходят через `ExceptionHandlingMiddleware`, который перехватывает непредвиденные ошибки.

## Структура проекта

```
UniversityApi
├── Controllers          BaseApiController, Students/Teachers/Courses/EnrollmentsController
├── Models               Student, Teacher, Course, Enrollment
├── DTO                  Students, Teachers, Courses, Enrollments
├── Data                 ApplicationDbContext, DbSeeder, Migrations
├── Repositories
│   ├── Interfaces       IStudentRepository, ITeacherRepository, ICourseRepository, IEnrollmentRepository
│   └── Implementations  StudentRepository, TeacherRepository, CourseRepository, EnrollmentRepository
├── Services
│   ├── Interfaces       IStudentService, ITeacherService, ICourseService, IEnrollmentService
│   └── Implementations  StudentService, TeacherService, CourseService, EnrollmentService
├── Mapping              MappingProfile
├── Middleware           ExceptionHandlingMiddleware
├── Common               ReturnResult<T>
├── Program.cs
└── appsettings.json
database/university_db.sql — дамп БД с тестовыми данными
```

## Endpoints

### Students

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/students` | Список всех студентов | 200 |
| GET | `/api/students/{id}` | Студент по ID | 200, 404 |
| GET | `/api/students/{id}/courses` | Студент вместе с его курсами и оценками | 200, 404 |
| POST | `/api/students` | Создать студента (`StudentCreateDto`) | 201, 400, 409 |
| PUT | `/api/students/{id}` | Изменить студента (`StudentUpdateDto`) | 200, 400, 404, 409 |
| DELETE | `/api/students/{id}` | Удалить студента (его записи на курсы удаляются каскадно) | 200, 404 |

### Teachers

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/teachers` | Список преподавателей | 200 |
| GET | `/api/teachers/{id}` | Преподаватель по ID | 200, 404 |
| POST | `/api/teachers` | Создать преподавателя (`TeacherCreateDto`) | 201, 400, 409 |
| PUT | `/api/teachers/{id}` | Изменить преподавателя (`TeacherUpdateDto`) | 200, 400, 404, 409 |
| DELETE | `/api/teachers/{id}` | Удалить преподавателя (нельзя, если за ним закреплены курсы) | 200, 404, 409 |

### Courses

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/courses?name=&teacherId=&minCredits=&maxCredits=` | Список курсов с фильтрацией (все параметры необязательные) | 200 |
| GET | `/api/courses/{id}` | Курс по ID | 200, 404 |
| POST | `/api/courses` | Создать курс и привязать преподавателя (`CourseCreateDto`) | 201, 400, 404 |
| PUT | `/api/courses/{id}` | Изменить курс (`CourseUpdateDto`) | 200, 400, 404 |
| DELETE | `/api/courses/{id}` | Удалить курс | 200, 404 |

### Enrollments

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/enrollments` | Список записей на курсы | 200 |
| GET | `/api/enrollments/{id}` | Запись по ID | 200, 404 |
| POST | `/api/enrollments` | Записать студента на курс (`EnrollmentCreateDto`) | 201, 400, 404, 409 |
| PUT | `/api/enrollments/{id}` | Изменить оценку студента (`EnrollmentUpdateDto`, 0–100) | 200, 400, 404 |
| DELETE | `/api/enrollments/{id}` | Удалить студента с курса | 200, 404 |

Любой endpoint при непредвиденной ошибке возвращает 500.

## Формат ответа

Все endpoints возвращают `ReturnResult<T>`:

```json
{
  "statusCode": 404,
  "isSuccess": false,
  "result": null,
  "errorCode": "STUDENT_NOT_FOUND",
  "errorMessage": "Student not found",
  "traceId": "0HNP65IR8I56Q:00000001"
}
```

Ошибки валидации DTO (`[Required]`, `[EmailAddress]`, `[MaxLength]`, `[Range]`) возвращаются в том же формате с кодом 400 и `errorCode: "VALIDATION_ERROR"`. Непредвиденные ошибки — 500 и `errorCode: "INTERNAL_SERVER_ERROR"` без StackTrace и SQL; подробности пишутся в лог.

## Бизнес-правила

| Правило | Ответ |
|---|---|
| Нельзя записать студента на несуществующий курс | 404 `COURSE_NOT_FOUND` |
| Нельзя записать несуществующего студента на курс | 404 `STUDENT_NOT_FOUND` |
| Студент не может быть дважды записан на один курс | 409 `ALREADY_ENROLLED` (+ уникальный индекс `StudentId, CourseId`) |
| Курс не может ссылаться на несуществующего преподавателя | 404 `TEACHER_NOT_FOUND` |
| Несуществующий объект не возвращается как успешный результат | 404 `*_NOT_FOUND` |
| Email студента / преподавателя уникален | 409 `STUDENT_EMAIL_EXISTS` / `TEACHER_EMAIL_EXISTS` |
| Нельзя удалить преподавателя, у которого есть курсы | 409 `TEACHER_HAS_COURSES` |

## Конфигурация моделей EF Core

- **Conventions** — свойство `Id` становится первичным ключом, `TeacherId`/`StudentId`/`CourseId` — внешними ключами, имена таблиц берутся из `DbSet`.
- **Data Annotations** — `Student`, `Teacher`: `[Key]`, `[Required]`, `[MaxLength]`.
- **Fluent API** — `Course`, `Enrollment` в `ApplicationDbContext.OnModelCreating`: `HasKey`, `IsRequired`, `HasMaxLength`, связи `HasOne/WithMany/HasForeignKey`, `OnDelete`, уникальные индексы.

Связи: Teacher 1 → N Course, Student N ↔ N Course через Enrollment.

## Dependency Injection

Все зависимости регистрируются в `Program.cs` с lifetime **Scoped**:
- `ApplicationDbContext` (через `AddDbContext`) — Scoped по умолчанию: один контекст на HTTP-запрос, он не потокобезопасен.
- Репозитории и сервисы используют `DbContext`, поэтому тоже Scoped: Singleton не может зависеть от Scoped-сервиса, а Transient создавал бы лишние экземпляры внутри одного запроса.
- `IMapper` регистрируется через `AddAutoMapper`, `ILogger<T>` предоставляется ASP.NET Core.

## Логирование

- **Information** — создание, изменение, удаление объектов.
- **Warning** — попытка получить несуществующий объект, нарушение бизнес-правил.
- **Error** — непредвиденные ошибки (`ExceptionHandlingMiddleware`, с полным исключением).

## Сценарии для демонстрации

| Сценарий | Запрос |
|---|---|
| Список студентов | `GET /api/students` |
| Создание студента | `POST /api/students` → 201 |
| Получение созданного студента | `GET /api/students/{id}` |
| Изменение студента | `PUT /api/students/{id}` |
| Создание преподавателя | `POST /api/teachers` |
| Создание курса с преподавателем | `POST /api/courses` с `teacherId` |
| Запись студента на курс | `POST /api/enrollments` → 201 |
| Повторная запись | тот же `POST /api/enrollments` → 409 |
| Студент с курсами | `GET /api/students/{id}/courses` |
| Фильтрация курсов | `GET /api/courses?minCredits=5`, `GET /api/courses?name=web&teacherId=1` |
| Удаление / изменение | `DELETE /api/enrollments/{id}`, `PUT /api/enrollments/{id}` |
| Несуществующий объект | `GET /api/students/999` → 404 |
| Ошибка валидации | `POST /api/students` с `"email": "not-an-email"` → 400 |
| Логи | консоль приложения |

Пример тела `POST /api/students`:
```json
{ "firstName": "Alex", "lastName": "Smith", "email": "alex@student.kz", "birthDate": "2004-03-15" }
```

Пример тела `POST /api/courses`:
```json
{ "name": "Web Services Development", "description": "REST API", "credits": 5, "teacherId": 1 }
```
