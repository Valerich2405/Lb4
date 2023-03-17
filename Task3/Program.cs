using System;
using System.Collections.Generic;
using System.Threading;

namespace Task3
{
    public class EventBus
    {
        private Dictionary<string, Dictionary<int, List<Delegate>>> eventHandlers = new Dictionary<string, Dictionary<int, List<Delegate>>>();
        private Dictionary<string, DateTime> lastEventTime = new Dictionary<string, DateTime>();
        private readonly int maxRetries;
        private readonly int minDelay;
        private readonly int maxDelay;
        private readonly double delayMultiplier;

        public EventBus(int maxRetries, int minDelay, int maxDelay, double delayMultiplier)
        {
            this.maxRetries = maxRetries;
            this.minDelay = minDelay;
            this.maxDelay = maxDelay;
            this.delayMultiplier = delayMultiplier;
        }

        public void Register(string eventName, int priority, Delegate eventHandler)
        {
            if (!eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = new Dictionary<int, List<Delegate>>();
            }
            if (!eventHandlers[eventName].ContainsKey(priority))
            {
                eventHandlers[eventName][priority] = new List<Delegate>();
            }
            eventHandlers[eventName][priority].Add(eventHandler);
        }

        public void Unregister(string eventName, int priority, Delegate eventHandler)
        {
            if (eventHandlers.ContainsKey(eventName))
            {
                if (eventHandlers[eventName].ContainsKey(priority))
                {
                    eventHandlers[eventName][priority].Remove(eventHandler);
                }
            }
        }

        public void Send(string eventName, object sender, EventArgs args)
        {
            if (eventHandlers.ContainsKey(eventName))
            {
                DateTime minEventTime;
                if (!lastEventTime.TryGetValue(eventName, out minEventTime) || (DateTime.Now - minEventTime).TotalMilliseconds >= GetDelay())
                {
                    lastEventTime[eventName] = DateTime.Now;
                    for (int retries = 0; retries < maxRetries; retries++)
                    {
                        foreach (int priority in eventHandlers[eventName].Keys)
                        {
                            foreach (Delegate eventHandler in eventHandlers[eventName][priority])
                            {
                                eventHandler.DynamicInvoke(sender, args);
                            }
                        }

                        if ((DateTime.Now - lastEventTime[eventName]).TotalMilliseconds >= GetDelay())
                        {
                            break;
                        }
                        else
                        {
                            Thread.Sleep(GetDelay());
                        }
                    }
                }
            }
        }

        private int GetDelay()
        {
            int currentDelay = minDelay;
            if (maxDelay > minDelay)
            {
                Random random = new Random();
                currentDelay = (int)(currentDelay * delayMultiplier * (1 + random.NextDouble()));
                currentDelay = Math.Min(currentDelay, maxDelay);
            }
            return currentDelay;
        }
    }

    public class Publisher
    {
        private EventBus eventBus;

        public Publisher(EventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void SendEvent()
        {
            for (int i = 1; i <= 10; i++)
            {
                if (i % 2 == 0)
                {
                    eventBus.Send("EvenEvent", this, EventArgs.Empty);
                }
                else
                {
                    eventBus.Send("OddEvent", this, EventArgs.Empty);
                }
                Thread.Sleep(2000);
            }
        }
    }

    public class PrioritySubscriber
    {
        public int Priority { get; set; }

        public PrioritySubscriber(int priority)
        {
            Priority = priority;
        }
        public void HandleEvent(object sender, EventArgs args)
        {
            Console.WriteLine($"Priority {Priority} subscriber handling event");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            EventBus eventBus = new EventBus(5, 1000, 5000, 1.5);
            Publisher publisher = new Publisher(eventBus);

            PrioritySubscriber sub1 = new PrioritySubscriber(1);
            PrioritySubscriber sub2 = new PrioritySubscriber(2);
            PrioritySubscriber sub3 = new PrioritySubscriber(3);

            eventBus.Register("OddEvent", sub1.Priority, new Action<object, EventArgs>(sub1.HandleEvent));
            eventBus.Register("EvenEvent", sub2.Priority, new Action<object, EventArgs>(sub2.HandleEvent));
            eventBus.Register("OddEvent", sub3.Priority, new Action<object, EventArgs>(sub3.HandleEvent));


            publisher.SendEvent();

            Console.ReadLine();
        }
    } 
}

