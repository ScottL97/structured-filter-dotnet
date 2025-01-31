using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StructuredFilter.Utils;

public class LazyObjectGetException : Exception;

public class LazyObjectGetter<T>(
    LazyObjectGetter<T, string, object>.ObjectGetter objectGetter,
    Dictionary<string, object>? args)
    : LazyObjectGetter<T, string, object>(objectGetter, args);

public class LazyObjectGetter<T, TArgKey, TArgValue> where TArgKey : notnull
{
    public delegate Task<(T, bool)> ObjectGetter(Dictionary<TArgKey, TArgValue>? args);
    private static readonly LazyObjectGetException ObjectGetFailedException = new ();
    private readonly Lazy<Task<T>> _lazyObject;
    public readonly Dictionary<TArgKey, TArgValue>? Args;

    public LazyObjectGetter(ObjectGetter objectGetter, Dictionary<TArgKey, TArgValue>? args)
    {
        Args = args;
        _lazyObject = new Lazy<Task<T>>(async () =>
        {
            var (obj, isExists) = await objectGetter(args);
            if (!isExists || obj == null)
            {
                throw ObjectGetFailedException;
            }

            return obj;
        });
    }

    public async Task<T> GetAsync()
    {
        return await _lazyObject.Value;
    }
}
