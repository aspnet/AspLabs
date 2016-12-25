namespace GenericReceivers.Dependencies
{
    /// <summary>
    /// Sample custom dependency used for constructor injection for <see cref="GenericJsonWebHookHandler"/>
    /// </summary>
    public interface IMyDependency
    {
        void DoIt();
    }
}