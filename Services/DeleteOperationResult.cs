namespace DatabaseMastery.HotCoffeePostgreSQL.Services
{
    public enum DeleteOperationStatus
    {
        Deleted,
        NotFound,
        BlockedByDependencies
    }

    public sealed class DeleteOperationResult
    {
        public DeleteOperationStatus Status { get; init; }

        public static DeleteOperationResult Deleted { get; } =
            new DeleteOperationResult { Status = DeleteOperationStatus.Deleted };

        public static DeleteOperationResult NotFound { get; } =
            new DeleteOperationResult { Status = DeleteOperationStatus.NotFound };

        public static DeleteOperationResult BlockedByDependencies { get; } =
            new DeleteOperationResult { Status = DeleteOperationStatus.BlockedByDependencies };
    }
}
