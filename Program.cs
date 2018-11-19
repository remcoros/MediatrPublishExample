using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MediatRPublishExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddScoped<ServiceFactory>(p => p.GetService);
            services.AddSingleton<IMediator, CustomMediator>();

            services.AddTransient<INotificationHandler<PingMe>, PingMeOne>();
            services.AddTransient<INotificationHandler<PingMe>, PingMeTwo>();
            services.AddTransient<INotificationHandler<PingMe>, PingMeThree>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            try
            {
                await mediator.Publish(new PingMe());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class CustomMediator : Mediator
    {
        public CustomMediator(ServiceFactory serviceFactory) : base(serviceFactory)
        {
        }

        protected override Task PublishCore(IEnumerable<Task> allHandlers)
        {
            // Override the default implementation so the handlers run in parallel.
            // Also, if an exception occurs while one handler is executing, it will
            // not prevent other handlers from running.
            return Task.WhenAll(allHandlers);
        }
    }

    public class PingMe : INotification
    {
    }

    public class PingMeOne : INotificationHandler<PingMe>
    {
        public Task Handle(PingMe notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("PingMeOne");
            throw new System.NotImplementedException();
        }
    }

    public class PingMeTwo : INotificationHandler<PingMe>
    {
        public async Task Handle(PingMe notification, CancellationToken cancellationToken)
        {

            await Task.Delay(1000, cancellationToken);

            Console.WriteLine("PingMeTwo");
        }
    }

    public class PingMeThree : INotificationHandler<PingMe>
    {
        public Task Handle(PingMe notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("PingMeThree");

            Task.Delay(100);

            Console.WriteLine("PingMeThree After");

            return Task.CompletedTask;
        }
    }
}
