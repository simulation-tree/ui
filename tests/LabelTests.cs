using Cameras;
using Rendering;
using Unmanaged;
using Worlds;

namespace UI.Tests
{
    public class LabelTests : UITests
    {
        [Test]
        public void VerifyLabelCompliance()
        {
            using World world = CreateWorld();

            Settings settings = new(world);
            Destination destination = new(world, new(1920, 1080), "dummy");
            Camera camera = Camera.CreateOrthographic(world, destination, 1f);
            Canvas canvas = new(settings, camera);
            Label label = new(canvas, "Test");
            Assert.That(label.IsCompliant, Is.True);

            ASCIIText256 text = "ababish";

            label.SetText(text);

            Assert.That(label.UnderlyingText.ToString(), Is.EqualTo(text.ToString()));
        }

        [Test]
        public void CheckLabelTextUpdating()
        {
            using World world = CreateWorld();

            Settings settings = new(world);
            Destination destination = new(world, new(1920, 1080), "dummy");
            Camera camera = Camera.CreateOrthographic(world, destination, 1f);
            Canvas canvas = new(settings, camera);
            Label label = new(canvas, "Test");

            Assert.That(label.UnderlyingText.ToString(), Is.EqualTo("Test"));

            ASCIIText256 text = "ababish";

            label.SetText(text);

            Assert.That(label.UnderlyingText.ToString(), Is.EqualTo(text.ToString()));
        }
    }
}