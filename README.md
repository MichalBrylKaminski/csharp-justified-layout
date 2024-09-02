# Justified Layout

A .NET implementation of [Flickr's Justified Layout](https://github.com/flickr/justified-layout)

## Usage

There are two ways to use it

### JustifiedLayoutHelper

A static class that can be used directly in your code:

```var layoutGeometry = JustifiedLayout.ComputeLayout(layoutConfig, layoutData, itemLayoutData)```

### IJustifiedLayoutService

An interface that you can inject into your class

```
public interface IJustifiedLayoutService
{
    Layout ComputeLayout(LayoutConfig layoutConfig, LayoutData layoutData, List<Item> itemLayoutData);
}
```
