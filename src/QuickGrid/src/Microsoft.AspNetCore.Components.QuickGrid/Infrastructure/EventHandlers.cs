namespace Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

/// <summary>
/// Configures event handlers for <see cref="QuickGrid{TGridItem}"/>.
/// </summary>
[EventHandler("onclosecolumnoptions", typeof(EventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers
{
}
