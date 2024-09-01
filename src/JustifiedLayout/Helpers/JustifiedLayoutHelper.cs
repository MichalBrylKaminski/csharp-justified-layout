using JustifiedLayout.Models;

namespace JustifiedLayout.Helpers;

public static class JustifiedLayoutHelper
{
    /// <summary>
    /// Calculate the current layout for all items in the list that require layout.
    /// <c>Layout</c> means geometry: position within container and size.
    /// </summary>
    /// <param name="layoutConfig">The layout configuration.</param>
    /// <param name="layoutData">The current state of the layout.</param>
    /// <param name="itemLayoutData">List of items to lay out, with data required to lay out each item.</param>
    /// <returns>The newly-calculated layout, containing the new container height, and lists of layout items.</returns>
    public static Layout ComputeLayout(LayoutConfig layoutConfig, LayoutData layoutData, List<Item> itemLayoutData)
    {
        Row currentRow = null;
        layoutData.ContainerHeight += layoutConfig.ContainerPadding.Top;
        
        // Apply forced aspect ratio if specified, and set a flag.
        ForceAspectRatio(layoutConfig, itemLayoutData);

        for (var i = 0; i < itemLayoutData.Count; i++)
        {
            var itemData = itemLayoutData[i];

            if (itemData.AspectRatio <= 0)
            {
                throw new ArgumentException($"Item {i} has an invalid aspect ratio");
            }
            
            currentRow ??= CreateNewRow(layoutConfig, layoutData);

            var itemAdded = currentRow.TryAddItem(itemData);

            if (!currentRow.IsLayoutComplete()) continue;

            AddRow(layoutConfig, layoutData, currentRow);
            
            if (layoutData.Rows.Count >= layoutConfig.MaxNumRows)
            {
                currentRow = null;
                break;
            }

            currentRow = CreateNewRow(layoutConfig, layoutData);

            // Item was rejected; add it to its own row
            if (itemAdded) continue;
                
            currentRow.TryAddItem(itemData);

            if (!currentRow.IsLayoutComplete()) continue;

            AddRow(layoutConfig, layoutData, currentRow);
            
            if (layoutData.Rows.Count >= layoutConfig.MaxNumRows)
            {
                currentRow = null;
                break;
            }

            currentRow = CreateNewRow(layoutConfig, layoutData);
        }

        // Handle any leftover content
        if (currentRow is not null && currentRow.Items.Count > 0 && layoutConfig.ShowWidows)
        {
            // Last page of all content or orphan suppression is suppressed; lay out orphans.
            if (layoutData.Rows.Count > 0)
            {
                // Only Match previous row's height if it exists and it isn't a breakout row
                var nextToLastRowHeight = layoutData.Rows[^1].IsBreakoutRow
                    ? layoutData.Rows[^1].TargetRowHeight
                    : layoutData.Rows[^1].Height;

                currentRow.ForceComplete(nextToLastRowHeight);
            }
            else
            {
                // ...else use target height if there is no other row height to reference.
                currentRow.ForceComplete();
            }

            AddRow(layoutConfig, layoutData, currentRow);
            layoutConfig.WidowCount = currentRow.Items.Count;
        }

        // We need to clean up the bottom container padding
        // First remove the height added for box spacing
        layoutData.ContainerHeight -= layoutConfig.BoxSpacing.Vertical;
        // Then add our bottom container padding
        layoutData.ContainerHeight += layoutConfig.ContainerPadding.Bottom;

        return new Layout
        {
            ContainerHeight = layoutData.ContainerHeight,
            WidowCount = layoutConfig.WidowCount,
            Boxes = layoutData.LayoutItems
        };
    }

    /// <summary>
    /// Enforce aspect ratio on all items in layout if required.
    /// </summary>
    /// <param name="layoutConfig">The layout configuration.</param>
    /// <param name="itemLayoutData">List of items</param>
    private static void ForceAspectRatio(LayoutConfig layoutConfig, List<Item> itemLayoutData)
    {
        if (!layoutConfig.ForceAspectRatio) return;
        
        foreach (var itemData in itemLayoutData)
        {
            itemData.ForcedAspectRatio = true;
            itemData.AspectRatio = layoutConfig.ForceAspectRatio ? layoutConfig.TargetRowHeight / layoutConfig.TargetRowHeight : itemData.AspectRatio;
        }
    }

    /// <summary>
    /// Create a new <c>Row</c>.
    /// </summary>
    /// <param name="layoutConfig">The layout configuration.</param>
    /// <param name="layoutData">The current state of the layout.</param>
    /// <returns>A new, empty row of the type specified by this layout.</returns>
    private static Row CreateNewRow(LayoutConfig layoutConfig, LayoutData layoutData)
    {
        // Work out if this is a full width breakout row
        var isBreakoutRow = layoutConfig.FullWidthBreakoutRowCadence.HasValue 
                            && (layoutData.Rows.Count + 1) % layoutConfig.FullWidthBreakoutRowCadence.Value == 0;

        return new Row(new RowParams
        {
            Top = layoutData.ContainerHeight,
            Left = layoutConfig.ContainerPadding.Left,
            Width = layoutConfig.ContainerWidth - layoutConfig.ContainerPadding.Left - layoutConfig.ContainerPadding.Right,
            Spacing = layoutConfig.BoxSpacing.Horizontal,
            TargetRowHeight = layoutConfig.TargetRowHeight,
            TargetRowHeightTolerance = layoutConfig.TargetRowHeightTolerance,
            EdgeCaseMinRowHeight = 0.5 * layoutConfig.TargetRowHeight,
            EdgeCaseMaxRowHeight = 2 * layoutConfig.TargetRowHeight,
            IsBreakoutRow = isBreakoutRow,
            WidowLayoutStyle = layoutConfig.WidowLayoutStyle
        });
    }

    /// <summary>
    /// Add a completed <c>Row</c> to the layout.
    /// <para>Note: the row must have already been completed.</para>
    /// </summary>
    /// <param name="layoutConfig">The layout configuration.</param>
    /// <param name="layoutData">The current state of the layout.</param>
    /// <param name="row">The <c>Row</c> to add.</param>
    /// <seealso cref="Row"/>
    private static void AddRow(LayoutConfig layoutConfig, LayoutData layoutData, Row row)
    {
        layoutData.Rows.Add(row);
        layoutData.LayoutItems.AddRange(row.Items);
        layoutData.ContainerHeight += row.Height + layoutConfig.BoxSpacing.Vertical;
    }
}
