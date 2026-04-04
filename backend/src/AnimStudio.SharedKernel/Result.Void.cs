namespace AnimStudio.SharedKernel
{
    /// <summary>Non-generic result type for operations with no return value.</summary>
    public sealed class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public string? ErrorCode { get; }

        private Result(bool isSuccess, string? error, string? errorCode)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorCode = errorCode;
        }

        public static Result Success() => new Result(true, null, null);

        public static Result Failure(string error, string? errorCode = null)
            => new Result(false, error, errorCode);
    }
}
