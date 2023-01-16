using System;
using System.Collections.Concurrent;
using System.Collections.Generic;using System.Configuration;
using System.Runtime.Remoting.Contexts;

public class Pipeline
{
    private readonly DependencyProvider _dependencyProvider;

    public Pipeline(DependencyProvider dependencyProvider)
    {
        _dependencyProvider = dependencyProvider;
    }
    private readonly Queue<Func<IMiddleware>> _queue
    = new Queue<Func<IMiddleware>>();

    public Pipeline Use(Action<IPipelineContext, Action> method)
        => Use(new SimpleMiddleware(method));

    public class SimpleMiddleware : IMiddleware
    {
        private readonly Action<IPipelineContext, Action> _method;

        public SimpleMiddleware(Action<IPipelineContext, Action> method)
        {
            _method = method;
        }

        public void Execute(IPipelineContext context, Action next)
        {
            _method(context, next);
        }
    }

    public Pipeline Use<TMiddleware>()
        where TMiddleware:IMiddleware
    {
        return Use(() => _dependencyProvider.Require<TMiddleware>());
    }
    public Pipeline Use(IMiddleware instance)
    {
        return Use(() => instance);
    }
    public Pipeline Use<TMiddleware>(Func<TMiddleware> provider)
        where TMiddleware : IMiddleware
    {
        _queue.Enqueue(()=>provider());
        return this;
    }

    public void Run(IPipelineContext context)
    {
        if (_queue.Count>0)
        {
            RunInternal(context);
        }
    }
    private void RunInternal(IPipelineContext context)
        => _queue.Dequeue()().Execute(context, () => RunInternal(context));
}
