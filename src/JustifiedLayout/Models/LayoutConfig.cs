namespace JustifiedLayout.Models;

public class LayoutConfig
{
    public double ContainerWidth { get; set; } = 1060;
    public Padding ContainerPadding { get; set; } = new(10);
    public Spacing BoxSpacing { get; set; } = new(10);
    public double TargetRowHeight { get; set; } = 320;
    public double TargetRowHeightTolerance { get; set; } = 0.25;
    public int MaxNumRows { get; set; } = int.MaxValue;
    public bool ForceAspectRatio { get; set; } = false;
    public bool ShowWidows { get; set; } = true;
    public int? FullWidthBreakoutRowCadence { get; set; } = null;
    public LayoutStyle WidowLayoutStyle { get; set; } = LayoutStyle.Left;
    public int WidowCount { get; set; } = 0;
}
