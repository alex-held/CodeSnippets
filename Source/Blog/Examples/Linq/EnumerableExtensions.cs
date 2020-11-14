﻿namespace Examples.Linq
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class EnumerableExtensions
    {
        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> asyncAction)
        {
            foreach (T value in source)
            {
                await asyncAction(value);
            }
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, Task> asyncAction)
        {
            int index = 0;
            foreach (T value in source)
            {
                await asyncAction(value, checked(++index));
            }
        }

        public static async Task ParallelForEachAsync<T>(
            this IEnumerable<T> source, Func<T, Task> asyncAction, int? maxDegreeOfParallelism = null)
        {
            maxDegreeOfParallelism ??= Environment.ProcessorCount;
            OrderablePartitioner<T> partitioner = source is IList<T> list
                ? Partitioner.Create(list, loadBalance: true)
                : Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering);
            await Task.WhenAll(partitioner
                .GetPartitions(maxDegreeOfParallelism.Value)
                .Select(partition => Task.Run(async () =>
                {
                    while (partition.MoveNext())
                    {
                        await asyncAction(partition.Current);
                    }
                })));
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source) where T : class
        {
            return source.Where(value => !(value is null))!; // Equivalent to: source.Where(value => !(value is null)).Select(value => value!).
        }

        public static ParallelQuery<T> NotNull<T>(this ParallelQuery<T?> source) where T : class
        {
            return source.Where(value => !(value is null))!; // Equivalent to: source.Where(value => !(value is null)).Select(value => value!).
        }
    }
}