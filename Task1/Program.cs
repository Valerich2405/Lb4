using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Task1
{
    class Warehouse
    {
        public delegate void DeliveryHandler(string message);
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
                notify?.Invoke($"Кiлькiсть товарiв на складi змiнено i дорiвнює: {currentLoad}");
            }
        }

        public void Add(int quantity)
        {
            if (CurrentLoad + quantity <= 50)
            {
                CurrentLoad += quantity;
                notify?.Invoke($"Додано товарiв на склад: {quantity}");
            }
            else
            {
                notify?.Invoke($"На складi недостатньо мiсця. Не вдалося додати: {quantity}");
            }
        }

        public void Remove(int quantity)
        {
            if (CurrentLoad - quantity >= 0)
            {
                CurrentLoad -= quantity;
                notify?.Invoke($"Вiдвантажено товарiв зi складу: {quantity}");
            }
            else
            {
                notify?.Invoke($"На складi недостатньо товарiв для вiдвантаження: {quantity}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Warehouse warehouse = new Warehouse();
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
}