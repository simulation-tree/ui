using Cameras;
using Rendering;
using Worlds;

namespace UI.Tests
{
    public class LabelTests : UITests
    {
        [Test]
        public void CheckIfLabelIsLabel()
        {
            using World world = CreateWorld();
            Settings settings = new(world);
            Destination destination = new(world, new(1920, 1080), "dummy");
            Camera camera = Camera.CreateOrthographic(world, destination, 1f);
            Canvas canvas = new(world, settings, camera);
            Label label = new(canvas, "Test");
            Assert.That(label.IsCompliant, Is.True);
        }
    }
}