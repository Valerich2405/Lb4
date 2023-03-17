using System;
using System.Collections.Generic;
using System.Threading;

namespace Task1
{
    public class EventBus
    {
        private Dictionary<string, List<Delegate>> eventHandlers = new Dictionary<string, List<Delegate>>();
        private Dictionary<string, DateTime> lastEventTime = new Dictionary<string, DateTime>();
        private int throttlingTime;

        public EventBus(int throtTime)
        {
            throttlingTime = throtTime;
        }

        public void Register(string eventName, Delegate eventHandler)
        {
            if (!eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = new List<Delegate>();
            }
            eventHandlers[eventName].Add(eventHandler);
        }

        public void Unregister(string eventName, Delegate eventHandler)
        {
            if (eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName].Remove(eventHandler);
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
                    foreach (Delegate eventHandler in eventHandlers[eventName])
                    {
                        eventHandler.DynamicInvoke(sender, args);
                    }
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            EventBus eventBus = new EventBus(2000);

            eventBus.Register("NewEvent", new EventHandler(Handler));

            for (int i = 0; i < 10; i++)
            {
                eventBus.Send("NewEvent", null, EventArgs.Empty);
                Thread.Sleep(1000);
            }

            eventBus.Unregister("NewEvent", new EventHandler(Handler));
        }

        static void Handler(object sender, EventArgs args)
        {
            Console.WriteLine("Loading...");
        }
    }
}

