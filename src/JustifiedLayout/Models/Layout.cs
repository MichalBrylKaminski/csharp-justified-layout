namespace JustifiedLayout.Models;

public class Layout
{
    public double ContainerHeight { get; set; }
    public int WidowCount { get; set; }
    public List<Item> Boxes { get; set; } = new();
}
