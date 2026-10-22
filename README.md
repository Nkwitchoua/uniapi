# University Course Management API

REST Web API для системы управления университетскими курсами. Хранит информацию о студентах, преподавателях, учебных курсах и записях студентов на курсы.

**Задание 2** добавило к API из задания 1: ASP.NET Core Identity, JWT-аутентификацию, роли Admin / Teacher / Student с проверкой принадлежности ресурсов, версии API v1 и v2, CORS и отдельное клиентское приложение `UniversityClient`.

## Технологии

- .NET 10, ASP.NET Core Web API (контроллеры)
- Entity Framework Core 10 + PostgreSQL (Npgsql)
- ASP.NET Core Identity (EF Core stores), JWT Bearer
- Asp.Versioning (версии API через сегмент URL)
- AutoMapper
- Swagger / OpenAPI (Swashbuckle) — документы v1 и v2, авторизация по JWT
- Клиент: ASP.NET Core Razor Pages + `IHttpClientFactory`

## Запуск

1. Запустите PostgreSQL. Строка подключения — `UniversityApi/appsettings.json`:
   ```
   Host=localhost;Port=5432;Database=university_db;Username=postgres;Password=postgres
   ```
2. Задайте ключ подписи JWT. В репозитории ключа нет — он хранится в user-secrets (вне папки проекта) или в переменной окружения:
   ```bash
   dotnet user-secrets set "Jwt:Key" "<случайная строка не короче 32 символов>" --project UniversityApi
   ```
   Для сервера вместо user-secrets используется переменная окружения `Jwt__Key`. Без ключа API не стартует и пишет, как его задать.
3. Запустите API:
   ```bash
   dotnet run --project UniversityApi
   ```
   Swagger: http://localhost:5175/swagger (в правом верхнем углу выбирается документ V1 или V2).
4. Запустите клиент (в другом терминале):
   ```bash
   dotnet run --project UniversityClient
   ```
   Клиент: http://localhost:5200

При старте API применяет миграции (`InitialCreate`, `AddIdentityAndRoles`), заполняет пустую БД тестовыми данными (`Data/DbSeeder.cs`), создаёт роли и тестовых пользователей (`Data/IdentitySeeder.cs`). Вместо этого базу можно восстановить из дампа `database/university_db.sql`:
```bash
createdb -U postgres university_db
psql -U postgres -d university_db -f database/university_db.sql
```

### Тестовые пользователи

| Логин | Пароль | Роль | Связь |
|---|---|---|---|
| `admin` | `Admin123!` | Admin | — |
| `ivanov` | `Teacher123!` | Teacher | Teacher #1 Ivan Ivanov (курс Web Services Development) |
| `petrova` | `Teacher123!` | Teacher | Teacher #3 Anna Petrova (Linear Algebra, Calculus) |
| `alex` | `Student123!` | Student | Student #1 Alex Smith |
| `maria` | `Student123!` | Student | Student #2 Maria Kim |

Это учебные пароли только для локальной демонстрации.

## Аутентификация и авторизация

**Аутентификация** отвечает на вопрос «кто вы?»: API проверяет JWT (подпись, издатель, аудитория, срок действия). Нет токена или он плохой → **401 Unauthorized**.
**Авторизация** отвечает на вопрос «что вам можно?»: пользователь уже известен, проверяются его роль и права на конкретный объект. Прав нет → **403 Forbidden**.

Пример: `DELETE /api/courses/1` без токена → 401 (не знаем, кто это). Тот же запрос с токеном студента `alex` → 403 (знаем, что это студент, а удалять курсы может только Admin).

### Endpoints аутентификации

| Метод | URL | Доступ | Описание |
|---|---|---|---|
| POST | `/api/auth/register` | все | Регистрация. Создаёт учётную запись **и** запись Student, роль всегда `Student` |
| POST | `/api/auth/login` | все | Вход, возвращает `accessToken` (JWT) и `expiresAt` |
| GET | `/api/auth/me` | любой вошедший | Текущий пользователь: id, имя, email, роли, studentId / teacherId |

Пример входа:
```json
POST /api/auth/login
{ "userName": "alex", "password": "Student123!" }
```
Токен передаётся в заголовке `Authorization: Bearer <accessToken>`. В Swagger — кнопка **Authorize**, вставить только сам токен.

### JWT

- Claims: `sub` (id пользователя), `unique_name` (логин), `role` (роли), `student_id` / `teacher_id` (связь с сущностью), `jti`.
- Подпись HMAC-SHA256, проверяются подпись, `Issuer`, `Audience`, срок действия (`ClockSkew = 0`).
- Срок жизни — `Jwt:AccessTokenMinutes` (15 минут).
- Просроченный токен → 401 `TOKEN_EXPIRED`, изменённый или некорректный → 401 `UNAUTHORIZED`.
- Ключ не хранится в коде и в Git (user-secrets / `Jwt__Key`).

### Identity

- `IdentityDbContext<ApplicationUser>` — таблицы `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` и др. в той же БД.
- `ApplicationUser` расширен полями `StudentId` и `TeacherId` (уникальные, FK на Students / Teachers).
- Пароли хешируются средствами Identity (`PasswordHasher`), собственного хеширования нет.
- Требования к паролю: от 8 символов, цифра, строчная и заглавная буква.
- Имя пользователя и email уникальны → 409 `USER_ALREADY_EXISTS`.
- Блокировка: 5 неудачных попыток → учётная запись заблокирована на 5 минут (401 `ACCOUNT_LOCKED`). Разблокировать досрочно может Admin.
- `UserManager` — создание пользователей, роли, поиск; `SignInManager.CheckPasswordSignInAsync` — проверка пароля с учётом блокировки.

### Роли и политики

| Роль | Полномочия |
|---|---|
| Admin | Управление пользователями и ролями, полный доступ к данным |
| Teacher | Просмотр курсов и студентов, изменение оценок **только по своим курсам** |
| Student | Просмотр курсов, **только своих** записей и оценок |

Политики (`Program.cs`): `AdminOnly` (Admin), `Staff` (Admin, Teacher), `CanGrade` (Admin, Teacher).

### Управление пользователями (только Admin)

| Метод | URL | Описание |
|---|---|---|
| GET | `/api/users` | Список пользователей (без хешей паролей, security stamp и т.п.) |
| GET | `/api/users/{id}` | Пользователь по id |
| POST | `/api/users` | Создать пользователя с ролью (`UserCreateDto`) |
| PUT | `/api/users/{id}/role` | Назначить роль (`{ "role": "Teacher", "teacherId": 2 }`) |
| POST | `/api/users/{id}/unlock` | Снять блокировку |

Роль `Teacher` требует `teacherId`, роль `Student` — `studentId`; одну сущность нельзя связать с двумя учётными записями. Admin не может менять роль самому себе.

**Защита от повышения привилегий:** регистрация всегда даёт роль Student и создаёт новую запись Student (нельзя «привязаться» к чужому студенту); поля роли в `RegisterDto` нет; все операции с ролями доступны только политике `AdminOnly`.

### Матрица доступа

| Endpoint | Аноним | Student | Teacher | Admin |
|---|---|---|---|---|
| `GET /api/courses`, `/api/v1/courses`, `/api/v2/courses` (+ `/{id}`) | ✓ | ✓ | ✓ | ✓ |
| `POST/PUT/DELETE /api/courses` | 401 | 403 | 403 | ✓ |
| `GET /api/teachers` (+ `/{id}`) | ✓ | ✓ | ✓ | ✓ |
| `POST/PUT/DELETE /api/teachers` | 401 | 403 | 403 | ✓ |
| `GET /api/students` | 401 | 403 | ✓ | ✓ |
| `GET /api/students/{id}` | 401 | только свой | ✓ | ✓ |
| `GET /api/students/{id}/courses` | 401 | только свой | только оценки по своим курсам | ✓ |
| `POST/PUT/DELETE /api/students` | 401 | 403 | 403 | ✓ |
| `GET /api/enrollments` | 401 | только свои | по своим курсам | все |
| `GET /api/enrollments/{id}` | 401 | только своя | по своему курсу | ✓ |
| `POST/DELETE /api/enrollments` | 401 | 403 | 403 | ✓ |
| `PUT /api/enrollments/{id}` (оценка) | 401 | 403 | только свой курс | ✓ |
| `/api/users/*` | 401 | 403 | 403 | ✓ |

### Почему одной проверки роли недостаточно

Атрибут `[Authorize(Policy = CanGrade)]` пропускает к `PUT /api/enrollments/{id}` **любого** преподавателя. Если остановиться на этом, Иванов сможет поставить оценку на курсе Петровой. Поэтому сервис дополнительно сравнивает `Course.TeacherId` записи с `teacher_id` из токена (`EnrollmentService.IsOwnCourse`), а для студентов — `StudentId` записи с `student_id` (`StudentService.IsOtherStudent`, `EnrollmentService.CanRead`). Роль отвечает на вопрос «какого типа действие можно выполнять», проверка принадлежности — «можно ли выполнять его **над этим объектом**». Проверка выполняется на сервере: клиент лишь не показывает лишние кнопки, но API всё равно вернёт 403.

## Схема прохождения запроса

```
Клиент (UniversityClient / Swagger / Postman)
  │  PUT /api/enrollments/3   Authorization: Bearer eyJ...
  ▼
ExceptionHandlingMiddleware      — перехват непредвиденных ошибок → 500 ReturnResult
  ▼
CORS middleware                  — политика UniversityClients (localhost:5200, localhost:7200)
  ▼
Authentication middleware        — JwtBearer: подпись, Issuer, Audience, срок
  │   ├─ токена нет / он неверный → HttpContext.User анонимный
  │   └─ токен верный → ClaimsPrincipal { sub, unique_name, role=Teacher, teacher_id=1 }
  ▼
Authorization middleware         — [Authorize(Policy = CanGrade)] → RequireRole(Admin, Teacher)
  │   ├─ пользователь анонимный → 401 (JwtBearerEvents.OnChallenge, формат ReturnResult)
  │   └─ роль не подходит → 403 (JwtBearerEvents.OnForbidden)
  ▼
EnrollmentsController            — тонкий контроллер, вызывает сервис
  ▼
EnrollmentService                — проверка владения: Course.TeacherId == teacher_id (ICurrentUser)
  │   └─ чужой курс → ReturnResult 403 FORBIDDEN
  ▼
Repository → EF Core → PostgreSQL
```

**Как связаны части:** Identity хранит пользователей, хеши паролей и роли в БД и проверяет пароль при входе. `TokenService` превращает пользователя и его роли в **claims** и упаковывает их в подписанный JWT. При каждом следующем запросе **Authentication middleware** проверяет JWT и восстанавливает из claims `HttpContext.User` — к БД Identity для этого обращаться не нужно. **Authorization middleware** сравнивает claim `role` с атрибутами `[Authorize]` и политиками. Проверку владения делает уже бизнес-логика по claims `student_id` / `teacher_id`. Порядок в `Program.cs`: `UseCors` → `UseAuthentication` → `UseAuthorization` → `MapControllers`.

## Версии API

Версия задаётся сегментом URL (`UrlSegmentApiVersionReader`), в ответе есть заголовок `api-supported-versions`.

| | v1 | v2 |
|---|---|---|
| Маршруты | `/api/v1/courses`, `/api/v1/courses/{id}` и прежние `/api/courses` | `/api/v2/courses`, `/api/v2/courses/{id}` |
| DTO | `CourseDto` (как в задании 1) | `CourseV2Dto`: `teacher { id, fullName, department, email }`, `enrolledStudentsCount` |
| Контроллер | `Controllers/V1/CoursesController` (чтение + CRUD) | `Controllers/V2/CoursesController` (чтение) |

Старые клиенты продолжают работать по `/api/courses` и получают прежний контракт — версия 1.0 подставляется по умолчанию (`AssumeDefaultVersionWhenUnspecified`). Бизнес-логика не дублируется: оба контроллера вызывают одни и те же `CourseService.GetAllAsync<TDto>` / `GetByIdAsync<TDto>`, отличается только DTO, в который AutoMapper отображает сущность. Endpoints `auth` и `users` — `[ApiVersionNeutral]` и показаны в обоих документах Swagger.

## CORS

Именованная политика `UniversityClients`, адреса берутся из `Cors:AllowedOrigins` в `appsettings.json`: `http://localhost:5200` и `https://localhost:7200` (клиентское приложение). Запросы с других Origin не получают заголовок `Access-Control-Allow-Origin`.

## Клиентское приложение UniversityClient

Отдельный проект Razor Pages, к базе данных не обращается — только HTTP-запросы к API.

- Адрес API — `UniversityApi:BaseUrl` в `UniversityClient/appsettings.json`.
- Типизированный `HttpClient` (`Services/UniversityApiClient.cs`), регистрируется через `AddHttpClient<UniversityApiClient>` (`IHttpClientFactory`).
- Страницы: список курсов с переключателем v1 / v2, карточка курса, вход, «Мои записи» (`/api/auth/me` + `/api/enrollments` с JWT).
- JWT хранится в серверной сессии (cookie HttpOnly), в браузер не отдаётся.
- Ошибки: API выключен → «API недоступен», таймаут → «API не ответил вовремя», неуспешный код → сообщение из `ReturnResult.errorMessage`; при 401 сессия очищается и открывается страница входа.

## Комплексные сценарии безопасности

Все сценарии проверяются скриптом (API должен быть запущен):
```bash
python tests/security_scenarios.py
```

| № | Сценарий | Запрос | Результат |
|---|---|---|---|
| 1 | Аноним запрашивает курсы | `GET /api/courses` | 200 |
| 2 | Аноним создаёт курс | `POST /api/courses` | 401 `UNAUTHORIZED` |
| 3 | Неправильный пароль | `POST /api/auth/login` | 401 `INVALID_CREDENTIALS`, токена нет |
| 4 | Правильный пароль | `POST /api/auth/login` | 200, `accessToken` |
| 5 | Просроченный JWT | любой защищённый endpoint | 401 `TOKEN_EXPIRED` |
| 6 | Student удаляет курс | `DELETE /api/courses/1` (alex) | без токена — 401, с токеном студента — **403** `FORBIDDEN` |
| 7 | Teacher меняет оценку на своём курсе | `PUT /api/enrollments/{id}` (ivanov, Web Services Development) | 200 |
| 8 | Teacher меняет оценку на чужом курсе | `PUT /api/enrollments/{id}` (ivanov, Calculus) | 403 `FORBIDDEN` |
| 9 | Student смотрит чужие оценки | `GET /api/students/2/courses` (alex) | 403 `FORBIDDEN` |
| 10 | Admin назначает роль | `PUT /api/users/{id}/role` (admin) | 200, роль изменена |
| 11 | Student назначает себе Admin | `PUT /api/users/{id}/role` (alex) | 403 `FORBIDDEN` |
| 12 | Клиент обращается к v1 и v2 | `GET /api/v1/courses/1`, `GET /api/v2/courses/1` | обе версии 200, у каждой свой контракт |

> В задании для сценария 6 указан 401. Аутентифицированный студент получает 403: он известен системе, но прав не хватает. 401 возвращается, если тот же запрос отправлен без токена или с недействительным токеном.

Для ручной проверки просроченного токена в Swagger/Postman можно временно поставить `"AccessTokenMinutes": 1`, войти и подождать минуту.

## Архитектура

```
Client (UniversityClient / Swagger / Postman)
  ↓ HTTP (JSON) + Authorization: Bearer
Middleware            — ExceptionHandling → CORS → Authentication → Authorization
  ↓
Controller            — Auth, Users, Students, Teachers, V1/Courses, V2/Courses, Enrollments
  ↓ DTO
Service               — бизнес-правила, проверка владения (ICurrentUser), ReturnResult<T>
  ↓
Repository / UserManager
  ↓
Entity Framework Core — ApplicationDbContext : IdentityDbContext<ApplicationUser>
  ↓
Database              — PostgreSQL
```

## Структура проекта

```
UniversityApi
├── Controllers          BaseApiController, Auth, Users, Students, Teachers, Enrollments
│   ├── V1               CoursesController (v1 + прежние маршруты /api/courses)
│   └── V2               CoursesController (расширенный контракт)
├── Models               Student, Teacher, Course, Enrollment, ApplicationUser
├── DTO                  Students, Teachers, Courses (+ CourseV2Dto), Enrollments, Auth, Users
├── Data                 ApplicationDbContext, DbSeeder, IdentitySeeder, Migrations
├── Repositories         Interfaces / Implementations
├── Services             Interfaces / Implementations (+ AuthService, UserService, TokenService)
├── Security             Roles, Policies, CustomClaims, JwtOptions, CurrentUser
├── Swagger              ConfigureSwaggerOptions (документ на каждую версию + JWT)
├── Mapping              MappingProfile
├── Middleware           ExceptionHandlingMiddleware
├── Common               ReturnResult<T>
└── Program.cs
UniversityClient         Razor Pages клиент (Pages, Services/UniversityApiClient, Models)
tests/security_scenarios.py — проверка сценариев раздела 7
database/university_db.sql  — дамп БД с тестовыми данными, ролями и пользователями
```

## Endpoints (данные)

### Students

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/students` | Список студентов (Admin, Teacher) | 200, 401, 403 |
| GET | `/api/students/{id}` | Студент по ID | 200, 401, 403, 404 |
| GET | `/api/students/{id}/courses` | Студент с курсами и оценками | 200, 401, 403, 404 |
| POST | `/api/students` | Создать студента (Admin) | 201, 400, 401, 403, 409 |
| PUT | `/api/students/{id}` | Изменить студента (Admin) | 200, 400, 401, 403, 404, 409 |
| DELETE | `/api/students/{id}` | Удалить студента (Admin) | 200, 401, 403, 404 |

### Teachers

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/teachers` | Список преподавателей | 200 |
| GET | `/api/teachers/{id}` | Преподаватель по ID | 200, 404 |
| POST | `/api/teachers` | Создать преподавателя (Admin) | 201, 400, 401, 403, 409 |
| PUT | `/api/teachers/{id}` | Изменить преподавателя (Admin) | 200, 400, 401, 403, 404, 409 |
| DELETE | `/api/teachers/{id}` | Удалить преподавателя (Admin) | 200, 401, 403, 404, 409 |

### Courses (v1; `/api/courses` = `/api/v1/courses`)

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/v1/courses?name=&teacherId=&minCredits=&maxCredits=` | Список курсов с фильтрацией | 200 |
| GET | `/api/v1/courses/{id}` | Курс по ID | 200, 404 |
| POST | `/api/v1/courses` | Создать курс (Admin) | 201, 400, 401, 403, 404 |
| PUT | `/api/v1/courses/{id}` | Изменить курс (Admin) | 200, 400, 401, 403, 404 |
| DELETE | `/api/v1/courses/{id}` | Удалить курс (Admin) | 200, 401, 403, 404 |

### Courses v2

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/v2/courses?name=&teacherId=&minCredits=&maxCredits=` | Курсы с преподавателем и числом студентов | 200 |
| GET | `/api/v2/courses/{id}` | Курс по ID | 200, 404 |

### Enrollments

| Метод | URL | Описание | Коды ответа |
|---|---|---|---|
| GET | `/api/enrollments` | Записи (фильтруются по роли) | 200, 401 |
| GET | `/api/enrollments/{id}` | Запись по ID | 200, 401, 403, 404 |
| POST | `/api/enrollments` | Записать студента на курс (Admin) | 201, 400, 401, 403, 404, 409 |
| PUT | `/api/enrollments/{id}` | Изменить оценку (Admin, Teacher своего курса) | 200, 400, 401, 403, 404 |
| DELETE | `/api/enrollments/{id}` | Удалить студента с курса (Admin) | 200, 401, 403, 404 |

Любой endpoint при непредвиденной ошибке возвращает 500.

## Формат ответа

Все endpoints, включая 401 и 403 от middleware, возвращают `ReturnResult<T>`:

```json
{
  "statusCode": 403,
  "isSuccess": false,
  "result": null,
  "errorCode": "FORBIDDEN",
  "errorMessage": "You can change grades only on your own courses",
  "traceId": "0HNP65IR8I56Q:00000001"
}
```

Ошибки валидации DTO — 400 `VALIDATION_ERROR`. Непредвиденные ошибки — 500 `INTERNAL_SERVER_ERROR` без StackTrace и SQL.

## Бизнес-правила

| Правило | Ответ |
|---|---|
| Нельзя записать студента на несуществующий курс | 404 `COURSE_NOT_FOUND` |
| Нельзя записать несуществующего студента на курс | 404 `STUDENT_NOT_FOUND` |
| Студент не может быть дважды записан на один курс | 409 `ALREADY_ENROLLED` |
| Курс не может ссылаться на несуществующего преподавателя | 404 `TEACHER_NOT_FOUND` |
| Email студента / преподавателя уникален | 409 `STUDENT_EMAIL_EXISTS` / `TEACHER_EMAIL_EXISTS` |
| Нельзя удалить преподавателя, у которого есть курсы | 409 `TEACHER_HAS_COURSES` |
| Имя пользователя уникально | 409 `USER_ALREADY_EXISTS` |
| Teacher меняет оценки только на своих курсах | 403 `FORBIDDEN` |
| Student видит только свои записи и оценки | 403 `FORBIDDEN` |

## Логирование

- **Information** — создание, изменение, удаление объектов; вход пользователя; смена роли.
- **Warning** — несуществующий объект, нарушение бизнес-правил, неудачный вход, отказ в доступе к чужим данным.
- **Error** — непредвиденные ошибки (`ExceptionHandlingMiddleware`).

В логи не пишутся пароли, JWT и ключ подписи: в сообщениях только id пользователя, логин и коды ошибок Identity.

## Dependency Injection

Репозитории, сервисы, `ICurrentUser`, `ITokenService` — **Scoped** (зависят от `DbContext` / текущего запроса). `UserManager`, `SignInManager`, `RoleManager` регистрирует Identity. `UniversityApiClient` в клиенте — typed client из `IHttpClientFactory`.
