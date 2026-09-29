using TronderLeikan.Domain.Games;

namespace TronderLeikan.Application.Games.Responses;

public record GameDetailResponse(
    Guid Id,
    Guid TournamentId,
    string Name,
    string? Description,
    bool IsDone,
    GameType GameType,
    bool HasBanner,
    IReadOnlyList<Guid> Participants,
    IReadOnlyList<Guid> Organizers,
    IReadOnlyList<Guid> PlayingOrganizers,
    IReadOnlyList<Guid> Spectators,
    IReadOnlyList<Guid> FirstPlace,
    IReadOnlyList<Guid> SecondPlace,
    IReadOnlyList<Guid> ThirdPlace);
