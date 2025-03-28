﻿namespace cleaner.services;

public class RabbitMqSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ExchangeName { get; set; }
    public string QueueName { get; set; }
    public List<string> RoutingKeys { get; set; } = new();
}