using DemoApi.RestHateos;
using Microsoft.Extensions.DependencyInjection;
using RiskFirst.Hateoas;

namespace DemoApi
{
    public static class LinkServiceExtensions
    {
        public static void SetupLinks(this IServiceCollection services)
        {
            services.AddLinks(c =>
                c.AddPolicy<Customer>(
                    c => c
                        .RequireSelfLink()
                        .RequireRoutedLink("all", "all", a => a.Id)));

            //.RequireRoutedLink("all", "Customers")));
        }
    }
}
