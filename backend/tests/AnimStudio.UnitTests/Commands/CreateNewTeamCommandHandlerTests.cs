using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Events;
using AnimStudio.IdentityModule.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AnimStudio.UnitTests.Commands
{
    public class CreateNewTeamCommandHandlerTests
    {
        [Fact]
        public void Team_Create_AddsOwnerMemberAndRaisesEvent()
        {
            var id = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var team = Team.Create(id, "Acme Studio", ownerId);
            team.Id.Should().Be(id);
            team.Members.Should().ContainSingle(m => m.UserId == ownerId && m.Role == TeamRole.Owner);
            team.DomainEvents.Should().ContainSingle(e => e is TeamCreated);
        }

        [Fact]
        public void Team_InviteMember_AddsPendingMember()
        {
            var team = Team.Create(Guid.NewGuid(), "Studio", Guid.NewGuid());
            team.ClearDomainEvents();
            var result = team.InviteMember(Guid.NewGuid(), TeamRole.Member);
            result.IsSuccess.Should().BeTrue();
            team.Members.Should().HaveCountGreaterThan(1);
        }
    }
}
