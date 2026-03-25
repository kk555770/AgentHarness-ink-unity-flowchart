namespace OpsidanosInk.CanonicalGraph
{
    public interface ICurrentFlowInkCompileProbe
    {
        bool TryCompile(string inkContent, out string errorMessage);
    }
}
