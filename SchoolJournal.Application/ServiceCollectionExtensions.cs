using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Application.Common.Behaviors;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Application.Common.Services;

namespace SchoolJournal.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        services.AddScoped<IAuditContext, AuditContext>();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(AuditBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddMapster();

        return services;
    }
}