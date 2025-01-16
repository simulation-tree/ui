using Cameras;
using Rendering;
using Worlds;

namespace InteractionKit.Tests
{
    public class LabelTests : InteractionKitTests
    {
        [Test]
        public void CheckIfLabelIsLabel()
        {
            Settings settings = new(world);
            Destination destination = new(world, new(1920, 1080), "dummy");
            Camera camera = Camera.CreateOrthographic(world, destination, 1f);
            Canvas canvas = new(world, settings, camera);
            Label label = new(canvas, "Test");
            Assert.That(label.Is(), Is.True);
        }
    }
}