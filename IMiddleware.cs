using System;

public interface IMiddleware
{
    void Execute(IPipelineContext context, Action next);
}