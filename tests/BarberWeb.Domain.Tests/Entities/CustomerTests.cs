
using BarberWeb.Domain.Entities;

namespace BarberWeb.Domain.Tests.Entities
{
    public class CustomerTests
    {
        [Theory]
        [InlineData("AA")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA62")]
        public void CreateCustomer_WithInvalidName_ThrowsException(string name)
        {
            string email = "teste@123.com";
            string phoneNumber = "4002-89222";

            var ex = Assert.Throws<ArgumentException>(() => new Customer(name, email, phoneNumber));
            Assert.Equal("Name must be between 3 and 60 characters.", ex.Message);
        }

        [Theory]
        [InlineData("4002-8922")]
        [InlineData("40002-8922000102")]
        public void CreateCustomer_WithInvalidPhoneNumber_ThrowsException(string name)
        {
            string email = "teste@123.com";
            string phoneNumber = "4002-8922"; //9

            var ex = Assert.Throws<ArgumentException>(() => new Customer(name, email, phoneNumber));
            Assert.Equal("Phone Number must be between 10 and 15 characters.", ex.Message);
        }

        [Fact]
        public void CreateCustomer_WithValidName_CreatesCustomer()
        {
            string email = "teste@123.com";
            string phoneNumber = "4002-89222";
            string name = "Olá, você esta lendo?";

            var customer = new Customer(name, email, phoneNumber);

            Assert.Equal("Olá, você esta lendo?", customer.Name);
            Assert.Equal("4002-89222", customer.PhoneNumber);
            Assert.Equal("teste@123.com", customer.Email);
        }
    }
}
