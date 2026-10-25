# CRUD Kronix for Devs — Backend

API REST de cadastro de medicamentos, feita para ensinar novos desenvolvedores do Kronix os padrões do time. Tem login com JWT e um CRUD completo de medicamentos protegido por token. Os dados ficam em memória, então somem quando a aplicação para.

## Sumário

- [Stack](#stack)
- [Passo a passo para startar a API](#passo-a-passo-para-startar-a-api)
- [Configurando o User Secrets](#configurando-o-user-secrets)
- [Arquitetura](#arquitetura)
- [Autenticação](#autenticação)
- [Endpoints](#endpoints)
- [Regras de negócio e validações](#regras-de-negócio-e-validações)
- [Tratamento de erros](#tratamento-de-erros)
- [Testes](#testes)

## Stack

| Item | Tecnologia |
| --- | --- |
| Runtime | .NET 10 (`net10.0`) |
| Web | ASP.NET Core com Controllers |
| Autenticação | JWT Bearer (HS256), `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.13 |
| Hash de senha | `PasswordHasher` do ASP.NET Core Identity (PBKDF2 com salt) |
| Documentação da API | OpenAPI nativo + Scalar 2.18.2 em `/scalar` |
| Persistência | Repositórios em memória (`ConcurrentDictionary`) |
| Testes | xUnit 2.9.3, `Microsoft.AspNetCore.Mvc.Testing`, coverlet |

## Passo a passo para startar a API

**Pré-requisitos:** SDK do .NET 10 e, se preferir, Visual Studio 2022 (versão com suporte a .NET 10).

### 1. Clone o repositório na branch `develop`

A `develop` é a branch base de desenvolvimento.

```bash
git clone -b develop https://github.com/gabriel-fancio-ems/crud-kronix-for-devs-backend.git
cd crud-kronix-for-devs-backend
```

Para trabalhar em algo novo, crie sua branch a partir da `develop`:

```bash
git checkout -b feature/minha-tarefa
```

### 2. Configure o User Secrets

A API só sobe com o usuário administrador e a chave JWT configurados. Sem eles, a aplicação para na inicialização com uma mensagem dizendo o que falta. Siga [Configurando o User Secrets](#configurando-o-user-secrets).

### 3. Rode a API

Pelo terminal:

```bash
cd CrudKronixForDevs/CrudKronixForDevs.Api
dotnet run
```

Ou pelo Visual Studio: abra `CrudKronixForDevs/CrudKronixForDevs.slnx`, defina **CrudKronixForDevs.Api** como projeto de inicialização, escolha o perfil **http** e pressione **F5**.

A API escuta em `http://localhost:5203`. No console aparece `Usuário carregado: <seu e-mail>` quando o administrador foi criado.

### 4. Abra a documentação interativa

Acesse `http://localhost:5203/scalar`. O documento OpenAPI fica em `/openapi/v1.json`. Os dois só existem em `Development`, que é o ambiente padrão do perfil `http`.

### 5. Faça login e teste um endpoint

1. No Scalar, chame `POST /api/auth/login` com o e-mail e a senha que você colocou no User Secrets.
2. Copie o `accessToken` da resposta.
3. Cole no campo de autenticação **Bearer** do Scalar, sem a palavra "Bearer".
4. Chame `GET /api/auth/me` para confirmar que o token funciona.
5. Pronto: os endpoints de `/api/medication` já estão liberados.

## Configurando o User Secrets

O User Secrets guarda as configurações sensíveis num `secrets.json` fora da pasta do projeto, então elas nunca vão para o Git. O `UserSecretsId` já está no `.csproj` da Api, por isso não é preciso rodar `dotnet user-secrets init`. O arquivo deve ficar neste padrão:

```json
{
  "User": {
    "Email": "seu.email@ems.com.br",
    "Name": "Seu Nome",
    "Password": "SuaSenha@123"
  },
  "Jwt": {
    "Key": "<chave-aleatoria-com-32-caracteres-ou-mais>"
  }
}
```

O e-mail e a senha de `User` são os que você vai usar no login, e a `Jwt:Key` precisa ter pelo menos 32 caracteres.

### Opção 1: pelo Visual Studio (Manage User Secrets)

1. Abra a solução `CrudKronixForDevs.slnx` no Visual Studio.
2. No **Solution Explorer**, clique com o botão direito no projeto **CrudKronixForDevs.Api** (não na solução nem no Core).
3. Escolha **Manage User Secrets**. O Visual Studio abre o `secrets.json`, vazio (`{ }`) na primeira vez.
4. Apague o conteúdo, cole o JSON acima com os seus dados e salve com **Ctrl+S**.

### Opção 2: por comando (dotnet CLI)

Entre na pasta do projeto Api:

```bash
cd CrudKronixForDevs/CrudKronixForDevs.Api
```

Defina cada chave. O `:` separa a seção da propriedade, igual à estrutura do JSON:

```bash
dotnet user-secrets set "User:Email" "seu.email@ems.com.br"
dotnet user-secrets set "User:Name" "Seu Nome"
dotnet user-secrets set "User:Password" "SuaSenha@123"
dotnet user-secrets set "Jwt:Key" "<chave-aleatoria-com-32-caracteres-ou-mais>"
```

Ou importe tudo de uma vez a partir de um arquivo com o JSON acima (apague o arquivo depois, para não versioná-lo):

```bash
# PowerShell ou CMD (Windows)
type .\meus-segredos.json | dotnet user-secrets set

# macOS / Linux
cat ./meus-segredos.json | dotnet user-secrets set
```

Confira o que ficou salvo:

```bash
dotnet user-secrets list
```

A saída deve listar `Jwt:Key`, `User:Email`, `User:Name` e `User:Password`. Para começar do zero, use `dotnet user-secrets clear`.

### Onde o arquivo fica

As duas opções gravam o mesmo arquivo:

| Sistema | Caminho |
| --- | --- |
| Windows | `%APPDATA%\Microsoft\UserSecrets\c9acfe1c-325e-4edd-b720-68e3597bb8ec\secrets.json` |
| macOS / Linux | `~/.microsoft/usersecrets/c9acfe1c-325e-4edd-b720-68e3597bb8ec/secrets.json` |

### Outras configurações

O User Secrets só é lido em `Development`. Em outros ambientes (Docker, servidor), use variáveis de ambiente com `__` no lugar de `:`, por exemplo `Jwt__Key` e `User__Email`.

| Configuração | Onde | Padrão |
| --- | --- | --- |
| `Jwt:Key` | User Secrets ou `Jwt__Key` | nenhum, obrigatório (mínimo 32 caracteres) |
| `Jwt:Issuer` | `appsettings.json` | `CrudKronixForDevs` |
| `Jwt:Audience` | `appsettings.json` | `CrudKronixForDevs` |
| `Jwt:ExpirationMinutes` | `appsettings.json` | `60` |
| `User:Email`, `User:Name`, `User:Password` | User Secrets ou `User__Email` etc. | nenhum, obrigatórios |

## Arquitetura

A solução (`CrudKronixForDevs.slnx`) tem três projetos: a **Api** recebe HTTP e cuida de infraestrutura web, o **Core** guarda as regras de negócio e não depende do ASP.NET, e o **Tests** cobre os dois. O Core só conhece interfaces (`IPasswordService`, `ITokenService`, repositórios); as implementações concretas são registradas pela Api.

```
CrudKronixForDevs/
├── CrudKronixForDevs.Api/      # Controllers, validação, JWT, middleware, DI
├── CrudKronixForDevs.Core/     # Casos de uso, domínio, DTOs, repositórios em memória
└── CrudKronixForDevs.Tests/    # Testes unitários e de integração
```

| Pasta | Projeto | Responsabilidade |
| --- | --- | --- |
| `Api/Controllers` | Api | `AuthController`, `MedicationController` e `OperationCenterController`: recebem a requisição, validam o formato e delegam ao Core |
| `Api/Validation` | Api | Validadores manuais (`IValidator<T>`) que devolvem `campo → erros` |
| `Api/Authorization` | Api | Geração de JWT, hash de senha, opções do JWT e esquema Bearer no OpenAPI |
| `Api/Middlewares` | Api | `ExceptionHandlingMiddleware`: converte exceções em ProblemDetails |
| `Api/DependencyInjection` | Api | Registro de serviços e criação do usuário administrador na subida |
| `Core/Applications` | Core | Casos de uso: `MedicationApplication`, `OperationCenterApplication` e `AuthApplication` |
| `Core/Domains` | Core | Entidades `Medication`, `OperationCenter` e `User`, enum `OperationCenterStatus`, exceções de domínio e interfaces de repositório |
| `Core/Dtos` | Core | Contratos de entrada e saída (`MedicationRequest`, `MedicationResponse`, `OperationCenterRequest`, `OperationCenterResponse`, `LoginRequest`, `LoginResponse`) |
| `Core/Services` | Core | Repositórios em memória de medicamentos, centros de operação e usuários |
| `Core/Extensions` | Core | Mapeamento entidade → DTO e normalização de texto para busca |
| `Core/Resources` | Core | Mensagens exibidas ao usuário, centralizadas |
| `Core/Configuration` | Core | `UserOptions`: dados do usuário administrador |
| `Tests/Unit`, `Tests/Integration` | Tests | Testes unitários por camada e testes de ponta a ponta |

**Caminho de uma requisição:** Controller → Validator → Application (caso de uso) → Domínio → Repositório → DTO de resposta.

**Tempo de vida das dependências:** repositórios, validadores, `PasswordService`, `JwtTokenService` e `TimeProvider` são Singleton (os repositórios precisam sobreviver entre requisições). `MedicationApplication`, `OperationCenterApplication` e `AuthApplication` são Scoped.

**Pipeline HTTP, na ordem:** `ExceptionHandlingMiddleware` → OpenAPI e Scalar (só em Development) → `UseAuthentication` → `UseAuthorization` → Controllers.

## Autenticação

Existe um único usuário, o administrador criado na subida da API a partir da seção `User`. Ele faz login com e-mail e senha e recebe um JWT válido por 60 minutos. Todo endpoint de medicamentos e de centros de operação exige esse token.

1. Na subida, `AdminUserAsync` lê `UserOptions`, gera o hash da senha com `PasswordService` e grava o usuário no repositório em memória. O e-mail é guardado em minúsculas.
2. `POST /api/auth/login` valida o formato, busca o usuário por e-mail (sem diferenciar maiúsculas) e confere a senha contra o hash.
3. E-mail inexistente e senha errada recebem a mesma resposta 401, para não revelar quais e-mails existem.
4. Com credenciais corretas, `JwtTokenService` assina o token com HS256 e a chave `Jwt:Key`.
5. Nas próximas chamadas, envie o cabeçalho `Authorization: Bearer <accessToken>`.

**Claims do token:** `sub` (id do usuário), `email`, `name` e `jti` (id único do token). `MapInboundClaims = false` mantém esses nomes curtos ao ler o token.

**Validação do token:** emissor, audiência, assinatura e validade são conferidos em toda requisição, com tolerância de relógio (`ClockSkew`) de 1 minuto.

## Endpoints

São doze endpoints: dois de autenticação, cinco de medicamentos e cinco de centros de operação. Todos falam JSON em camelCase; só o login é público.

| Método | Rota | Auth | Sucesso | Erros |
| --- | --- | --- | --- | --- |
| POST | `/api/auth/login` | Não | 200 `LoginResponse` | 400, 401 |
| GET | `/api/auth/me` | Sim | 200 `{ id, name, email }` | 401 |
| GET | `/api/medication?nome=<trecho>` | Sim | 200 lista de `MedicationResponse` | 401 |
| GET | `/api/medication/{id}` | Sim | 200 `MedicationResponse` | 401, 404 |
| POST | `/api/medication` | Sim | 201 `MedicationResponse` + cabeçalho `Location` | 400, 401, 409 |
| PUT | `/api/medication/{id}` | Sim | 200 `MedicationResponse` | 400, 401, 404, 409 |
| DELETE | `/api/medication/{id}` | Sim | 204 sem corpo | 401, 404 |
| GET | `/api/operation-center?descricao=<trecho>` | Sim | 200 lista de `OperationCenterResponse` | 401 |
| GET | `/api/operation-center/{id}` | Sim | 200 `OperationCenterResponse` | 401, 404 |
| POST | `/api/operation-center` | Sim | 201 `OperationCenterResponse` + cabeçalho `Location` | 400, 401, 409 |
| PUT | `/api/operation-center/{id}` | Sim | 200 `OperationCenterResponse` | 400, 401, 404, 409 |
| DELETE | `/api/operation-center/{id}` | Sim | 204 sem corpo | 401, 404, 409 |

`{id}` é um GUID. A listagem de medicamentos filtra por trecho do nome **ou** do princípio ativo, sem diferenciar acentos e maiúsculas, e ordena por nome. A de centros de operação filtra por trecho da descrição e ordena por descrição.

### Login

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "seu.email@ems.com.br", "password": "SuaSenha@123" }
```

```json
{
  "accessToken": "eyJhbGciOi...",
  "tokenType": "Bearer",
  "expiresAt": "2026-10-21T15:34:00Z",
  "name": "Seu Nome",
  "email": "seu.email@ems.com.br"
}
```

### Medicamento: entrada (POST e PUT)

```json
{
  "name": "Dipirona Monoidratada",
  "activeIngredient": "Dipirona",
  "price": 12.90,
  "quantityInStock": 150,
  "expirationDate": "2027-10-20",
  "operationCenterId": "9b2f1c3e-0d4a-4f6b-8e7a-1c2d3e4f5a6b"
}
```

### Medicamento: saída

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Dipirona Monoidratada",
  "activeIngredient": "Dipirona",
  "price": 12.90,
  "stockQuantity": 150,
  "expirationDate": "2027-10-20",
  "operationCenterId": "9b2f1c3e-0d4a-4f6b-8e7a-1c2d3e4f5a6b",
  "createdAt": "2026-10-21T11:34:00"
}
```

Todo medicamento pertence a um centro de operação: `operationCenterId` é obrigatório e precisa apontar para um centro já cadastrado.

### Centro de operação: entrada (POST e PUT)

```json
{
  "description": "Centro de Distribuição São Paulo",
  "status": "Active"
}
```

`status` aceita `Active` ou `Inactive`.

### Centro de operação: saída

```json
{
  "id": "9b2f1c3e-0d4a-4f6b-8e7a-1c2d3e4f5a6b",
  "description": "Centro de Distribuição São Paulo",
  "status": "Active",
  "createdAt": "2026-10-21T11:34:00"
}
```

O estoque se chama `quantityInStock` na entrada e `stockQuantity` na saída. O PUT substitui todos os campos; não há atualização parcial.

## Regras de negócio e validações

A validação acontece em duas camadas: o validador da Api devolve 400 com todos os erros de formato de uma vez, e a entidade `Medication` revalida as regras para nunca ficar em estado inválido. O nome duplicado é checado no caso de uso, porque precisa consultar os outros registros.

| Campo | Validador da Api (400) | Entidade / caso de uso |
| --- | --- | --- |
| `name` | obrigatório, até 150 caracteres | obrigatório; espaços nas pontas removidos; único (409) |
| `activeIngredient` | obrigatório, até 150 caracteres | obrigatório; espaços nas pontas removidos |
| `price` | maior que 0 e no máximo 100.000,00 | maior que 0 |
| `quantityInStock` | não negativo | não negativo |
| `expirationDate` | obrigatório | posterior à data de hoje (400 via `DomainException`) |
| `operationCenterId` | obrigatório (não pode ser GUID vazio) | o centro precisa existir (400 via `DomainException`) |
| `description` (centro) | obrigatório, até 150 caracteres | obrigatório; espaços nas pontas removidos; única (409) |
| `status` (centro) | obrigatório, `Active` ou `Inactive` | precisa ser um valor válido do enum |
| `email` (login) | obrigatório e com `@` | guardado em minúsculas |
| `password` (login) | obrigatório | conferido contra o hash |

**Nome único:** dois medicamentos não podem ter o mesmo nome, comparado sem acentos e sem maiúsculas ("Dipirona" e "DIPIRONA" colidem). No PUT, o próprio registro é ignorado na checagem. A mesma regra vale para a descrição dos centros de operação.

**Centro em uso:** um centro de operação com medicamentos vinculados não pode ser removido (409).

**Data de referência:** "hoje" vem do `TimeProvider` em horário local, o que permite congelar a data nos testes.

**Mensagens:** todos os textos exibidos ao usuário ficam em `MedicationMessages`, `OperationCenterMessages` e `AuthMessages`, em `Core/Resources`.

## Tratamento de erros

Toda resposta de erro segue o formato ProblemDetails (RFC 9457), com `Content-Type: application/problem+json`. Os controllers não têm `try/catch`: o `ExceptionHandlingMiddleware`, primeiro da pipeline, captura as exceções.

| Origem | Status | `title` |
| --- | --- | --- |
| Validador da Api | 400 | erros por campo em `errors` (`ValidationProblemDetails`) |
| `DuplicateMedicationException` | 409 | Medicamento duplicado |
| `DuplicateOperationCenterException` | 409 | Centro de operação duplicado |
| `OperationCenterInUseException` | 409 | Centro de operação em uso |
| `DomainException` | 400 | Regra de negócio violada |
| Login inválido | 401 | Não autorizado |
| Token ausente, inválido ou expirado | 401 | resposta padrão do JwtBearer |
| Id inexistente | 404 | resposta padrão do ASP.NET |
| Qualquer outra exceção | 500 | Erro interno, sem detalhes para o cliente; o stack trace vai para o log |

Exemplo de 400 de validação:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "nome": ["O nome do medicamento é obrigatório."],
    "preco": ["O preço deve ser maior que zero."]
  }
}
```

As chaves de `errors` são `nome`, `principioAtivo`, `preco`, `quantidadeEstoque`, `dataValidade` e `centroOperacao` para medicamentos, e `descricao` e `status` para centros de operação.

## Testes

O projeto `CrudKronixForDevs.Tests` tem 170 métodos de teste xUnit (`[Fact]` e `[Theory]`): 135 unitários e 35 de integração. Rode tudo da pasta `CrudKronixForDevs`:

```bash
dotnet test

# com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

| Área | Arquivo | Testes |
| --- | --- | --- |
| Casos de uso | `MedicationApplicationTests` | 27 |
| Casos de uso | `OperationCenterApplicationTests` | 13 |
| Casos de uso | `AuthApplicationTests` | 7 |
| Domínio | `MedicationTests` | 19 |
| Domínio | `OperationCenterTests` | 8 |
| Domínio | `UserTests` | 5 |
| Validação | `MedicationRequestValidatorTests` | 12 |
| Validação | `OperationCenterRequestValidatorTests` | 7 |
| Validação | `LoginRequestValidatorTests` | 4 |
| Repositórios | `MedicationRepositoryInMemoryTests` | 8 |
| Repositórios | `OperationCenterRepositoryInMemoryTests` | 7 |
| Repositórios | `UserRepositoryInMemoryTests` | 4 |
| Autorização | `JwtTokenServiceTests` | 6 |
| Autorização | `PasswordServiceTests` | 4 |
| Extensões | `StringExtensionsTests`, `MedicationExtensionsTests`, `OperationCenterExtensionsTests` | 4 |
| Integração | `MedicationEndpointsTests` | 16 |
| Integração | `OperationCenterEndpointsTests` | 10 |
| Integração | `AuthEndpointsTests` | 9 |

**Integração:** `CrudKronixApiFactory` sobe a API real em memória com `WebApplicationFactory<Program>` e injeta um usuário e uma chave JWT só de teste. Por isso os testes não dependem do User Secrets de quem roda. `CreateAuthenticatedClientAsync()` devolve um `HttpClient` já com o token.

**Apoios:** `FixedTimeProvider` congela o relógio em 15/01/2026 10h UTC, `FakeTokenService` evita assinar JWT nos testes de caso de uso e `MedicationRequestBuilder` / `OperationCenterRequestBuilder` montam requisições válidas para variar um campo por vez.
