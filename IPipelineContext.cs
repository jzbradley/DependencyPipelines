public interface IPipelineContext
{
    IPipelineContext Parent { get; }
    DependencyProvider DependencyProvider { get; }
}