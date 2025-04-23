# UI

Contains abstractions of pieces from various projects, with systems to fulfil them to provide
the tools to build a retained UI environment.

### Using

To use any of this library's features, there must first exist a `Settings` entity:
```cs
using (World world = new())
{
    Settings settings = new(world);
}
```

### Canvases

A canvas and an interaction context are needed to build UI from. They only require a reference to an **orthographic** camera.
```cs
Window window = new(world, "Editor", new(200, 200), new(900, 720), "vulkan");
Camera camera = Camera.CreateOrthographic(world, window, 1f));
Canvas canvas = new(settings, camera);
```

### Custom `IVirtualWindow` types

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

### Control fields

Control fields draw an editor for a component with a prefixed label:
```cs
Entity dataEntity = new(world);
dataEntity.AddComponent<float>(0f);

ControlField numberField = ControlField.Create<float, NumberTextEditor>(canvas, "Number", dataEntity);
numberField.LabelColor = new(0, 0, 0, 1);
numberField.Anchor = Anchor.TopLeft;
numberField.Pivot = new(0f, 1f, 0f);
numberField.Position = new(100, 100);
numberField.Width = 180f;
```

Included are these editors:
- `NumberTextEditor` for numeric components:
    - `FixedString`
    - `byte` and `sbyte`
    - `short` and `ushort`
    - `int` and `uint`
    - `long` and `ulong`
- `TextEditor` for text components and arrays:
    - `FixedString` component
    - array of `char`
    - array of `LabelCharacter`
- `BooleanEditor` for boolean components:
    - `bool` component

### Images

Image entities display a colored square, with the option to display a custom texture:
```cs
Image image = new(canvas);
image.Size = new(64, 64);
image.Position = new(100, 100);
image.Anchor = Anchor.TopLeft;
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

### Preprocessing labels

Assume the following context:
```cs
Label testLabel = new(world, context, "This is {{something}} to be replaced");
```

All label entities contain two arrays to describe their content. One for the original
text as `TextCharacter`, and another for the processed text as `LabelCharacter`. This
allows processing of the original text, possible in two ways:

With a label processor entity function, it gives maximum flexibility as it lets the
programmer replace the entire text with a new one:
```cs
LabelProcessor.Create(world, new(&ReplaceText));

[UnmanagedCallersOnly]
private static Bool ReplaceText(TryProcessLabel.Input input)
{
    ReadOnlySpan<char> original = input.OriginalText;
    if (original.Contains("{{something}}"))
    {
        input.SetResult("Actually a different piece of text");
        return true; //text becomes "Actually a different piece of text"
    }

    return false;
}
```

With a `IsToken` component, the component expects the token without the {{ and }}.
Along with a `char`/`LabelCharacter`/`TextCharacter` array for the replacement text:
```cs
Entity dataEntity = new(world);
dataEntity.AddComponent(new IsToken("something"));
dataEntity.CreateArray("green".AsSpan());

//text becomes "This is green to be replaced"
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
    Trace.WriteLine($"Pressed button `{buttonEntity}`");
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
    Trace.WriteLine($"Selected option: {option.text}");
}
```