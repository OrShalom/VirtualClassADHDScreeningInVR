using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class QueueHandler : IDisposable
{
    private readonly string queueName;
    private readonly IConnection connection;
    private readonly IModel channel;

    public QueueHandler(string queueName)
    {
        this.queueName = queueName;
        // Create a connection factory and set the connection parameters
        var factory = new ConnectionFactory();
        factory.VirtualHost = "vjibybay";
        factory.Password = "nFjJmsci4nMvVBdTqu-e6_YE9hmHUA8z";
        factory.Port = 5672;
        factory.Uri = new Uri("amqps://vjibybay:nFjJmsci4nMvVBdTqu-e6_YE9hmHUA8z@cow.rmq2.cloudamqp.com/vjibybay");
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
    }

    public void Dispose()
    {
        connection.Dispose();
        channel.Dispose();
    }

    public void SendMessageToQ(string msg)
    {
        channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // Send the message
        var body = System.Text.Encoding.UTF8.GetBytes(msg);
        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);

    }

    public void SubscribeToQueue(EventHandler<BasicDeliverEventArgs> handler)
    {
        // Declare the queue
        channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // Set up a consumer to handle incoming messages
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += handler;
        channel.BasicConsume(queue: queueName,
                             autoAck: true,
                             consumer: consumer);
    }
}
