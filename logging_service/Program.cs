using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace logging_service
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();

            factory.UserName = "guest";
            factory.Password = "guest";
            factory.HostName = "localhost";
            string exchangeName = "order_exchange";
            string queueName = "logging_queue";
            string routingKey = "create_order";

            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
                    channel.QueueDeclare(queueName, true, false, false, null);
                    channel.QueueBind(queueName, exchangeName, routingKey, null);

                    var consumer = new EventingBasicConsumer(channel);
                    
                    consumer.Received += (ch, ea) =>
                    {
                        Console.WriteLine("--------- Before Receiving -----------");
                        Console.WriteLine(ch);
                        Console.WriteLine(ea);
                        var body = ea.Body.ToArray();
                        // var message = Encoding.UTF8.GetString(body);
                        // copy or deserialise the payload
                        // and process the message
                        // ...
                        channel.BasicAck(ea.DeliveryTag, false);
                        Console.WriteLine("--------- After Receiving ------------");
                    };

                    channel.BasicConsume(queueName, false, consumer);
                    Console.WriteLine("Press [enter] to exit.");
                    // This is necessary for the program not to quit immediately
                    Console.ReadLine();
                }
            }
        }
    }
}
