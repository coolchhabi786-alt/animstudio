using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Events;
using AnimStudio.IdentityModule.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AnimStudio.UnitTests.Commands
{
    public class RegisterUserCommandHandlerTests
    {
        [Fact]
        public void User_Create_RaisesUserRegisteredEvent()
        {
            var id = Guid.NewGuid();
            var user = User.Create(id, "auth0|123", "alice@example.com", "Alice");
            user.Id.Should().Be(id);
            user.Email.Should().Be("alice@example.com");
            user.DomainEvents.Should().ContainSingle(e => e is UserRegistered);
        }

        [Fact]
        public void User_UpdateProfile_ChangesDisplayName()
        {
            var user = User.Create(Guid.NewGuid(), "ext-1", "bob@example.com", "Bob");
            user.UpdateProfile("Robert", null);
            user.DisplayName.Should().Be("Robert");
        }

        [Fact]
        public void Email_IsValid_AcceptsValidEmails()
        {
            Email.IsValid("valid@example.com").Should().BeTrue();
            Email.IsValid("invalid").Should().BeFalse();
        }
    }
}
