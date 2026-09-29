# Brief: 5. Dommer eller spiller?

Story: [docs/backlog.md](../backlog.md#5-dommer-eller-spiller)

**Etterpå:** Hver arrangør har sin egen rolle, «spilte» eller «dømte». Scoreboardet gir hver arrangør poeng ut fra egen rolle: deltakerpoeng pluss arrangørtillegg hvis hen spilte, og det høyere arrangørtillegget uten deltakerpoeng hvis hen bare dømte. Et spill kan ha én arrangør av hver type. I admin kan man legge til en arrangør med rolle, og spillsidene viser rollen ved hver arrangør.

**Ikke rør:** API-kontrakten for `AddOrganizer` (`WithParticipation` per person finnes allerede), poengreglene i `TournamentPointRules`, og hvordan tilskuere og plasseringer regnes ut.

**Vi vet at det virker når:**
- en test i `Application.Tests` viser at et spill med én spillende og én dømmende arrangør gir henholdsvis 3+1 og 3 poeng med standardreglene
- en test viser at en person som står både i `Participants` og som spillende arrangør får deltakerpoeng én gang
- en test viser at `AddOrganizer` med ny rolle for samme person endrer rollen
- eksisterende scoreboard-tester fortsatt er grønne, og demodataene gir samme poeng som før
- frontend-lint er grønn med `--max-warnings 0`

## Funn i koden

- API-et tar allerede imot rollen per arrangør. `AddOrganizerCommand(GameId, PersonId, WithParticipation)` får den riktige informasjonen, men `Game.AddOrganizer` lagrer den som ett flagg for hele spillet (`src/TronderLeikan.Domain/Games/Game.cs`). Feilen ligger derfor i domenet og i lagringen, ikke i API-et.
- Demodataene har bare én arrangør per spill (`DemoDataSeeder.cs`), så de skal gi samme poeng som før.
- Skjemaet for å opprette spill i admin (`admin/tournaments/[slug]/page.tsx` og `actions.ts`) sender `isOrganizersParticipating`, men `CreateGameCommand` har ikke feltet. Avkrysningsboksen gjør ingenting.
- Den offentlige spillsiden og admin-siden for spill viser «Arrangør deltar» for hele spillet ut fra `isOrganizersParticipating`.
- Admin har en action for å legge til deltakere (`admin/tournaments/[slug]/games/[gameId]/actions.ts`), men ingen for arrangører.

## Beslutninger

1. **Lagring:** ny liste `_playingOrganizers` (uuid[]-kolonne, samme `UuidArray`-mønster som de andre listene i `GameConfiguration`) ved siden av `_organizers`. En arrangør som står i begge listene spilte, en som bare står i `_organizers` dømte. Lista må også inn i `TestAppDbContext`.
2. **Eksisterende spill:** migrasjonen fyller `PlayingOrganizers` med alle arrangørene i spill der `IsOrganizersParticipating` var true, og fjerner deretter kolonnen. Eksisterende spill får samme poeng som før.
3. **API-respons:** `GameDetailResponse` får `PlayingOrganizers: Guid[]`, og `IsOrganizersParticipating` fjernes. Frontend oppdateres i samme endring.
4. **Ny rolle for samme person:** siste kall til `AddOrganizer` gjelder. Da kan en feilregistrert rolle rettes ved å legge til personen på nytt.
5. **Dømmende arrangør på pallen:** utenfor scope. Ingen validering nå, og plasseringspoeng gis som i dag. Innspill til story 10.
6. **Både deltaker og spillende arrangør:** scoreboardet gir deltakerpoeng én gang per person og spill, uansett hvor personen står. Validering i domenet hører til story 10.
7. **Frontend:**
   - Spillsidene (offentlig og admin) viser «spilte» eller «dømte» ved hver arrangør i stedet for «Arrangør deltar» for hele spillet.
   - Den døde avkrysningsboksen fjernes fra skjemaet for å opprette spill.
   - Admin-siden for spill får et skjema for å legge til en arrangør med rolle, etter samme mønster som deltaker-actionen.
