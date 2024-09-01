namespace JustifiedLayout.Models;

public class LayoutData
{
    public List<Item> LayoutItems { get; set; } = new();
    public List<Row> Rows { get; set; } = new();
    public double ContainerHeight { get; set; }
}
