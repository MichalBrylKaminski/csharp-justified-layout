using JustifiedLayout.Helpers;
using JustifiedLayout.Models;

namespace JustifiedLayout.Services;

public interface IJustifiedLayoutService
{
    Layout ComputeLayout(LayoutConfig layoutConfig, LayoutData layoutData, List<Item> itemLayoutData);
}

public class JustifiedLayoutService : IJustifiedLayoutService
{
    /// <summary>
    /// Calculate the current layout for all items in the list that require layout.
    /// <c>Layout</c> means geometry: position within container and size.
    /// </summary>
    /// <param name="layoutConfig">The layout configuration.</param>
    /// <param name="layoutData">The current state of the layout.</param>
    /// <param name="itemLayoutData">List of items to lay out, with data required to lay out each item.</param>
    /// <returns>The newly-calculated layout, containing the new container height, and lists of layout items.</returns>
    public Layout ComputeLayout(LayoutConfig layoutConfig, LayoutData layoutData, List<Item> itemLayoutData)
    {
        return JustifiedLayoutHelper.ComputeLayout(layoutConfig, layoutData, itemLayoutData);
    }
}
