using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

// Single Responsibility Principle (SRP)
interface ILoggerService
{
    void Log(string message);
}

class LoggerService : ILoggerService
{
    public void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now}] Log: {message}");
    }
}

// Open/Closed Principle (OCP)
interface IPaymentMethod
{
    void ProcessPayment(decimal amount);
}

class CreditCardPayment : IPaymentMethod
{
    public void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing credit card payment of ${amount}");
    }
}

class PayPalPayment : IPaymentMethod
{
    public void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing PayPal payment of ${amount}");
    }
}

// Factory Pattern - Payment Factory
class PaymentFactory
{
    public static IPaymentMethod CreatePaymentMethod(string type)
    {
        switch (type)
        {
            case "CreditCard":
                return new CreditCardPayment();
            case "PayPal":
                return new PayPalPayment();
            default:
                throw new ArgumentException("Invalid payment type");
        }
    }
}

// Repository Pattern - Order Repository
interface IOrderRepository
{
    void SaveOrder(Order order);
    List<Order> GetOrders();
}

class OrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new List<Order>();

    public void SaveOrder(Order order)
    {
        _orders.Add(order);
        Console.WriteLine("Order saved successfully.");
    }

    public List<Order> GetOrders()
    {
        return _orders;
    }
}

// Order Model
class Order
{
    public decimal Amount { get; set; }
    public string PaymentType { get; set; }
}

// Dependency Inversion Principle (DIP)
class OrderProcessor
{
    private readonly IPaymentMethod _paymentMethod;
    private readonly ILoggerService _logger;
    private readonly IOrderRepository _orderRepository;

    public OrderProcessor(IPaymentMethod paymentMethod, ILoggerService logger, IOrderRepository orderRepository)
    {
        _paymentMethod = paymentMethod;
        _logger = logger;
        _orderRepository = orderRepository;
    }

    public void ProcessOrder(Order order)
    {
        if (order.Amount > 100)
        {
            order.Amount *= 0.9m;
            Console.WriteLine("A 10% discount has been applied!");
        }

        _logger.Log("Processing order...");
        _paymentMethod.ProcessPayment(order.Amount);
        _orderRepository.SaveOrder(order);
        _logger.Log("Order processed.");
    }
}

class Program
{
    static void Main()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ILoggerService, LoggerService>()
            .AddSingleton<IOrderRepository, OrderRepository>()
            .BuildServiceProvider();

        var logger = serviceProvider.GetService<ILoggerService>();
        var orderRepository = serviceProvider.GetService<IOrderRepository>();

        while (true)
        {
            Console.WriteLine("Choose an option: \n1. Make a Payment \n2. View Transaction History \n3. Exit");
            string choice = Console.ReadLine();

            if (choice == "3")
                break;

            switch (choice)
            {
                case "1":
                    Console.Write("Enter payment type (CreditCard/PayPal): ");
                    string paymentType = Console.ReadLine();

                    Console.Write("Enter order amount: ");
                    decimal amount;
                    while (!decimal.TryParse(Console.ReadLine(), out amount))
                    {
                        Console.Write("Invalid input. Enter a valid amount: ");
                    }

                    try
                    {
                        IPaymentMethod paymentMethod = PaymentFactory.CreatePaymentMethod(paymentType);
                        var orderProcessor = new OrderProcessor(paymentMethod, logger, orderRepository);
                        var order = new Order { Amount = amount, PaymentType = paymentType };
                        orderProcessor.ProcessOrder(order);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    break;

                case "2":
                    Console.WriteLine("\nTransaction History:");
                    foreach (var order in orderRepository.GetOrders())
                    {
                        Console.WriteLine($"- Payment: {order.PaymentType}, Amount: ${order.Amount}");
                    }
                    break;

                default:
                    Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
                    break;
            }
        }
    }
}
