using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using Xunit;

namespace BarberWeb.Domain.Tests.Entities
{
    public class ProfessionalDeactivationTests
    {
        private static readonly DateTimeOffset Now = new(2026, 11, 30, 12, 0, 0, TimeSpan.Zero);

        private static Professional PersistedProfessional(int id = 42)
            => new("João Silva", "joao@email.com", "11988887777") { Id = id };

        [Fact]
        public void NewProfessional_ShouldBeActive()
        {
            var professional = PersistedProfessional();

            Assert.False(professional.IsDeleted);
            Assert.Null(professional.DeletedAt);
        }

        [Fact]
        public void Deactivate_ShouldSetDeletedAtAndKeepName()
        {
            var professional = PersistedProfessional();

            professional.Deactivate(Now);

            Assert.True(professional.IsDeleted);
            Assert.Equal(Now, professional.DeletedAt);
            Assert.Equal("João Silva", professional.Name);
        }

        [Fact]
        public void Deactivate_ShouldAnonymizeEmailAndPhone()
        {
            var professional = PersistedProfessional(42);

            professional.Deactivate(Now);

            Assert.Equal("deleted-42@removed.invalid", professional.Email);
            Assert.Equal("0000000000042", professional.PhoneNumber);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(42)]
        [InlineData(int.MaxValue)]
        public void Deactivate_ShouldGenerateUniqueTombstonesThatRespectColumnLimits(int id)
        {
            var professional = PersistedProfessional(id);

            professional.Deactivate(Now);

            Assert.InRange(professional.Email.Length, 5, 60);
            Assert.Equal(13, professional.PhoneNumber.Length);
            Assert.All(professional.PhoneNumber, c => Assert.True(char.IsAsciiDigit(c)));
            Assert.StartsWith("000", professional.PhoneNumber);
        }

        [Fact]
        public void Deactivate_ForDifferentProfessionals_ShouldNotCollide()
        {
            var first = PersistedProfessional(1);
            var second = PersistedProfessional(2);

            first.Deactivate(Now);
            second.Deactivate(Now);

            Assert.NotEqual(first.Email, second.Email);
            Assert.NotEqual(first.PhoneNumber, second.PhoneNumber);
        }

        [Fact]
        public void Deactivate_WhenAlreadyDeactivated_ShouldThrowBusinessException()
        {
            var professional = PersistedProfessional();
            professional.Deactivate(Now);

            var exception = Assert.Throws<BusinessException>(() => professional.Deactivate(Now.AddHours(1)));

            Assert.Equal("Professional is already deactivated.", exception.Message);
            Assert.Equal(Now, professional.DeletedAt);
        }

        [Fact]
        public void Deactivate_WhenNotPersisted_ShouldThrowInvalidOperationException()
        {
            var professional = new Professional("João Silva", "joao@email.com", "11988887777"); // Id = 0

            Assert.Throws<InvalidOperationException>(() => professional.Deactivate(Now));
            Assert.False(professional.IsDeleted);
        }
    }
}
