namespace JustifiedLayout.Models;

public class RowParams
{
    public double Top { get; set; }
    public double Left { get; set; }
    public double Width { get; set; }
    public double Spacing { get; set; }
    public double TargetRowHeight { get; set; }
    public double TargetRowHeightTolerance { get; set; }
    public double EdgeCaseMinRowHeight { get; set; }
    public double EdgeCaseMaxRowHeight { get; set; }
    public LayoutStyle WidowLayoutStyle { get; set; }
    public bool IsBreakoutRow { get; set; }
}
