namespace Application.Committee.Commands.App.PostDayRating;

public record PostCommitteeDayRatingCommand : IRequest<Result>
{
    public int UserId { get; set; }
    public int ListId { get; set; }
    public DateOnly Date { get; set; }
    public int Score { get; set; }
    public string? Note { get; set; }
}

public record GetCommitteeMissingDayRatingsQuery(int UserId, DateOnly Date)
    : IRequest<Result<List<CommitteeMissingDayRatingDto>>>;

public record CommitteeMissingDayRatingDto
{
    public int ListId { get; set; }
    public required string ListName { get; set; }
}

public class PostCommitteeDayRatingHandler : IRequestHandler<PostCommitteeDayRatingCommand, Result>
{
    public Task<Result> Handle(PostCommitteeDayRatingCommand request, CancellationToken cancellationToken)
    {
        if (request.Score is < 0 or > 10)
            return Task.FromResult(Result.Failed(new Error("التقييم من 0 إلى 10", "DayRating_Invalid")));
        return Task.FromResult(Result.Success());
    }
}
