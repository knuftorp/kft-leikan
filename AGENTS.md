# AGENTS.md

Trønder Leikan: turnerings- og poengsystem for uformelle konkurranser.
Poengregler og domene: `docs/TRONDER_LEIKAN.md`.
Kommentarer, feilmeldinger og tekster i UI skrives på norsk.

## Kjøre og teste

- Hele stacken: `dotnet run --project src/TronderLeikan.AppHost`. Frontend kjøres via AppHost, ikke alene, ellers peker `API_BASE_URL` til feil sted.
- Kjør aldri AppHost fra en worktree. Zitadel-porten og Postgres-volumet er delt, og `zitadel-bootstrap/` finnes bare i hovedklonen.
- Ikke kjør `reset-local` uten å spørre. Det sletter databasevolumet.
- Innlogging på `/admin`: `zitadel-admin@zitadel.localhost` / `Password1!`.
- Oppstartsfeil: se «Feilsøking» i `README.md`.
- `Api.Tests` og `Infrastructure.Tests` bruker Testcontainers og trenger Docker.
- Frontend-lint kjøres med `--max-warnings 0` i CI. En advarsel knekker bygget.
- Migrasjoner: `dotnet tool restore`, så `dotnet ef migrations add <Navn> --project src/TronderLeikan.Infrastructure`. De kjøres av DbMigrator ved oppstart, ikke av API-et.

## Backend

- Egen mediator (`ISender`, `Application/Common/Sender.cs`), ikke MediatR. Handlers og validators registreres automatisk med Scrutor.
- Ny use case: `Application/<Aggregat>/{Commands,Queries}/<UseCase>/` med command/query-record, handler og eventuelt validator. Respons-DTO legges i `<Aggregat>/Responses/`.
- Handlers returnerer `Result`/`Result<T>` og kaster ikke. Feil defineres i `Application/Common/Errors/<Aggregat>Errors.cs`, og controllere svarer med `.Match(Ok, Problem)`.
- Application bruker `IAppDbContext`, aldri `AppDbContext`.
- Domenehendelser (`AddDomainEvent`) skrives til outbox og event store i `AppDbContext.SaveChangesAsync`. Hver hendelse må ha en `Guid`-egenskap som slutter på `Id`, ellers feiler lagringen. Ingenting konsumerer outboxen ennå, så event handlers er placeholdere.
- Scoreboard-poeng lagres ikke. De regnes ut ved spørring i `GetScoreboardQueryHandler`.

## Tester

- xUnit + AwesomeAssertions (ikke FluentAssertions).
- `Application.Tests` bruker `TestAppDbContext` (EF InMemory) med egen, håndskrevet modellkonfigurasjon. Nye entiteter, owned types og backing field-lister må legges inn der også, ikke bare i `Infrastructure/Persistence/Configurations`.

## Frontend

- Next.js 16: `src/proxy.ts` erstatter `middleware.ts` og beskytter `/admin/*`.
