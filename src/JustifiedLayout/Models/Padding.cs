namespace JustifiedLayout.Models;

public class Padding(double value)
{
    public double Top { get; set; } = value;
    public double Right { get; set; } = value;
    public double Bottom { get; set; } = value;
    public double Left { get; set; } = value;
}
