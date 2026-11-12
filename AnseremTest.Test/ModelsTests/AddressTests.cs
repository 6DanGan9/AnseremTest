
using AnseremTest.Models;
using Microsoft.AspNetCore.Routing;

namespace AnseremTest.Tests.ModelsTests
{
    public class AddressTests
    {
        private const string ValidAddress = "v.vladislavovich@alfa.com";

        [Fact]
        public void Constructor_SetsName()
        {
            var address = new Address(ValidAddress);
            Assert.Equal(ValidAddress, address.Name);
        }

        [Fact]
        public void GetDomain_ValidAddress_ReturnsDomain()
        {
            var address = new Address(ValidAddress);
            Assert.Equal("alfa.com", address.GetDomain());
        }

        [Fact]
        public void GetDomain_AnotherValidAddress_ReturnsDomain()
        {
            var address = new Address("user@example.org");
            Assert.Equal("example.org", address.GetDomain());
        }

        [Theory]
        [InlineData("v.vladislavovich@alfa.com", "alfa.com")]
        [InlineData("a@b.co", "b.co")]
        [InlineData("name@sub.domain.com", "sub.domain.com")]
        public void GetDomain_MultipleAddresses_ReturnsExpectedDomain(string name, string expected)
        {
            var address = new Address(name);
            Assert.Equal(expected, address.GetDomain());
        }

        [Fact]
        public void IsValid_ValidAddress_ReturnsTrue()
        {
            var address = new Address(ValidAddress);
            Assert.True(address.IsValid());
        }

        [Theory]
        [InlineData("plainaddress")]          // нет @ и домена
        [InlineData("@alfa.com")]             // нет локальной части
        [InlineData("v.vladislavovich@")]     // нет домена
        [InlineData("v.vladislavovich@alfa")] // нет точки в домене
        [InlineData("v vladislavovich@alfa.com")] // пробел
        public void IsValid_InvalidAddresses_ReturnsFalse(string name)
        {
            var address = new Address(name);
            Assert.False(address.IsValid());
        }

        [Fact]
        public void IsValid_Length254_ReturnsTrue()
        {
            // 247 символов локальной части + "@a.co" = ровно 254
            string name = new string('a', 247) + "@a.co";
            var address = new Address(name);
            Assert.True(address.IsValid());
        }

        [Fact]
        public void IsValid_LengthMoreThan254_ThrowsException()
        {
            string name = new string('a', 248) + "@a.co"; // 255 символов
            var address = new Address(name);

            try
            {
                address.IsValid();
            }
            catch (Exception ex)
            {
                Assert.Equal("Адрес не может быть длиннее 254 символов", ex.Message);
            }
        }

    }
}
