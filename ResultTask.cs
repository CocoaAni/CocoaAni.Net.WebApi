namespace CocoaAni.Net.WebApi;

public class ResultTask<T> : ResultTask<T, T>
{
    public ResultTask(Func<object?, Result<T, T>> function, object? state) : base(function, state)
    {
    }

    public ResultTask(Func<object?, Result<T, T>> function, object? state, CancellationToken cancellationToken) : base(function, state, cancellationToken)
    {
    }

    public ResultTask(Func<object?, Result<T, T>> function, object? state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(function, state, cancellationToken, creationOptions)
    {
    }

    public ResultTask(Func<object?, Result<T, T>> function, object? state, TaskCreationOptions creationOptions) : base(function, state, creationOptions)
    {
    }

    public ResultTask(Func<Result<T, T>> function) : base(function)
    {
    }

    public ResultTask(Func<Result<T, T>> function, CancellationToken cancellationToken) : base(function, cancellationToken)
    {
    }

    public ResultTask(Func<Result<T, T>> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(function, cancellationToken, creationOptions)
    {
    }

    public ResultTask(Func<Result<T, T>> function, TaskCreationOptions creationOptions) : base(function, creationOptions)
    {
    }
}

public class ResultTask<TV, TE> : Task<Result<TV, TE>>
{
    public ResultTask(Func<object?, Result<TV, TE>> function, object? state) : base(function, state)
    {
    }

    public ResultTask(Func<object?, Result<TV, TE>> function, object? state, CancellationToken cancellationToken) : base(function, state, cancellationToken)
    {
    }

    public ResultTask(Func<object?, Result<TV, TE>> function, object? state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(function, state, cancellationToken, creationOptions)
    {
    }

    public ResultTask(Func<object?, Result<TV, TE>> function, object? state, TaskCreationOptions creationOptions) : base(function, state, creationOptions)
    {
    }

    public ResultTask(Func<Result<TV, TE>> function) : base(function)
    {
    }

    public ResultTask(Func<Result<TV, TE>> function, CancellationToken cancellationToken) : base(function, cancellationToken)
    {
    }

    public ResultTask(Func<Result<TV, TE>> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(function, cancellationToken, creationOptions)
    {
    }

    public ResultTask(Func<Result<TV, TE>> function, TaskCreationOptions creationOptions) : base(function, creationOptions)
    {
    }
}