using System;
using System.Collections.Generic;
using System.Threading;

namespace Task2
{
    public class EventBus
    {
        private Dictionary<string, Dictionary<int, List<Delegate>>> eventHandlers = new Dictionary<string, Dictionary<int, List<Delegate>>>();
        private Dictionary<string, DateTime> lastEventTime = new Dictionary<string, DateTime>();
        private int throttlingTime;

        public EventBus(int throtTime)
        {
            throttlingTime = throtTime;
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
                if (!lastEventTime.TryGetValue(eventName, out minEventTime) || (DateTime.Now - minEventTime).TotalMilliseconds >= throttlingTime)
                {
                    lastEventTime[eventName] = DateTime.Now;
                    foreach (int priority in eventHandlers[eventName].Keys)
                    {
                        foreach (Delegate eventHandler in eventHandlers[eventName][priority])
                        {
                            eventHandler.DynamicInvoke(sender, args);
                        }
                    }
                }
            }
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
            EventBus eventBus = new EventBus(2000);
            PrioritySubscriber sub1 = new PrioritySubscriber(1);
            PrioritySubscriber sub2 = new PrioritySubscriber(2);
            PrioritySubscriber sub3 = new PrioritySubscriber(3);
            PrioritySubscriber sub4 = new PrioritySubscriber(4);
            eventBus.Register("EvenEvent", 1, new EventHandler(sub1.HandleEvent));
            eventBus.Register("EvenEvent", 2, new EventHandler(sub2.HandleEvent));
            eventBus.Register("OddEvent", 3, new EventHandler(sub3.HandleEvent));
            eventBus.Register("OddEvent", 4, new EventHandler(sub4.HandleEvent));
            Publisher publisher = new Publisher(eventBus);
            publisher.SendEvent();
        }
    }
}
