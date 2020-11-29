using Xunit;

namespace CairoDesktop.Tests
{
    public class FeelGoodTests
    {
        [Fact]
        public void Int_Parse_Succeeds()
        {
            // Arrange
            string tenString = "10";
            int tenInt = 10;

            // Act
            int result = int.Parse(tenString);

            // Assert
            Assert.Equal(tenInt, result);
        }
    }
}