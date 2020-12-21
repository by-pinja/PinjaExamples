namespace Pinja.Examples.ConventionBasedFactories
{
    /// <summary>
    /// Defines an interface for a common dependency service for the message handlers.
    /// </summary>
    public interface IExampleDependency
    {
        /// <summary>
        /// Gets some resource for the message handlers.
        /// </summary>
        /// <returns></returns>
        IExampleResource GetResource();
    }
}
