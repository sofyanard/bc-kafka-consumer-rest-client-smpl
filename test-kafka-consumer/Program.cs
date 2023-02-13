using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using System.Net;
using System;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using test_kafka_consumer;
using System.Reflection.Metadata;

public class Program
{
    private static void Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Program is starting...");

        var appConfig = new ConfigurationBuilder().AddJsonFile($"appsettings.json").Build();

        string topic = appConfig["kafka:topic"];
        string groupId = appConfig["kafka:group-id"];
        string bootstrapServers = appConfig["kafka:bootstrap-server"];
        string saslUsername = appConfig["kafka:sasl-username"];
        string saslPassword = appConfig["kafka:sasl-password"];

        var consumerConfig = new ConsumerConfig
        {
            GroupId = groupId,
            BootstrapServers = bootstrapServers,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = saslUsername,
            SaslPassword = saslPassword,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        logger.LogInformation($"ConsumerConfig: {JsonConvert.SerializeObject(consumerConfig)}");

        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };

        using (var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build())
        {
            logger.LogInformation($"Creating Consumer...");

            consumer.Subscribe(topic);
            logger.LogInformation($"Subscribing topic: {topic}");

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = consumer.Consume(cts.Token);
                        // Console.WriteLine($"Consumed message '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                        logger.LogInformation($"Consuming: {JsonConvert.SerializeObject(cr)}");
                        logger.LogInformation($"Message Received: {cr.Value}");

                        Thread postThread = new Thread(() => { Post(cr.Value); });
                        postThread.Start();
                    }
                    catch (ConsumeException ce)
                    {
                        logger.LogError($"ConsumeException: {ce.Message}");
                    }
                }
            }
            catch (OperationCanceledException xe)
            {
                logger.LogError($"OperationCanceledException: {xe.Message}");
            }
            catch (Exception e)
            {
                logger.LogError($"Exception: {e.Message}");
            }
            finally
            {
                consumer.Close();
            }
        }

        logger.LogInformation("Program has finished.");
    }

    private static async Task Post(string message)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger<Program>();

        var appConfig = new ConfigurationBuilder().AddJsonFile($"appsettings.json").Build();
        string host = appConfig["apisvc:url"];

        HttpClient _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Employee employee = new Employee(0, message, null, message);

        HttpContent body = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");

        try
        {
            logger.LogInformation($"Sending: {JsonConvert.SerializeObject(employee)}");
            HttpResponseMessage response = await _client.PostAsync(host, body);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation($"Response Status: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(content))
                {
                    logger.LogInformation($"Response Content: {content}");
                }
            }
            else
            {
                logger.LogWarning($"Response Status: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            logger.LogError($"Exception: {e.Message}");
        }
    }
}
