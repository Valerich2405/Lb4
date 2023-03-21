using System;

namespace Task4
{
    public class Workflow
    {
        public event EventHandler<ActionEventArgs> ActionAdded;
        public event EventHandler<ActionEventArgs> ActionCompleted;
        public event EventHandler WorkflowCompleted;

        private enum State
        {
            AddAction,
            ExecuteAction,
            WorkflowComplete
        }

        private State currentState = State.AddAction;

        public void Run()
        {
            var action1 = new Action("Action 1");
            var action2 = new Action("Action 2");
            var action3 = new Action("Action 3");

            AddAction(action1);
            AddAction(action2);
            AddAction(action3);

            while (currentState != State.WorkflowComplete)
            {
                switch (currentState)
                {
                    case State.AddAction:
                        currentState = State.ExecuteAction;
                        break;
                    case State.ExecuteAction:
                        action1.Execute();
                        action2.Execute();
                        action3.Execute();
                        currentState = State.WorkflowComplete;
                        break;
                }
            }

            OnWorkflowCompleted();
        }

        private void AddAction(Action action)
        {
            OnActionAdded(action);
            OnActionCompleted(action);
        }

        protected virtual void OnActionAdded(Action action)
        {
            ActionAdded?.Invoke(this, new ActionEventArgs(action));
        }

        protected virtual void OnActionCompleted(Action action)
        {
            ActionCompleted?.Invoke(this, new ActionEventArgs(action));
        }

        protected virtual void OnWorkflowCompleted()
        {
            WorkflowCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    public class Action
    {
        public string Name { get; set; }

        public Action(string name)
        {
            Name = name;
        }

        public void Execute()
        {
            Console.WriteLine($"Executing action '{Name}'...");
            Console.WriteLine();
        }
    }

    public class ActionEventArgs : EventArgs
    {
        public Action Action { get; set; }

        public ActionEventArgs(Action action)
        {
            Action = action;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Workflow started");
            Console.WriteLine();

            var workflow = new Workflow();
            workflow.ActionAdded += OnActionAdded;
            workflow.ActionCompleted += OnActionCompleted;
            workflow.WorkflowCompleted += OnWorkflowCompleted;

            workflow.Run();

            Console.WriteLine("Workflow ended");
        }

        private static void OnActionAdded(object sender, ActionEventArgs a)
        {
            Console.WriteLine($"Action '{a.Action.Name}' added to the workflow");
            Console.WriteLine();
        }

        private static void OnActionCompleted(object sender, ActionEventArgs a)
        {
            Console.WriteLine($"Action '{a.Action.Name}' completed");
            Console.WriteLine();
        }

        private static void OnWorkflowCompleted(object sender, EventArgs a)
        {
            Console.WriteLine("Workflow completed");
            Console.WriteLine();
        }
    }
}