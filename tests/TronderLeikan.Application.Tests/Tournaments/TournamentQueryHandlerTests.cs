using TronderLeikan.Application.Tournaments.Queries.GetTournaments;
using TronderLeikan.Application.Tournaments.Queries.GetTournamentBySlug;
using TronderLeikan.Application.Tournaments.Queries.GetScoreboard;
using TronderLeikan.Domain.Games;
using TronderLeikan.Domain.Persons;
using TronderLeikan.Domain.Tournaments;

namespace TronderLeikan.Application.Tests.Tournaments;

public sealed class TournamentQueryHandlerTests
{
    [Fact]
    public async Task GetTournaments_ReturnererAlleTurneringer()
    {
        await using var db = TestAppDbContext.Create();
        db.Tournaments.Add(Tournament.Create("NM 2026", "nm-2026"));
        await db.SaveChangesAsync();
        var result = await new GetTournamentsQueryHandler(db).Handle(new GetTournamentsQuery());
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetTournamentBySlug_FinnerTurnering()
    {
        await using var db = TestAppDbContext.Create();
        db.Tournaments.Add(Tournament.Create("NM 2026", "nm-2026"));
        await db.SaveChangesAsync();
        var result = await new GetTournamentBySlugQueryHandler(db).Handle(new GetTournamentBySlugQuery("nm-2026"));
        Assert.True(result.IsSuccess);
        Assert.Equal("NM 2026", result.Value!.Name);
    }

    [Fact]
    public async Task GetScoreboard_BeregnerPoengRiktig()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        var personOla = Person.Create("Ola", "Nordmann");
        var personKari = Person.Create("Kari", "Traa");
        db.Persons.AddRange(personOla, personKari);
        // Ola 1. plass, Kari 2. plass — begge deltakere
        var game = Game.Create("Spill 1", tournament.Id);
        game.AddParticipant(personOla.Id);
        game.AddParticipant(personKari.Id);
        game.Complete([personOla.Id], [personKari.Id], []);
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var result = await new GetScoreboardQueryHandler(db).Handle(new GetScoreboardQuery(tournament.Id));

        // Ola: participation(3) + firstPlace(3) = 6, Kari: participation(3) + secondPlace(2) = 5
        Assert.True(result.IsSuccess);
        var ola = result.Value!.Single(e => e.PersonId == personOla.Id);
        var kari = result.Value!.Single(e => e.PersonId == personKari.Id);
        Assert.Equal(6, ola.TotalPoints);
        Assert.Equal(5, kari.TotalPoints);
        Assert.Equal(1, ola.Rank);
        Assert.Equal(2, kari.Rank);
    }

    [Fact]
    public async Task GetScoreboard_ArrangørerFårPoengEtterEgenRolle()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        var personMari = Person.Create("Mari", "Spiller");
        var personTor = Person.Create("Tor", "Dommer");
        db.Persons.AddRange(personMari, personTor);
        // Mari arrangerte og spilte, Tor arrangerte og dømte
        var game = Game.Create("Spill 1", tournament.Id);
        game.AddOrganizer(personMari.Id, withParticipation: true);
        game.AddOrganizer(personTor.Id, withParticipation: false);
        game.Complete([], [], []);
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var result = await new GetScoreboardQueryHandler(db).Handle(new GetScoreboardQuery(tournament.Id));

        // Mari: participation(3) + organizedWithParticipation(1) = 4, Tor: organizedWithoutParticipation(3) = 3
        Assert.True(result.IsSuccess);
        var mari = result.Value!.Single(e => e.PersonId == personMari.Id);
        var tor = result.Value!.Single(e => e.PersonId == personTor.Id);
        Assert.Equal(4, mari.TotalPoints);
        Assert.Equal(3, tor.TotalPoints);
    }

    [Fact]
    public async Task GetScoreboard_DeltakerSomOgsåErSpillendeArrangør_FårDeltakerpoengÉnGang()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        var personMari = Person.Create("Mari", "Spiller");
        db.Persons.Add(personMari);
        // Mari er registrert både som deltaker og som arrangør som spilte
        var game = Game.Create("Spill 1", tournament.Id);
        game.AddParticipant(personMari.Id);
        game.AddOrganizer(personMari.Id, withParticipation: true);
        game.Complete([], [], []);
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var result = await new GetScoreboardQueryHandler(db).Handle(new GetScoreboardQuery(tournament.Id));

        // Mari: participation(3) én gang + organizedWithParticipation(1) = 4
        Assert.True(result.IsSuccess);
        var mari = result.Value!.Single(e => e.PersonId == personMari.Id);
        Assert.Equal(4, mari.TotalPoints);
    }

    [Fact]
    public async Task GetScoreboard_ArrangørLagtTilPåNyttMedAnnenRolle_SisteRolleGjelder()
    {
        await using var db = TestAppDbContext.Create();
        var tournament = Tournament.Create("NM", "nm");
        db.Tournaments.Add(tournament);
        var personTor = Person.Create("Tor", "Dommer");
        db.Persons.Add(personTor);
        // Tor ble først feilregistrert som spillende arrangør, så rettet til dømmende
        var game = Game.Create("Spill 1", tournament.Id);
        game.AddOrganizer(personTor.Id, withParticipation: true);
        game.AddOrganizer(personTor.Id, withParticipation: false);
        game.Complete([], [], []);
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var result = await new GetScoreboardQueryHandler(db).Handle(new GetScoreboardQuery(tournament.Id));

        // Tor: organizedWithoutParticipation(3) = 3
        Assert.True(result.IsSuccess);
        var tor = result.Value!.Single(e => e.PersonId == personTor.Id);
        Assert.Equal(3, tor.TotalPoints);
    }
}
