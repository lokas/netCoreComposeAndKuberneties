using DemoApi.RestHateos;
using Microsoft.Extensions.DependencyInjection;
using RiskFirst.Hateoas;
using RiskFirst.Hateoas.Models;

namespace DemoApi
{
    public static class LinkServiceExtensions
    {
        public static void SetupLinks(this IServiceCollection services)
        {
            services.AddLinks(c =>
            {
                //   c.UseRelativeHrefs();

                c.AddPolicy<Customer>(policy =>
                {
                    policy.RequireRoutedLink("self", "GetValueByIdRoute", x => new { id = x.Id });
                });

                c.AddPolicy<Customer>("FullInfoPolicy",
                    c => c
                        .RequireSelfLink()
                        .RequireRoutedLink("create", "InsertValueRoute", a => a.Id)
                        .RequireRoutedLink("all", "GetAllValuesRoute", a => a.Id));

                c.AddPolicy<ItemsLinkContainer<Customer>>(policy =>
                {
                    policy.RequireSelfLink(); //TODO: ask does this works?
                    //.RequiresPagingLinks("me","next","previous");
                });
            });
        }
    }
}