using NetArchTest.Rules;

namespace CleanCut.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainNamespace = "CleanCut.Domain";
    private const string ApplicationNamespace = "CleanCut.Application";
    private const string InfrastructureNamespace = "CleanCut.Infrastructure";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        // Arrange
        var assembly = typeof(CleanCut.Domain.Common.BaseEntity).Assembly;

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(ApplicationNamespace, InfrastructureNamespace)
            .GetResult();

        // Assert
        Assert.True(testResult.IsSuccessful, 
            $"Domain layer should not depend on other layers. Violations: {string.Join(", ", testResult.FailingTypes?.Select(t => t.Name) ?? [])}");
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOnInfrastructure()
    {
        // Arrange
        var assembly = typeof(CleanCut.Application.DependencyInjection).Assembly;

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        Assert.True(testResult.IsSuccessful, 
            $"Application layer should not depend on Infrastructure layer. Violations: {string.Join(", ", testResult.FailingTypes?.Select(t => t.Name) ?? [])}");
    }

    [Fact]
    public void Domain_Entities_Should_InheritFromBaseEntity()
    {
        // Arrange
        var assembly = typeof(CleanCut.Domain.Common.BaseEntity).Assembly;

        // Act
        var testResult = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Entities")
            .Should()
            .Inherit(typeof(CleanCut.Domain.Common.BaseEntity))
            .GetResult();

        // Assert
        Assert.True(testResult.IsSuccessful, 
            $"All domain entities should inherit from BaseEntity. Violations: {string.Join(", ", testResult.FailingTypes?.Select(t => t.Name) ?? [])}");
    }

    [Fact]
    public void All_Classes_Should_Have_Correct_Namespace()
    {
        // Simple test to ensure the architecture test framework is working
        var assembly = typeof(CleanCut.Domain.Common.BaseEntity).Assembly;

        var testResult = Types
            .InAssembly(assembly)
            .Should()
            .ResideInNamespaceStartingWith(DomainNamespace)
            .GetResult();

        Assert.True(testResult.IsSuccessful);
    }
}
