using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Neat.Results
{
    public static class Result
    {
        /// <summary>
        /// Creates a failure result from errors.
        /// </summary>
        public static Result<T> Failure<T>(params string[] errors) =>
            new Result<T>.Failure(ImmutableArray.Create(errors));

        /// <summary>
        /// Creates a failure result from errors.
        /// </summary>
        public static Result<T> Failure<T>(ImmutableArray<string> errors) =>
            new Result<T>.Failure(errors);

        /// <summary>
        /// Creates a successful result from value.
        /// </summary>
        public static Result<T> Success<T>(T value) =>
            new Result<T>.Success(value);
    }

    public class Result<T>
    {
        /// <summary>
        /// When result is Success, returns its value; otherwise throws InvalidOperationException.
        /// </summary>
        public T ValueOrThrow() =>
            ValueOrThrow(() => new InvalidOperationException("Failure does not have a value"));

        /// <summary>
        /// When result is Success, returns its value; otherwise throws the provided exception.
        /// </summary>
        public T ValueOrThrow<TException>(Func<TException> factory) where TException : Exception =>
            Value(value => value, _ => throw factory.Invoke());

        /// <summary>
        /// When result is Success, returns its value; otherwise returns the provided default value.
        /// </summary>
        public T ValueOrDefault(T @default) =>
            Value(value => value, _ => @default);

        /// <summary>
        /// When the result is Failure, returns the errors; otherwise returns an empty array.
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<string> ErrorsOrEmpty() =>
            Value(value => ImmutableArray<string>.Empty, errors => errors);

        /// <summary>
        /// Converts a result to value. When the result is Success, invokes the onSuccess function;
        /// otherwise invokes the onFailure function.
        /// </summary>
        public TOut Value<TOut>(Func<T, TOut> onSuccess, Func<ImmutableArray<string>, TOut> onFailure) =>
            this switch
            {
                Success success => onSuccess(success.Value),
                Failure failure => onFailure(failure.Errors),
                _ => throw new NotSupportedException(),
            };

        /// <summary>
        /// Converts a result to another result. When the result is Success, invokes the onSuccess function;
        /// otherwise invokes the onFailure function.
        /// </summary>
        public Result<TOut> Select<TOut>(Func<T, Result<TOut>> onSuccess, Func<ImmutableArray<string>, Result<TOut>> onFailure = null) =>
            this switch
            {
                Success success => onSuccess(success.Value),
                Failure failure => onFailure != null ? onFailure(failure.Errors) : Result.Failure<TOut>(failure.Errors),
                _ => throw new NotSupportedException(),
            };

        /// <summary>
        /// Performs a side effect action on a result. When the result is Success, invokes the onSuccess action;
        /// otherwise invokes the onFailure action.
        /// </summary>
        public void Match(Action<T> onSuccess, Action<ImmutableArray<string>> onFailure)
        {
            switch (this)
            {
                case Success success: onSuccess(success.Value); break;
                case Failure failure: onFailure(failure.Errors); break;
                default: throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Converts a result to another result asynchronously. When the result is Success, invokes the onSuccess
        /// function; otherwise invokes the onFailure function.
        /// </summary>
        public Task<Result<TOut>> SelectAsync<TOut>(Func<T, Task<Result<TOut>>> onSuccess, Func<ImmutableArray<string>, Task<Result<TOut>>> onFailure = null) =>
            this switch
            {
                Success success => onSuccess(success.Value),
                Failure failure =>
                    onFailure != null
                        ? onFailure(failure.Errors)
                        : Task.FromResult(Result.Failure<TOut>(failure.Errors)),
                _ => throw new NotSupportedException(),
            };

        /// <summary>
        /// Performs a side effect action on a result asynchronously. When the result is Success, invokes the
        /// onSuccess action; otherwise invokes the onFailure action.
        /// </summary>
        public Task MatchAsync(Func<T, Task> onSuccess, Func<ImmutableArray<string>, Task> onFailure) =>
            this switch
            {
                Success success => onSuccess(success.Value),
                Failure failure => onFailure(failure.Errors),
                _ => throw new NotSupportedException(),
            };

        private Result() { }

        internal sealed class Success : Result<T>
        {
            public T Value { get; }

            public Success(T value)
            {
                Value = value;
            }
        }

        internal sealed class Failure : Result<T>
        {
            public ImmutableArray<string> Errors { get; }

            public Failure(ImmutableArray<string> errors)
            {
                Errors = errors;
            }
        }
    }
}
