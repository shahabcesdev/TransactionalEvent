using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Session04.TransactionalEvent.Dal;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Session04.TransactionalEvent.EventPublisher
{
    class Program
    {
        public static async Task Main()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host("rabbitmq://localhost");

                sbc.ReceiveEndpoint("person_queue", ep =>
                {

                });
            });
            var opb = new DbContextOptionsBuilder<PersonDB>();
            opb.UseSqlServer("Server =.; Database=PersonDb; MultipleActiveResultSets=true; Trusted_Connection=True");

            var context = new PersonDB(opb.Options);

            await bus.StartAsync(); // This is important!

            do
            {
                var events = context.OutBoxEvents.ToList();
                foreach (var outboxEvent in events)
                {
                    await bus.Publish(outboxEvent);
                    context.Remove(outboxEvent);
                    context.SaveChanges();
                    Console.WriteLine("Event published successfully...");
                }
                
                System.Threading.Thread.Sleep(10000);
            } while (true);
           



            await bus.StopAsync();
        }
    }
}
