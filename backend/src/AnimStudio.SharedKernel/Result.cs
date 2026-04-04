using System.Text.Json.Serialization;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Non-generic marker interface for <see cref="Result{T}"/> so pipeline behaviours
    /// can inspect success/failure without binding to the type parameter.
    /// </summary>
    public interface IResult
    {
        bool IsSuccess { get; }
    }

    /// <summary>
    /// Represents the outcome of an operation.
    /// </summary>
    /// <typeparam name="T">The type of the value associated with the result.</typeparam>
    public class Result<T> : IResult
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets the value associated with a successful result.
        /// </summary>
        public T? Value { get; private set; }

        /// <summary>
        /// Gets the error message if the operation failed.
        /// </summary>
        public string? Error { get; private set; }

        /// <summary>
        /// Gets an optional error code for the result.
        /// </summary>
        public string? ErrorCode { get; private set; }
        [JsonConstructor]
        private Result(bool isSuccess, T? value, string? error, string? errorCode)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Factory method to create a successful result.
        /// </summary>
        /// <param name="value">The value associated with the success.</param>
        /// <returns>A success result.</returns>
        public static Result<T> Success(T value) => new Result<T>(true, value, null, null);

        /// <summary>
        /// Factory method to create a failure result.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <param name="errorCode">The optional error code.</param>
        /// <returns>A failure result.</returns>
        public static Result<T> Failure(string error, string? errorCode = null) => new Result<T>(false, default, error, errorCode);
    }
}