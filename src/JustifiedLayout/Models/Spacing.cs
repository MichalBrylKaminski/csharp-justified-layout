namespace JustifiedLayout.Models;

public class Spacing(double value)
{
    public double Horizontal { get; set; } = value;
    public double Vertical { get; set; } = value;
}
