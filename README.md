# CLO Employee Emergency Contact System API

CLO Virtual Fashion 직원 긴급 연락망 관리 시스템 Backend API

## 기술 스택

- **.NET 8** (ASP.NET Core Web API)
- **CQRS 패턴** (MediatR)
- **EF Core InMemory** Database
- **Swagger/OpenAPI** (Swashbuckle)
- **xUnit** + Moq + FluentAssertions (테스트)

## 사전 요구사항

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 이상

## 빌드 및 실행

```bash
# 빌드
dotnet build

# 테스트
dotnet test

# 실행
dotnet run --project src/CLOContactSystem.Api
```

실행 후 Swagger UI: http://localhost:5039/swagger

## API 응답 형식

모든 API는 아래와 같은 통일된 형식으로 응답합니다.

```json
{
  "success": true,
  "message": null,
  "code": 200,
  "data": { ... }
}
```

| 필드 | 타입 | 설명 |
|------|------|------|
| success | bool | 요청 성공 여부 |
| message | string? | 실패 시 에러 메시지 |
| code | int | HTTP 상태 코드 |
| data | object? | 응답 데이터 (실패 시 null) |

## API Endpoints

### GET /api/employee?page={page}&pageSize={pageSize}

직원 목록을 페이징하여 조회합니다.

```bash
curl http://localhost:5039/api/employee?page=1&pageSize=10
```

**Response (200 OK)**
```json
{
  "success": true,
  "message": null,
  "code": 200,
  "data": {
    "items": [
      {
        "name": "김철수",
        "email": "charles@clovf.com",
        "phoneNumber": "01075312468",
        "joinedDate": "2018-03-07"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

### GET /api/employee/{name}

이름으로 직원 정보를 조회합니다.

```bash
curl http://localhost:5039/api/employee/%EA%B9%80%EC%B2%A0%EC%88%98
```

**Response (200 OK)**
```json
{
  "success": true,
  "message": null,
  "code": 200,
  "data": {
    "name": "김철수",
    "email": "charles@clovf.com",
    "phoneNumber": "01075312468",
    "joinedDate": "2018-03-07"
  }
}
```

**Response (404 Not Found)**
```json
{
  "success": false,
  "message": "Employee '홍길동' not found.",
  "code": 404,
  "data": null
}
```

### POST /api/employee

직원 연락 정보를 등록합니다. 아래 입력 방식을 지원합니다.

#### 1. JSON Body

```bash
curl -X POST http://localhost:5039/api/employee \
  -H "Content-Type: application/json" \
  -d '[
    {"name":"김클로","email":"clo@clovf.com","tel":"010-1111-2424","joined":"2012-01-05"},
    {"name":"박마블","email":"md@clovf.com","tel":"010-3535-7979","joined":"2013-07-01"}
  ]'
```

#### 2. CSV Body

```bash
curl -X POST http://localhost:5039/api/employee \
  -H "Content-Type: text/csv" \
  -d '김철수, charles@clovf.com, 01075312468, 2018.03.07
박영희, matilda@clovf.com, 01087654321, 2021.04.28'
```

#### 3. JSON 파일 업로드

```bash
curl -X POST http://localhost:5039/api/employee \
  -F "file=@employees.json"
```

#### 4. CSV 파일 업로드

```bash
curl -X POST http://localhost:5039/api/employee \
  -F "file=@employees.csv"
```

#### 5. Form 필드 텍스트 입력

```bash
# CSV (필드명: csv)
curl -X POST http://localhost:5039/api/employee \
  -F "csv=홍길동, kildong.hong@clovf.com, 01012345678, 2015.08.15"

# JSON (필드명: json)
curl -X POST http://localhost:5039/api/employee \
  -F 'json=[{"name":"홍커넥","email":"connect@clovf.com","tel":"010-8531-7942","joined":"2019-12-05"}]'
```

**Response (201 Created)**
```json
{
  "success": true,
  "message": null,
  "code": 201,
  "data": [
    {
      "name": "김클로",
      "email": "clo@clovf.com",
      "phoneNumber": "01011112424",
      "joinedDate": "2012-01-05"
    }
  ]
}
```

**Response (400 Bad Request)**
```json
{
  "success": false,
  "message": "Validation failed.",
  "code": 400,
  "data": null
}
```

## 프로젝트 구조

```
CLOContactSystem/
├── src/CLOContactSystem.Api/
│   ├── Controllers/          # API 엔드포인트
│   ├── Domain/               # 엔티티, 인터페이스
│   ├── Application/          # CQRS Commands, Queries, DTOs, Validators
│   └── Infrastructure/       # DB Context, Parsers (CSV/JSON)
└── tests/CLOContactSystem.Tests/
    ├── Parsing/              # Parser 단위 테스트
    ├── Validators/           # Validator 단위 테스트
    ├── Handlers/             # CQRS Handler 단위 테스트
    └── Integration/          # API 통합 테스트
```

## 설계 패턴

- **CQRS**: MediatR로 Command(쓰기)와 Query(읽기)를 분리
- **Repository Pattern**: DB 접근을 인터페이스로 추상화
- **Parser Strategy**: CSV/JSON 파싱 로직을 분리하여 포맷 확장이 쉬운 구조

## 테스트

총 55개 테스트 (단위 + 통합)

```bash
dotnet test --verbosity normal
```
