using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LogMergeRx
{
    public static class Result
    {
        public static Result<T> Failure<T>(params string[] errors) =>
            new Result<T>.Failure(ImmutableArray.Create(errors));

        public static Result<T> Failure<T>(ImmutableArray<string> errors) =>
            new Result<T>.Failure(errors);

        public static Result<T> Success<T>(T value) =>
            new Result<T>.Success(value);

        public static Result<IEnumerable<T>> SelectMany<T>(this IEnumerable<Result<T>> collection)
        {
            var groups = collection.ToLookup(x => x.IsFailure);
            return groups[true].Any() // IsFailure==true
                ? Failure<IEnumerable<T>>(groups[true].OfType<Result<T>.Failure>().SelectMany(f => f.Errors).ToImmutableArray())
                : Success<IEnumerable<T>>(groups[false].OfType<Result<T>.Success>().Select(s => s.Value).ToList());
        }
    }

    public class Result<T>
    {
        [DebuggerStepThrough]
        public void Match(Action<T> onSuccess, Action<ImmutableArray<string>> onFailure)
        {
            if (this is Success success)
            {
                onSuccess(success.Value);
            }
            else if (this is Failure failure)
            {
                onFailure(failure.Errors);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        [DebuggerStepThrough]
        public TOut Select<TOut>(Func<T, TOut> onSuccess, Func<ImmutableArray<string>, TOut> onFailure) =>
            this switch
            {
                Success success => onSuccess(success.Value),
                Failure failure => onFailure(failure.Errors),
                _ => throw new NotSupportedException(),
            };

        public T ValueOrThrow() =>
            Select(value => value, errors => throw new InvalidOperationException());

        public bool IsFailure =>
            this is Failure;

        [DebuggerStepThrough]
        public Task<TOut> SelectAsync<TOut>(Func<T, Task<TOut>> onSuccess, Func<ImmutableArray<string>, TOut> onFailure) =>
            this switch
            {
                Success success => onSuccess(success.Value),
                Failure failure => Task.FromResult(onFailure(failure.Errors)),
                _ => throw new NotSupportedException(),
            };

        public sealed class Success : Result<T>
        {
            public T Value { get; }

            public Success(T value)
            {
                Value = value;
            }
        }

        public sealed class Failure : Result<T>
        {
            public ImmutableArray<string> Errors { get; }

            public Failure(ImmutableArray<string> errors)
            {
                Errors = errors;
            }
        }
    }
}
