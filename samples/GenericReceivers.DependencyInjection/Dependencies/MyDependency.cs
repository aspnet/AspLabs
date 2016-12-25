namespace GenericReceivers.Dependencies
{
    /// <summary>
    /// Sample custom dependency used for constructor injection for <see cref="GenericJsonWebHookHandler"/>
    /// </summary>
    public class MyDependency : IMyDependency
    {
        public void DoIt()
        {
        }
    }
}