using System;
using System.Collections.Generic;
using System.Threading;

namespace Task2
{
    public class Warehouse
    {
        public delegate void DeliveryHandler(string message, Priority priority);
        private DeliveryHandler notify;
        private int currentLoad;

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
                notify?.Invoke($"Кiлькiсть товарiв на складi змiнено i дорiвнює: {currentLoad}", Priority.Low);
            }
        }

        public void Add(int quantity)
        {
            if (CurrentLoad + quantity <= 50)
            {
                CurrentLoad += quantity;
                notify?.Invoke($"Додано товарiв на склад: {quantity}", Priority.High);
            }
            else
            {
                notify?.Invoke($"На складi недостатньо мiсця. Не вдалося додати: {quantity}", Priority.Low);
            }
        }

        public void Remove(int quantity)
        {
            if (CurrentLoad - quantity >= 0)
            {
                CurrentLoad -= quantity;
                notify?.Invoke($"Вiдвантажено товарiв зi складу: {quantity}", Priority.High);
            }
            else
            {
                notify?.Invoke($"На складi недостатньо товарiв для вiдвантаження: {quantity}", Priority.Low);
            }
        }
    }

    public enum Priority
    {
        Low,
        High
    }

    public class Subscriber
    {
        private Priority priority;

        public Subscriber(Priority priority)
        {
            this.priority = priority;
        }

        public void Subscribe(Warehouse warehouse)
        {
            warehouse.Notify += ReceiveMessage;
        }

        public void Unsubscribe(Warehouse warehouse)
        {
            warehouse.Notify -= ReceiveMessage;
        }

        private void ReceiveMessage(string message, Priority priority)
        {
            if (priority == this.priority)
            {
                Thread.Sleep(1000);
                Console.WriteLine(message);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Warehouse warehouse = new Warehouse();
            Subscriber lowPrioritySubscriber = new Subscriber(Priority.Low);
            Subscriber highPrioritySubscriber = new Subscriber(Priority.High);

            lowPrioritySubscriber.Subscribe(warehouse);
            highPrioritySubscriber.Subscribe(warehouse);

            warehouse.Add(10);
            warehouse.Add(20);
            warehouse.Add(20);
            warehouse.Add(20);
            warehouse.Remove(5);
            warehouse.Remove(25);

            lowPrioritySubscriber.Unsubscribe(warehouse);
            highPrioritySubscriber.Unsubscribe(warehouse);

            Console.ReadKey();
        }
    }
}
