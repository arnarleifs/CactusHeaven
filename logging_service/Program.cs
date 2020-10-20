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

            // factory.DispatchConsumersAsync = true;
            IConnection conn = factory.CreateConnection();
            // try{
            //     conn = factory.CreateConnection();
            // }catch{
            //     Console.WriteLine("Connection failed");
            // };

            IModel channel = conn.CreateModel();
            // try{
            //     channel = conn.CreateModel();
            // }catch{
            //     Console.WriteLine("Model Creation Failed");
            // };

            string exchangeName = "order_exchange";
            string queueName = "logging_queue";
            string routingKey = "create_order";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);
            // try{
            //     channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
            //     channel.QueueDeclare(queueName, true, false, false, null);
            //     channel.QueueBind(queueName, exchangeName, routingKey, null);
            // }catch{
            //     Console.WriteLine("Somethingg Broke in declorations");
            // }

            var consumer = new EventingBasicConsumer(channel);
            // try{
            //     consumer = new EventingBasicConsumer(channel);
            // }catch{
            //     Console.WriteLine("Consumer Creation Failed");
            // }

            Console.WriteLine("--------- Before Receiving -----------");

            consumer.Received += (ch, ea) =>
                            {
                                Console.WriteLine(ch);
                                Console.WriteLine(ea);
                                var body = ea.Body.ToArray();
                                // var message = Encoding.UTF8.GetString(body);
                                // copy or deserialise the payload
                                // and process the message
                                // ...
                                channel.BasicAck(ea.DeliveryTag, false);
                            };

            Console.WriteLine("--------- After Receiveig ------------");
            
            String consumerTag = channel.BasicConsume(queueName, false, consumer);
        }
    }
}
