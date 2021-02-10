using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;

namespace Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<KafkaConsumerService>();
            services.AddScoped<INewsItemEventHandler, NewsItemEventHandler>();

            services.AddScoped(ServiceProvider =>
            {
                var nodeUri = new Uri("http://elastic:9200");
                var settings = new ConnectionSettings(nodeUri);
                var client = new ElasticClient(settings);

                settings.DefaultMappingFor<NewsItem>(
                    x => x.IdProperty(x => x.Id).IndexName("newsitems"));
                
                settings.DefaultMappingFor<NewsItemSubscription>(
                    x => x.IdProperty(x => x.Id).IndexName("subscriptions"));

                return client;
            });

            services.AddScoped<INewsItemRepository, NewsItemRepository>();
            services.AddScoped<INewsItemSubscriptionRepository, NewsItemSubscriptionRepository>();
            services.AddScoped<INewsItemSubscriptionManager, NewsItemSubscriptionManager>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            ConfigureIndices(app);
        }

        private void ConfigureIndices(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var elasticClient = scope.ServiceProvider.GetRequiredService<ElasticClient>();

            elasticClient.Indices.Delete("subscriptions");
            elasticClient.Indices.Delete("newsitems");

            if (!elasticClient.Indices.Exists("newsitems").Exists)
            {
                elasticClient.Indices.Create("newsitems", request =>
                {
                    return request
                        .Map<NewsItem>(mm => mm.Properties(props => props
                            .Text(x => x.Name(x => x.Body))
                            .Text(x => x.Name(x => x.Title))))
                        .Settings(settings => settings
                            .NumberOfShards(1)
                            .NumberOfReplicas(1)
                            .Setting("lifecycle.name", "cleanup-newsitems"));
                });
            }

            if (!elasticClient.Indices.Exists("subscriptions").Exists)
            {
                // PLEASE NOTE: We're mapping extra properties that belong to the news item here.
                // This is needed for the percolator to actually work as specified. You need to make 
                // sure that the types of the fields match the ones used in the original index!
                elasticClient.Indices.Create("subscriptions", request =>
                {
                    return request
                        .Map<NewsItemSubscription>(mm => mm
                            .Properties(props => props
                                .Percolator(x => x.Name(y => y.Query))
                                .Text(p=>p.Name("body"))
                                .Text(p=>p.Name("title"))
                            )
                        )
                        .Settings(settings => settings
                            .NumberOfShards(1)
                            .NumberOfReplicas(1)
                            .Setting("lifecycle.name", "cleanup-subscriptions")
                        );
                });

                // This lifecycle policy automatically removes items older than 1 day.
                // You need to attach this to an index using the lifecycle.name setting.
                elasticClient.IndexLifecycleManagement.PutLifecycle("cleanup-subscriptions", policy =>
                    policy.Policy(p => p.Phases(
                            phases => phases.Delete(delete => delete
                                .MinimumAge("1d")
                                .Actions(actions => actions.Delete(x => x))
                            )
                        )
                    ));

                // This lifecycle automatically removes newsitems older than 1 year.
                // You need to attach this to an index using the lifecycle.name setting.
                elasticClient.IndexLifecycleManagement.PutLifecycle("cleanup-newsitems", policy =>
                    policy.Policy(p => p.Phases(
                            phases => phases.Delete(delete => delete
                                .MinimumAge("365d")
                                .Actions(actions => actions.Delete(x => x))
                            )
                        )
                    ));
            }
        }
    }
}