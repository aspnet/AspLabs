namespace Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;

// The grid cascades this so that descendant columns can talk back to it. It's an internal type
// so that it doesn't show up by mistake in unrelated components.
internal class InternalGridContext<TGridItem>
{
    public QuickGrid<TGridItem> Grid { get; }

    public InternalGridContext(QuickGrid<TGridItem> grid)
    {
        Grid = grid;
    }
}
