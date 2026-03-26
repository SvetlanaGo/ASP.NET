using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.Core.Exceptions;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models.Partners;
using Soenneker.Utils.AutoBogus;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners;

public class SetLimitTests
{
    private readonly Mock<IRepository<Partner>> _partnerRepositoryMock;
    private readonly Mock<IRepository<PartnerPromoCodeLimit>> _partnerLimitsRepositoryMock;
    private readonly PartnersController _sut;

    public SetLimitTests()
    {
        _partnerRepositoryMock = new Mock<IRepository<Partner>>();
        _partnerLimitsRepositoryMock = new Mock<IRepository<PartnerPromoCodeLimit>>();
        _sut = new PartnersController(_partnerRepositoryMock.Object, _partnerLimitsRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateLimit_WhenPartnerNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var createRequest = CreatePartnerPromoCodeLimitCreateRequest();
        _partnerRepositoryMock
            .Setup(r => r.GetById(partnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await _sut.CreateLimit(partnerId, createRequest, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Partner not found");

        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Detail.Should().Contain(partnerId.ToString());

        _partnerLimitsRepositoryMock.Verify(
            r => r.Add(It.IsAny<PartnerPromoCodeLimit>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateLimit_WhenPartnerBlocked_ReturnsUnprocessableEntity()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var createRequest = CreatePartnerPromoCodeLimitCreateRequest();
        var partner = CreatePartner(partnerId, isActive: false);
        _partnerRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Act
        var result = await _sut.CreateLimit(partnerId, createRequest, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<UnprocessableEntityObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Partner blocked");

        _partnerLimitsRepositoryMock.Verify(
            r => r.Add(It.IsAny<PartnerPromoCodeLimit>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateLimit_WhenValidRequest_ReturnsCreatedAndAddsLimit()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var createRequest = CreatePartnerPromoCodeLimitCreateRequest();
        var partner = CreatePartner(partnerId, isActive: true);
        _partnerRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Act
        var result = await _sut.CreateLimit(partnerId, createRequest, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeOfType<PartnerPromoCodeLimitResponse>();

        _partnerLimitsRepositoryMock.Verify(
            r => r.Add(
                It.Is<PartnerPromoCodeLimit>(l => MatchesNewLimit(l, partner, createRequest)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _partnerRepositoryMock.Verify(
            r => r.Update(It.IsAny<Partner>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateLimit_WhenValidRequestWithActiveLimits_CancelsOldLimitsAndAddsNew()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var canceledAt = DateTimeOffset.UtcNow.AddDays(-1);
        var createRequest = CreatePartnerPromoCodeLimitCreateRequest();

        var activeLimit1 = CreateLimit();
        var activeLimit2 = CreateLimit();
        var canceledLimit = CreateLimit(canceledAt: canceledAt);

        var partner = CreatePartner(
                partnerId,
                isActive: true,
                additionalLimits: new[] { activeLimit1, activeLimit2, canceledLimit });

        _partnerRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Act
        var result = await _sut.CreateLimit(partnerId, createRequest, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeOfType<PartnerPromoCodeLimitResponse>();

        activeLimit1.CanceledAt.Should().NotBeNull();
        activeLimit2.CanceledAt.Should().NotBeNull();
        canceledLimit.CanceledAt.Should().Be(canceledAt);

        _partnerRepositoryMock.Verify(
            r => r.Update(partner, It.IsAny<CancellationToken>()),
            Times.Once);

        _partnerLimitsRepositoryMock.Verify(
            r => r.Add(
                It.Is<PartnerPromoCodeLimit>(l => MatchesNewLimit(l, partner, createRequest)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateLimit_WhenUpdateThrowsEntityNotFoundException_ReturnsNotFound()
    {
        //Arrange
        var partnerId = Guid.NewGuid();
        var activeLimit = CreateLimit();
        var partner = CreatePartner(
                partnerId,
                isActive: true,
                additionalLimits: new[] { activeLimit });
        var createRequest = CreatePartnerPromoCodeLimitCreateRequest();

        _partnerRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _partnerRepositoryMock
            .Setup(r => r.Update(partner, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException<Partner>(partner.Id));

        //Act
        var result = await _sut.CreateLimit(partner.Id, createRequest, CancellationToken.None);

        //Assert
        result.Result.Should().BeOfType<NotFoundResult>();

        _partnerLimitsRepositoryMock.Verify(
        r => r.Add(It.IsAny<PartnerPromoCodeLimit>(), It.IsAny<CancellationToken>()),
        Times.Never);
    }

    private static PartnerPromoCodeLimitCreateRequest CreatePartnerPromoCodeLimitCreateRequest() =>
        new AutoFaker<PartnerPromoCodeLimitCreateRequest>()
            .RuleFor(r => r.EndAt, DateTimeOffset.UtcNow.AddDays(30))
            .RuleFor(r => r.Limit, f => f.Random.Int(1, 1000))
            .Generate();

    private static Partner CreatePartner(
        Guid partnerId,
        bool isActive,
        IEnumerable<PartnerPromoCodeLimit>? additionalLimits = null)
    {
        var role = new AutoFaker<Role>()
            .RuleFor(r => r.Id, _ => Guid.NewGuid())
            .Generate();

        var employee = new AutoFaker<Employee>()
            .RuleFor(e => e.Id, _ => Guid.NewGuid())
            .RuleFor(e => e.Role, role)
            .Generate();

        var limits = additionalLimits?.ToList() ?? new List<PartnerPromoCodeLimit>();

        return new AutoFaker<Partner>()
            .RuleFor(p => p.Id, _ => partnerId)
            .RuleFor(p => p.IsActive, _ => isActive)
            .RuleFor(p => p.Manager, employee)
            .RuleFor(p => p.PartnerLimits, limits)
            .Generate();
    }

    private static PartnerPromoCodeLimit CreateLimit(Guid? id = null, DateTimeOffset? canceledAt = null) =>
        new AutoFaker<PartnerPromoCodeLimit>()
            .RuleFor(l => l.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(l => l.CanceledAt, canceledAt)
            .RuleFor(l => l.CreatedAt, _ => DateTimeOffset.UtcNow.AddDays(-7))
            .RuleFor(l => l.EndAt, _ => DateTimeOffset.UtcNow.AddDays(30))
            .Generate();

    private static bool MatchesNewLimit(
        PartnerPromoCodeLimit limit,
        Partner expectedPartner,
        PartnerPromoCodeLimitCreateRequest expectedRequest) =>
            limit.Partner == expectedPartner &&
            limit.EndAt == expectedRequest.EndAt &&
            limit.CanceledAt == null &&
            limit.Limit == expectedRequest.Limit &&
            limit.IssuedCount == 0;
}
