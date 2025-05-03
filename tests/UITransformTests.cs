using System;
using Worlds;

namespace UI.Tests
{
    public class UITransformTests : UITests
    {
        [Test]
        public void RotateTransform()
        {
            using World world = CreateWorld();
            UITransform a = new(world, new(-123, 6990), new(100, 800), MathF.Tau * 0.25f);
            Assert.That(a.Position.X, Is.EqualTo(-123));
            Assert.That(a.Position.Y, Is.EqualTo(6990));
            Assert.That(a.Rotation, Is.EqualTo(MathF.Tau * 0.25f).Within(0.01f));
            Assert.That(a.Width, Is.EqualTo(100));
            Assert.That(a.Height, Is.EqualTo(800));

            UITransform b = new(world, new(-123, 6990), new(100, 800), MathF.Tau * -0.5f);
            Assert.That(b.Rotation, Is.EqualTo(MathF.Tau * 0.5f).Within(0.01f));
        }
    }
}