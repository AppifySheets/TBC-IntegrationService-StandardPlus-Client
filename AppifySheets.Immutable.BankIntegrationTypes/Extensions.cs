using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace AppifySheets.Immutable.BankIntegrationTypes;

public static class Extensions
{
    [DebuggerHidden, DebuggerStepThrough]
    public static IReadOnlyCollection<T> AsReadOnlyList<T>(this IEnumerable<T> enumerable)
        => enumerable is IReadOnlyCollection<T> listT
            ? listT
            : enumerable.ToList().AsReadOnly();
    
    public static TOutput Use<TInput, TOutput>(this TInput o, Func<TInput, TOutput> selector) =>
        selector != null ? selector(o) : throw new ArgumentNullException(nameof(selector));
    
    public static DateTime ToDateTime(this DateOnly dateOnly) => dateOnly.ToDateTime(TimeOnly.MinValue);
        
    
    public static async Task< IReadOnlyCollection<T>> WhenAll<T>(this IEnumerable<Task<T>> tasks)
    {
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.AsReadOnlyList();
    }
    
    public static async Task<TOutput> UseAsync<TInput, TOutput>(this Task<TInput> input, Func<TInput, TOutput> func)
    {
        var inputAwaited = await input;

        return func(inputAwaited);
    }
    
    public static async Task<Result<TOutput>> MapAsync<TInput, TOutput>(this Task<Result<TInput>> input, Func<TInput, TOutput> func)
    {
        var inputAwaited = await input;

        return inputAwaited.Map(func);
    }
    public static async Task<Result<TOutput>> MapAsync<TInput, TOutput>(this Task<Result<TInput>> input, Func<TInput, Task<TOutput>> func)
    {
        var inputAwaited = await input;

        return await inputAwaited.Map(func);
    }
}