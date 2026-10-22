"""Checks the security scenarios from section 7 against a running API.

Usage: python tests/security_scenarios.py [base_url]
An expired token for scenario 5 is signed with the key from user-secrets (Jwt:Key).
"""
import base64
import hashlib
import hmac
import json
import subprocess
import sys
import time
import urllib.error
import urllib.request

BASE = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5175"
results = []


def call(method, path, body=None, token=None):
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(BASE + path, data=data, method=method)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", f"Bearer {token}")
    try:
        with urllib.request.urlopen(req) as r:
            return r.status, json.loads(r.read() or b"null")
    except urllib.error.HTTPError as e:
        raw = e.read()
        return e.code, json.loads(raw) if raw else None


def login(user, password):
    status, body = call("POST", "/api/auth/login", {"userName": user, "password": password})
    return status, body, (body or {}).get("result") and body["result"]["accessToken"]


def check(no, name, ok, detail):
    results.append(ok)
    print(f"{'PASS' if ok else 'FAIL'}  {no:>2}. {name}: {detail}")


def b64(data):
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode()


def expired_token(user_id):
    secrets = subprocess.run(["dotnet", "user-secrets", "list", "--project", "UniversityApi"],
                             capture_output=True, text=True).stdout
    key = next(line.split(" = ", 1)[1] for line in secrets.splitlines() if line.startswith("Jwt:Key"))
    now = int(time.time())
    header = b64(json.dumps({"alg": "HS256", "typ": "JWT"}).encode())
    payload = b64(json.dumps({"sub": user_id, "unique_name": "admin", "role": "Admin", "iss": "UniversityApi",
                              "aud": "UniversityClients", "iat": now - 3600, "nbf": now - 3600, "exp": now - 60}).encode())
    signature = b64(hmac.new(key.encode(), f"{header}.{payload}".encode(), hashlib.sha256).digest())
    return f"{header}.{payload}.{signature}"


s, b = call("GET", "/api/courses")
check(1, "Anonymous GET /api/courses", s == 200, f"{s}, {len(b['result'])} courses")

s, b = call("POST", "/api/courses", {"name": "Hack", "credits": 3, "teacherId": 1})
check(2, "Anonymous POST /api/courses", s == 401, f"{s} {b and b['errorCode']}")

s, b, t = login("alex", "WrongPassword1")
check(3, "Wrong password", s == 401 and not t, f"{s} {b['errorCode']}, token issued: {bool(t)}")

_, _, admin = login("admin", "Admin123!")
_, _, ivanov = login("ivanov", "Teacher123!")
_, _, alex = login("alex", "Student123!")
check(4, "Correct password", bool(admin and ivanov and alex), "JWT issued for admin, ivanov, alex")

_, me = call("GET", "/api/auth/me", token=admin)
s, b = call("GET", "/api/students", token=expired_token(me["result"]["id"]))
check(5, "Expired JWT", s == 401, f"{s} {b['errorCode']}")

s, b = call("DELETE", "/api/courses/1", token=alex)
check(6, "Student DELETE /api/courses/1", s == 403, f"{s} {b['errorCode']} (authenticated but not allowed -> 403)")

_, enrollments = call("GET", "/api/enrollments", token=admin)
by_course = {e["courseName"]: e for e in enrollments["result"]}
own = by_course["Web Services Development"]  # Ivanov's course
foreign = by_course["Calculus"]              # Petrova's course

s, b = call("PUT", f"/api/enrollments/{own['id']}", {"grade": 95}, token=ivanov)
check(7, "Teacher grades own course", s == 200, f"{s}, grade={b['result']['grade']}")

s, b = call("PUT", f"/api/enrollments/{foreign['id']}", {"grade": 10}, token=ivanov)
check(8, "Teacher grades foreign course", s == 403, f"{s} {b['errorCode']}")

_, users = call("GET", "/api/users", token=admin)
alex_user = next(u for u in users["result"] if u["userName"] == "alex")
maria_user = next(u for u in users["result"] if u["userName"] == "maria")

s, b = call("GET", f"/api/students/{maria_user['studentId']}/courses", token=alex)
check(9, "Student reads another student's grades", s == 403, f"{s} {b['errorCode']}")
s, b = call("PUT", f"/api/users/{maria_user['id']}/role",
            {"role": "Student", "studentId": maria_user["studentId"]}, token=admin)
check(10, "Admin sets role", s == 200, f"{s}, roles={b['result']['roles']}")

s, b = call("PUT", f"/api/users/{alex_user['id']}/role", {"role": "Admin"}, token=alex)
check(11, "Student makes self Admin", s == 403, f"{s} {b['errorCode']}")

s1, v1 = call("GET", "/api/v1/courses/1")
s2, v2 = call("GET", "/api/v2/courses/1")
ok = s1 == s2 == 200 and "teacherName" in v1["result"] and "teacher" in v2["result"] and "enrolledStudentsCount" in v2["result"]
check(12, "v1 and v2", ok, f"v1 keys={sorted(v1['result'])} | v2 keys={sorted(v2['result'])}")

print(f"\n{sum(results)}/{len(results)} passed")
sys.exit(0 if all(results) else 1)
