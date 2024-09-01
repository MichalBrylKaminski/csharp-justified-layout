namespace JustifiedLayout.Models;

public class Row
{
    private const double MinNormal = 2.2250738585072014E-308d;
    
    /// <value>
    /// Target <c>Row</c> height.
    /// </value>
    public double TargetRowHeight { get; }
    
    /// <value>
    /// Full width breakout <c>Row</c>.
    /// </value>
    public bool IsBreakoutRow { get; }
    
    /// <value>
    /// Current height of the <c>Row</c>. Defaults to <c>0</c> for empty row.
    /// </value>
    public double Height { get; private set; } = 0;
    
    /// <value>
    /// Store layout data for each <seealso cref="Item"/> in <c>Row</c>.
    /// </value>
    public List<Item> Items { get; } = new();
    
    private readonly RowParams _parameters;
    private readonly double _minAspectRatio;
    private readonly double _maxAspectRatio;
    
    public Row(RowParams parameters)
    {
        _parameters = parameters;
        
        TargetRowHeight = _parameters.TargetRowHeight;
        IsBreakoutRow = _parameters.IsBreakoutRow;
        
        _minAspectRatio = parameters.Width / TargetRowHeight * (1 - parameters.TargetRowHeightTolerance);
        _maxAspectRatio = parameters.Width / TargetRowHeight * (1 + parameters.TargetRowHeightTolerance);
    }

    /// <summary>
    /// <para>
    /// Attempts to add a single item to the row.
    /// </para>
    /// <para>
    /// If the item fits in the row, without pushing row height beyond min/max tolerance,
    /// the item is added and the method returns true.
    /// </para>
    /// <para>
    /// If the item leaves row height too high, there may be room to scale it down and add another item.
    /// In this case, the item is added and the method returns true, but the row is incomplete.
    /// </para>
    /// <para>
    /// If the item leaves row height too short, there are too many items to fit within tolerance.
    /// The method will either accept or reject the new item, favoring the resulting row height closest to within tolerance.
    /// </para>
    /// <para>
    /// If the item is rejected, left/right padding will be required to fit the row height within tolerance;
    /// if the item is accepted, top/bottom cropping will be required to fit the row height within tolerance.
    /// </para>
    /// </summary>
    /// <param name="itemData"><c>Item</c> layout data, containing item aspect ratio.</param>
    /// <returns><c>True</c> if successfully added; <c>false</c> if rejected.</returns>
    /// <seealso cref="Item"/>
    public bool TryAddItem(Item itemData)
    {
        // Calculate aspect ratios for Items only assuming new item will be added; exclude spacing
        var rowWidthWithoutSpacing = _parameters.Width - Items.Count * _parameters.Spacing;
        var newAspectRatio = Items.Sum(item => item.AspectRatio) + itemData.AspectRatio;
        var targetAspectRatio = rowWidthWithoutSpacing / TargetRowHeight;

        // Handle big full-width breakout photos if we're doing them
        // Only do it if there's no other items in this row
        // Only go full width if this photo is a square or landscape
        if (IsBreakoutRow && Items.Count == 0 && itemData.AspectRatio >= 1)
        {
            Items.Add(itemData);
            CompleteLayout(rowWidthWithoutSpacing / itemData.AspectRatio, LayoutStyle.Justified);
            
            return true;
        }

        if (newAspectRatio < _minAspectRatio)
        {
            // New aspect ratio is too narrow / scaled row height is too tall.
            // Accept this item and leave row open for more items.
            Items.Add(itemData);
            
            return true;
        }

        if (newAspectRatio < _maxAspectRatio)
        {
            Items.Add(itemData);
            CompleteLayout(rowWidthWithoutSpacing / newAspectRatio, LayoutStyle.Justified);

            return true;
        }

        // New aspect ratio / scaled row height is within tolerance;
        // accept the new item and complete the row layout.
        // New aspect ratio is too wide / scaled row height will be too short.
        // Accept item if the resulting aspect ratio is closer to target than it would be without the item.
        // NOTE: Any row that falls into this block will require cropping/padding on individual items.
        if (Items.Count == 0)
        {
            // When there are no existing items, force acceptance of the new item and complete the layout.
            // This is the pano special case.
            Items.Add(itemData);
            CompleteLayout(rowWidthWithoutSpacing / newAspectRatio, LayoutStyle.Justified);
                
            return true;
        }

        // Calculate width/aspect ratio for row before adding new item
        var previousRowWidthWithoutSpacing = _parameters.Width - (Items.Count - 1) * _parameters.Spacing;
        var previousAspectRatio = Items.Sum(item => item.AspectRatio);
        var previousTargetAspectRatio = previousRowWidthWithoutSpacing / TargetRowHeight;

        if (Math.Abs(newAspectRatio - targetAspectRatio) > Math.Abs(previousAspectRatio - previousTargetAspectRatio))
        {
            // Row with new item is us farther away from target than row without; complete layout and reject item.
            CompleteLayout(previousRowWidthWithoutSpacing / previousAspectRatio, LayoutStyle.Justified);
                
            return false;
        }

        // Row with new item is us closer to target than row without;
        // accept the new item and complete the row layout.
        Items.Add(itemData);
        CompleteLayout(rowWidthWithoutSpacing / newAspectRatio, LayoutStyle.Justified);
            
        return true;
    }

    /// <summary>
    /// Check if a <c>Row</c> has completed its layout.
    /// </summary>
    /// <returns><c>True</c> if complete; <c>false</c> if no.</returns>
    public bool IsLayoutComplete() => Height > 0;
    
    /// <summary>
    /// Force completion of a <c>Row</c> layout with current items.
    /// </summary>
    /// <param name="rowHeight">
    /// Set the <c>Row</c> height to this value.
    /// If not provided it will default to <see cref="TargetRowHeight"/>
    /// </param>
    public void ForceComplete(double? rowHeight = null) 
        => CompleteLayout(rowHeight ?? TargetRowHeight, _parameters.WidowLayoutStyle);

    /// <summary>
    /// Set the <c>Row</c> height and compute item geometry from that height.
    /// Will justify items within the row unless instructed not to.
    /// </summary>
    /// <param name="newHeight">Set row height to this value.</param>
    /// <param name="widowLayoutStyle">Define how items should be displayed.</param>
    /// <seealso cref="LayoutStyle"/>
    private void CompleteLayout(double newHeight, LayoutStyle widowLayoutStyle)
    {
        var itemWidthSum = _parameters.Left;
        var rowWidthWithoutSpacing = _parameters.Width - (Items.Count - 1) * _parameters.Spacing;
        // Clamp row height to edge case minimum/maximum.
        var clampedHeight = Math.Max(_parameters.EdgeCaseMinRowHeight, Math.Min(newHeight, _parameters.EdgeCaseMaxRowHeight));
        double clampedToNativeRatio;
        
        if (Math.Abs(newHeight - clampedHeight) > MinNormal)
        {
            // If row height was clamped, the resulting row/item aspect ratio will be off,
            // so force it to fit the width (recalculate aspectRatio to match clamped height).
            // NOTE: this will result in cropping/padding commensurate to the amount of clamping.
            Height = clampedHeight;
            clampedToNativeRatio = (rowWidthWithoutSpacing / clampedHeight) / (rowWidthWithoutSpacing / newHeight);
        }
        else
        {
            // If not clamped, leave ratio at 1.0.
            Height = newHeight;
            clampedToNativeRatio = 1.0;
        }

        // Compute item geometry based on newHeight.
        foreach (var item in Items)
        {
            item.Top = _parameters.Top;
            item.Width = item.AspectRatio * Height * clampedToNativeRatio;
            item.Height = Height;
            item.Left = itemWidthSum;
            itemWidthSum += item.Width + _parameters.Spacing;
        }

        switch (widowLayoutStyle)
        {
            case LayoutStyle.Justified:
            {
                // If specified, ensure items fill row and distribute error
                // caused by rounding width and height across all items.
                itemWidthSum -= _parameters.Spacing + _parameters.Left;
                var errorWidthPerItem = (itemWidthSum - _parameters.Width) / Items.Count;
                var roundedCumulativeErrors = Items.Select((item, i) => Math.Round((i + 1) * errorWidthPerItem)).ToList();

                if (Items.Count == 1)
                {
                    // For rows with only one item, adjust item width to fill row.
                    Items[0].Width -= Math.Round(errorWidthPerItem);
                }
                else
                {
                    // For rows with multiple items, adjust item width and shift items to fill the row,
                    // while maintaining equal spacing between items in the row.
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (i > 0)
                        {
                            Items[i].Left -= roundedCumulativeErrors[i - 1];
                            Items[i].Width -= (roundedCumulativeErrors[i] - roundedCumulativeErrors[i - 1]);
                        }
                        else
                        {
                            Items[i].Width -= roundedCumulativeErrors[i];
                        }
                    }
                }

                break;
            }
            case LayoutStyle.Center:
            {
                // Center widows
                var centerOffset = (_parameters.Width - itemWidthSum) / 2;

                foreach (var item in Items)
                {
                    item.Left += centerOffset + _parameters.Spacing;
                }

                break;
            }
            case LayoutStyle.Left:
            default:
                // Nothing to do here
                break;
        }
    }
}
