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
                //c.UseRelativeHrefs(); 
                //u can define ur own resource location 
                //c.ConfigureHrefTransformation(t=>t.Add(f=>f.ActionContext.ActionDescriptor.))
                //c.AddPolicy();

                c.AddPolicy<Customer>(policy =>
                {
                    policy.RequireRoutedLink("self", "GetByIdRoute", x => new { id = x.Id });
                });

                //FullInfoPolicy this needs to match for both 
                //e.g. if action on the order controller has FullInfoPolicy Link attribute 
                c.AddPolicy<Customer>("FullInfoPolicy",
                    c => c
                        .RequireRoutedLink("self", "GetByIdRoute", x => new { id = x.Id })
                        .RequireRoutedLink("create", "InsertValueRoute", a => a.Id)
                        .RequireRoutedLink("all", "GetAllValuesRoute", a => a.Id));

                c.AddPolicy<ItemsLinkContainer<Customer>>(policy =>
                {
                    policy.RequireSelfLink();
                    //TODO: ask does this works?
                    //.RequiresPagingLinks("me","next","previous");
                });


                c.AddPolicy<Order>(policy =>
                {
                    policy.RequireRoutedLink("self", "GetOrderByIdRoute",
                        x => new { id = x.Id, customerId = x.ForCustomerId })
                          .RequireRoutedLink("children", "GetOrderItemsByIdRoute",
                              x => new { id = x.Id, customerId = x.ForCustomerId });
                });

                //c.AddPolicy<OrderDetails>(policy =>
                //{
                //    policy.RequireRoutedLink("self", "GetOrderByIdRoute",
                //            x => new { id = x.Id, customerId = x.ForCustomerId })
                //        .RequireRoutedLink("children", "GetOrderItemsByIdRoute",
                //            x => new { id = x.Id, customerId = x.ForCustomerId });
                //});

                c.AddPolicy<Order>("FullInfoPolicy", policy =>
                {
                    policy
                        .RequireSelfLink()
                        .RequireRoutedLink("children", "GetOrderItemsByIdRoute",
                        x => new { id = x.Id, customerId = x.ForCustomerId });
                    //.RequireRoutedLink("parent", "GetByIdRoute",a => a.ForCustomerId);
                });
            });
        }
    }
}