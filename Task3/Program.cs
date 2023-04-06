using System.Threading;
using System;

class Warehouse
{
    public delegate void DeliveryHandler(string message);
    private DeliveryHandler notify;
    private int currentLoad;
    private int retryCount;
    private int initialDelayMs;
    private int maxDelayMs;
    private readonly Random random;

    public event DeliveryHandler Notify
    {
        add
        {
            notify += value;
            Console.WriteLine($"{value.Method.Name} додано");
        }
        remove
        {
            notify -= value;
            Console.WriteLine($"{value.Method.Name} видалено");
        }
    }

    public int CurrentLoad
    {
        get { return currentLoad; }
        private set
        {
            currentLoad = value;
            notify?.Invoke($"Кiлькiсть товарiв на складi змiнено i дорiвнює: {currentLoad}");
        }
    }

    public Warehouse(int retryCount, int initialDelayMs, int maxDelayMs)
    {
        this.retryCount = retryCount;
        this.initialDelayMs = initialDelayMs;
        this.maxDelayMs = maxDelayMs;
        this.random = new Random();
    }

    public void Add(int quantity)
    {
        if (TryAction(() =>
        {
            if (CurrentLoad + quantity <= 50)
            {
                CurrentLoad += quantity;
            }
            else
            {
                throw new InvalidOperationException($"На складi недостатньо мiсця. Не вдалося додати: {quantity}");
            }
        }, $"На складi недостатньо мiсця. Не вдалося додати: {quantity}"))
        {
            notify?.Invoke($"Додано товарiв на склад: {quantity}");
        }
    }

    public void Remove(int quantity)
    {
        if (TryAction(() =>
        {
            if (CurrentLoad - quantity >= 0)
            {
                CurrentLoad -= quantity;
            }
            else
            {
                throw new InvalidOperationException($"На складi недостатньо товарiв для вiдвантаження: {quantity}");
            }
        }, $"На складi недостатньо товарiв для вiдвантаження: {quantity}"))
        {
            notify?.Invoke($"Вiдвантажено товарiв зi складу: {quantity}");
        }
    }

    private bool TryAction(Action action, string failureMessage)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка на спробi {i + 1} з {retryCount}: {ex.Message}");
                if (i < retryCount - 1)
                {
                    int delay = random.Next(initialDelayMs, maxDelayMs);
                    Console.WriteLine($"Наступна спроба через {delay} мс");
                    Thread.Sleep(delay);
                }
            }
        }
        Console.WriteLine(failureMessage);
        return false;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Warehouse warehouse = new Warehouse(3, 1000, 5000);
        warehouse.Notify += ShowMessage;
        warehouse.Add(10);
        warehouse.Add(20);
        warehouse.Add(20);
        warehouse.Add(20);
        warehouse.Remove(5);
        warehouse.Remove(25);

        Console.ReadKey();
    }
    static void ShowMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Thread.Sleep(500);
            Console.WriteLine(message);
        }
    }
}