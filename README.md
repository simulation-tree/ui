# Interaction Kit
Contains abstractions of pieces from various projects, with systems to fulfil them to provide
the tools to build a retained UI environment.

### Canvases and Interaction contexts
A canvas and an interaction context are needed to build UI from. They only require a reference to an **orthographic** camera.
```cs
using (World world = new())
{
    Window window = new(world, "Editor", new(200, 200), new(900, 720), "vulkan", new(&OnWindowClosed));
    Camera uiCamera = new(world, window, new CameraOrthographicSize(1f));
    Canvas canvas = new(world, uiCamera);
    using (InteractiveContext context = new(canvas))
    {
        //update
    }
}
```
When execution is finished, the `InteractiveContext` type is expected to be disposed. UI controls are expected to be a
descendant of a canvas.

### Extending virtual windows with `IVirtualWindow` types
The virtual window type contains the ability to display elements inside its container, can be dragged around and closed.
This example is an example that only contains a `Hello, World!` label:
```cs
VirtualWindow box = VirtualWindow.Create<ExampleWindow>(world, context);
box.Size = new(300, 300);
box.Position = new(40, 40);

public readonly struct ExampleWindow : IVirtualWindow
{
    readonly FixedString IVirtualWindow.Title => "Example";
    readonly unsafe VirtualWindowCloseFunction IVirtualWindow.CloseCallback => new(&Closed);

    readonly void IVirtualWindow.OnCreated(VirtualWindow window, InteractiveContext context)
    {
        Label testLabel = new(world, context, "Hello, World!");
        testLabel.Parent = window.Container; //make sure to parent under the window's container
        testLabel.Anchor = Anchor.TopLeft;
        testLabel.Color = Color.Black;
        testLabel.Position = new(4f, -4f);
        testLabel.Pivot = new(0f, 1f, 0f);
    }

    [UnmanagedCallersOnly]
    private static void Closed(VirtualWindow virtualWindow)
    {
        virtualWindow.Destroy();
    }
}
```

### Labels
Labels are entities that combine `TextRenderer` and `TextMesh` entities:
```cs
Label testLabel = new(world, context, "Hello, World!");
testLabel.Parent = canvas;
testLabel.Anchor = Anchor.TopLeft;
testLabel.Color = Color.Black;
testLabel.Pivot = new(0f, 1f, 0f);
```

### Buttons
The `Button` type only implements the callback, so for the common labelled button that requires
a `Label` entity to display the text:
```cs
Button testButton = new(world, new(&PressedTestButton), context);
testButton.Parent = canvas;
testButton.Color = Color.Black;
testButton.Anchor = Anchor.TopLeft;
testButton.Pivot = new(0f, 1f, 0f);
testButton.Size = new(180f, 24f);

Label testButtonLabel = new(world, context, "Press me");
testButtonLabel.Parent = testButton.AsEntity();
testButtonLabel.Anchor = Anchor.TopLeft;
testButtonLabel.Position = new(4f, -4f);
testButtonLabel.Pivot = new(0f, 1f, 0f);

[UnmanagedCallersOnly]
static void PressedTestButton(World world, uint buttonEntity)
{
    Debug.WriteLine($"Pressed button `{buttonEntity}`");
}
```

### Toggles
```cs
bool initialValue = false;
Toggle testToggle = new(world, context, initialValue);
testToggle.Parent = canvas;
testToggle.Size = new(24, 24);
testToggle.Anchor = Anchor.TopLeft;
testToggle.Pivot = new(0f, 1f, 0f);
testToggle.BackgroundColor = Color.Black;
testToggle.CheckmarkColor = Color.White;
```

### Scroll bars
```cs
float percentageSize = 0.25f;
ScrollBar horizontalScrollBar = new(world, context, Vector2.UnitX, percentageSize);
horizontalScrollBar.Parent = canvas;
horizontalScrollBar.Size = new(180f, 24f);
horizontalScrollBar.Anchor = Anchor.TopLeft;
horizontalScrollBar.Pivot = new(0f, 1f, 0f);
horizontalScrollBar.BackgroundColor = Color.Black;
horizontalScrollBar.ScrollHandleColor = Color.White;
```

### Dropdowns
```cs
Dropdown testDropdown = new(world, context);
testDropdown.Parent = canvas;
testDropdown.Size = new(180f, 24);
testDropdown.Anchor = Anchor.TopLeft;
testDropdown.Pivot = new(0f, 1f, 0f);
testDropdown.BackgroundColor = Color.Black;
testDropdown.LabelColor = Color.White;
testDropdown.TriangleColor = Color.White;

Menu testDropdownMenu = testDropdown.Menu;
testDropdownMenu.AddOption("Option A", context);
testDropdownMenu.AddOption("Option B", context);
OptionPath lastOption = testDropdownMenu.AddOption("Option C", context);
testDropdownMenu.AddOption("Option D/Apple", context);
testDropdownMenu.AddOption("Option D/Banana", context);
testDropdownMenu.AddOption("Option D/Cherry", context);
testDropdownMenu.AddOption("Option D/More.../Toyota", context);
testDropdownMenu.AddOption("Option D/More.../Honda", context);
testDropdownMenu.AddOption("Option D/More.../Hyndai", context);
testDropdownMenu.AddOption("Option D/More.../Mitsubishi", context);

testDropdown.SelectedOption = lastOption;
testDropdown.Callback = new(&DropdownOptionChanged);

[UnmanagedCallersOnly]
static void DropdownOptionChanged(Dropdown dropdown, uint previous, uint current)
{
    MenuOption option = dropdown.Options[current];
    Debug.WriteLine($"Selected option: {option.text}");
}
```
