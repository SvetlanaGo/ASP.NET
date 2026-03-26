using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models.PromoCodes;
using Soenneker.Utils.AutoBogus;
using System.Linq.Expressions;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.PromoCodes;

public class CreateTests
{
    private readonly Mock<IRepository<PromoCode>> _promoCodesRepositoryMock;
    private readonly Mock<IRepository<Customer>> _customersRepositoryMock;
    private readonly Mock<IRepository<CustomerPromoCode>> _customerPromoCodesRepositoryMock;
    private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
    private readonly Mock<IRepository<Preference>> _preferencesRepositoryMock;
    private readonly PromoCodesController _sut;

    public CreateTests()
    {
        _promoCodesRepositoryMock = new Mock<IRepository<PromoCode>>();
        _customersRepositoryMock = new Mock<IRepository<Customer>>();
        _customerPromoCodesRepositoryMock = new Mock<IRepository<CustomerPromoCode>>();
        _partnersRepositoryMock = new Mock<IRepository<Partner>>();
        _preferencesRepositoryMock = new Mock<IRepository<Preference>>();

        _sut = new PromoCodesController(
            _promoCodesRepositoryMock.Object,
            _customersRepositoryMock.Object,
            _customerPromoCodesRepositoryMock.Object,
            _partnersRepositoryMock.Object,
            _preferencesRepositoryMock.Object);
    }

    [Fact]
    public async Task Create_WhenPartnerNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        _partnersRepositoryMock
            .Setup(r => r.GetById(request.PartnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Partner not found");

        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Detail.Should().Contain(request.PartnerId.ToString());

        _promoCodesRepositoryMock.Verify(
            r => r.Add(It.IsAny<PromoCode>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_WhenPreferenceNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        var activeLimit = CreateLimit();
        var partner = CreatePartner(request.PartnerId, additionalLimits: new[] { activeLimit });

        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _preferencesRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Preference?)null);

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Preference not found");

        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Detail.Should().Contain(request.PreferenceId.ToString());

        _promoCodesRepositoryMock.Verify(
            r => r.Add(It.IsAny<PromoCode>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_WhenNoActiveLimit_ReturnsUnprocessableEntity()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        var canceledLimit = CreateLimit(canceledAt: DateTimeOffset.UtcNow.AddDays(-1));
        var expiredLimit = CreateLimit(endAtAddDays: -1);
        var partner = CreatePartner(request.PartnerId, additionalLimits: new[]
        {
            canceledLimit,
            expiredLimit
        });

        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _preferencesRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePreference(request.PreferenceId));

        _customersRepositoryMock
            .Setup(r => r.GetWhere(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>().AsReadOnly());

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("No active limit");

        _promoCodesRepositoryMock.Verify(
            r => r.Add(It.IsAny<PromoCode>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_WhenLimitExceeded_ReturnsUnprocessableEntity()
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        var activeLimit = CreateLimit(issuedCount: 10, limit: 10);
        var partner = CreatePartner(request.PartnerId, additionalLimits: new[] { activeLimit });

        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _preferencesRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePreference(request.PreferenceId));

        _customersRepositoryMock
            .Setup(r => r.GetWhere(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>().AsReadOnly());

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Limit exceeded");

        _promoCodesRepositoryMock.Verify(
            r => r.Add(It.IsAny<PromoCode>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0, 0, 100, TestDisplayName = "When zero customers with preference")]
    [InlineData(1, 0, 100, TestDisplayName = "When one customer with preference")]
    [InlineData(2, 5, 100, TestDisplayName = "When multiple customers, mid-range issued")]
    [InlineData(5, 99, 100, TestDisplayName = "When near limit boundary")]
    [InlineData(10, 0, 1000, TestDisplayName = "When many customers, high limit")]
    public async Task Create_WhenValidRequest_ReturnsCreatedAndIncrementsIssuedCount(
        int customersCount,
        int initialIssuedCount,
        int limit)
    {
        // Arrange
        var request = CreatePromoCodeCreateRequest();
        var preference = CreatePreference(request.PreferenceId);

        var customers = Enumerable.Range(0, customersCount)
            .Select(_ => CreateCustomer(preference))
            .ToArray();

        var activeLimit = CreateLimit(issuedCount: initialIssuedCount, limit: limit);
        var partner = CreatePartner(
            request.PartnerId,
            additionalLimits: new[] { activeLimit });

        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _preferencesRepositoryMock
            .Setup(r => r.GetById(preference.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);

        _customersRepositoryMock
            .Setup(r => r.GetWhere(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers.AsReadOnly());

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeOfType<PromoCodeShortResponse>();

        _promoCodesRepositoryMock.Verify(
            r => r.Add(
                It.Is<PromoCode>(pc => MatchesPromoCode(pc, request, partner, preference, customersCount)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _customerPromoCodesRepositoryMock.Verify(
            r => r.Add(It.IsAny<CustomerPromoCode>(), It.IsAny<CancellationToken>()),
            Times.Never);

        activeLimit.IssuedCount.Should().Be(initialIssuedCount + 1);

        _partnersRepositoryMock.Verify(
            r => r.Update(partner, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static PromoCodeCreateRequest CreatePromoCodeCreateRequest() =>
        new AutoFaker<PromoCodeCreateRequest>()
            .RuleFor(r => r.Code, f => f.Lorem.Word())
            .RuleFor(r => r.ServiceInfo, f => f.Lorem.Sentence())
            .RuleFor(r => r.PartnerId, _ => Guid.NewGuid())
            .RuleFor(r => r.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(r => r.EndDate, _ => DateTimeOffset.UtcNow.AddDays(30))
            .RuleFor(r => r.PreferenceId, _ => Guid.NewGuid())
            .Generate();

    private static Partner CreatePartner(
        Guid partnerId,
        IEnumerable<PartnerPromoCodeLimit>? additionalLimits = null) =>
            new AutoFaker<Partner>()
                .RuleFor(p => p.Id, _ => partnerId)
                .RuleFor(p => p.IsActive, _ => true)
                .RuleFor(p => p.PartnerLimits, _ => additionalLimits?.ToList() ?? new List<PartnerPromoCodeLimit>())
                .Generate();

    private static Preference CreatePreference(Guid? id = null) =>
        new AutoFaker<Preference>()
            .RuleFor(p => p.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Lorem.Word())
            .RuleFor(p => p.Customers, new List<Customer>())
            .Generate();

    private static Customer CreateCustomer(Preference preference) =>
        new AutoFaker<Customer>()
            .RuleFor(c => c.Id, _ => Guid.NewGuid())
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Preferences, new List<Preference> { preference })
            .Generate();

    private static PartnerPromoCodeLimit CreateLimit(
        int endAtAddDays = 30,
        DateTimeOffset? canceledAt = null,
        int issuedCount = 0,
        int limit = 100) =>
            new AutoFaker<PartnerPromoCodeLimit>()
                .RuleFor(l => l.Id, _ => Guid.NewGuid())
                .RuleFor(l => l.EndAt, _ => DateTimeOffset.UtcNow.AddDays(endAtAddDays))
                .RuleFor(l => l.CreatedAt, _ => DateTimeOffset.UtcNow.AddDays(-30))
                .RuleFor(l => l.CanceledAt, _ => canceledAt)                
                .RuleFor(l => l.Limit, _ => limit)
                .RuleFor(l => l.IssuedCount, _ => issuedCount)
                .Generate();

    private static bool MatchesPromoCode(
        PromoCode promoCode,
        PromoCodeCreateRequest request,
        Partner partner,
        Preference preference,
        int expectedCustomerCount) =>
            promoCode.Code == request.Code &&
            promoCode.ServiceInfo == request.ServiceInfo &&
            promoCode.Partner == partner &&
            promoCode.Preference == preference &&
            promoCode.BeginDate == request.BeginDate.UtcDateTime &&
            promoCode.EndDate == request.EndDate.UtcDateTime &&
            promoCode.CustomerPromoCodes.Count == expectedCustomerCount &&
            promoCode.CustomerPromoCodes.All(cpc => cpc.PromoCodeId == promoCode.Id);
}
