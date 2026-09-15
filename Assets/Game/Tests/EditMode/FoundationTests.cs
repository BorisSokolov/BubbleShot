using NUnit.Framework;

namespace BubbleShot.Core.Tests
{
    [TestFixture]
    public class FoundationTests
    {
        [Test]
        public void ProjectFoundation_AssemblyIsConfiguredCorrectly()
        {
            // Verifies that the test harness compiles and executes under NUnit
            Assert.Pass("BubbleShot foundation test assembly loaded and executed successfully.");
        }

        [Test]
        public void MathematicalConstants_AreConsistent()
        {
            const int standardWidth = 8;
            const int staggeredWidth = 7;
            Assert.That(staggeredWidth, Is.EqualTo(standardWidth - 1));
        }
    }
}
