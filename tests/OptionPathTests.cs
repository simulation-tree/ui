namespace InteractionKit.Tests
{
    public class OptionPathTests
    {
        [Test]
        public void CreatePathFromText()
        {
            OptionPath a = "2/7";
            Assert.That(a.Depth, Is.EqualTo(2));
            Assert.That(a[0], Is.EqualTo(2));
            Assert.That(a[1], Is.EqualTo(7));
        }

        [Test]
        public void CreateRootPathFromText()
        {
            OptionPath a = "5";
            Assert.That(a.Depth, Is.EqualTo(1));
            Assert.That(a[0], Is.EqualTo(5));
        }
    }
}
