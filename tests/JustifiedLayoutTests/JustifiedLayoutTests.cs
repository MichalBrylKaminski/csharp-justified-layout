using JustifiedLayout;
using JustifiedLayout.Models;

namespace JustifiedLayoutTests;

[TestFixture]
public class JustifiedLayoutTests
{
    [Test]
    public void ShouldCreateAdditionalRowsIfItWontFitWithinConstraints()
    {
        var layoutConfig = new LayoutConfig
        {
            ContainerWidth = 200,
            TargetRowHeight = 100
        };

        var layoutData = new LayoutData();
        var input = new List<double> { 1, 2 };

        var geometry = LayoutHelper.ComputeLayout(layoutConfig, layoutData, ConvertInputToItems(input));

        Assert.That(geometry.Boxes[0].Top, Is.EqualTo(10));
        Assert.That(geometry.Boxes[1].Top, Is.EqualTo(200));
    }

    [Test]
    public void ShouldNotAddTheRowIfWeAreLimitingItWithMaxNumRows()
    {
        var layoutConfig = new LayoutConfig
        {
            ContainerWidth = 200,
            TargetRowHeight = 100,
            MaxNumRows = 2
        };

        var layoutData = new LayoutData();
        var input = new List<double> { 1, 2, 1 };

        var geometry = LayoutHelper.ComputeLayout(layoutConfig, layoutData, ConvertInputToItems(input));

        Assert.That(geometry.Boxes.Count, Is.EqualTo(2));
    }

    [Test]
    public void ShouldHandleAPanoramaAsOnlyRowItem()
    {
        var layoutConfig = new LayoutConfig();
        var layoutData = new LayoutData();
        var input = new List<double> { 5 };

        var geometry = LayoutHelper.ComputeLayout(layoutConfig, layoutData, ConvertInputToItems(input));

        Assert.That(geometry.Boxes.Count, Is.EqualTo(1));
    }

    [Test]
    public void ShouldAllowNewItemAddedToTheRowToGetCloserToTheTargetRowHeight()
    {
        var layoutConfig = new LayoutConfig
        {
            ContainerWidth = 1000,
            TargetRowHeight = 250
        };

        var layoutData = new LayoutData();
        var input = new List<double> { 1, 4, 1.1 };

        var geometry = LayoutHelper.ComputeLayout(layoutConfig, layoutData, ConvertInputToItems(input));

        Assert.That(geometry.Boxes[0].Height, Is.EqualTo(194));
        Assert.That(geometry.Boxes[1].Height, Is.EqualTo(194));
        Assert.That(geometry.Boxes[2].Height, Is.EqualTo(194));
    }

    [Test]
    public void ShouldHandleWidthAndHeightObjectsAsInput()
    {
        var layoutConfig = new LayoutConfig();
        var layoutData = new LayoutData();
        var input = new List<(double Width, double Height)>
        {
            (400, 400),
            (500, 500),
            (600, 600),
            (700, 700)
        };

        var geometry = LayoutHelper.ComputeLayout(layoutConfig, layoutData, ConvertInputToItems(input));

        Assert.That(geometry.Boxes.Count, Is.EqualTo(4));
    }

    [Test]
    public void ShouldHandleArrayOfAspectRatiosAsInput()
    {
        var layoutConfig = new LayoutConfig();
        var layoutData = new LayoutData();
        var input = new List<double> { 1, 1, 1, 1 };

        var geometry = LayoutHelper.ComputeLayout(layoutConfig, layoutData, ConvertInputToItems(input));

        Assert.That(geometry.Boxes.Count, Is.EqualTo(4));
    }

    [Test]
    public void ShouldErrorIfBoxHeightNotPassedIn()
    {
        var layoutConfig = new LayoutConfig();
        var layoutData = new LayoutData();
        var input = new List<Item>
        {
            new Item { Width = 10 }
        };

        Assert.Throws<ArgumentException>(() => LayoutHelper.ComputeLayout(layoutConfig, layoutData, input));
    }

    [Test]
    public void ShouldReturnLayoutWithoutPassingInAConfig()
    {
        var input = new List<double> { 1, 1, 1, 1 };

        var geometry = LayoutHelper.ComputeLayout(new LayoutConfig(), new LayoutData(), ConvertInputToItems(input));

        Assert.That(geometry.Boxes.Count, Is.EqualTo(4));
    }

    [Test]
    public void ShouldAllowOverridingOfContainerWidth()
    {
        var layoutConfig = new LayoutConfig
        {
            ContainerWidth = 400
        };

        var layoutData = new LayoutData();
        var input = new List<double> { 1, 1, 1, 1 };

        var geometry = LayoutHelper.ComputeLayout(layoutConfig, layoutData, ConvertInputToItems(input));

        Assert.That(geometry.Boxes.Count, Is.EqualTo(4));
    }

    private List<Item> ConvertInputToItems(List<double> input)
    {
        return input.Select(i => new Item { AspectRatio = i }).ToList();
    }

    private List<Item> ConvertInputToItems(List<(double Width, double Height)> input)
    {
        return input.Select(i => new Item { AspectRatio = i.Width / i.Height }).ToList();
    }
}
