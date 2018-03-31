using FluentScheduler;
using System;
using Topshelf;

namespace TestTopShelf_Service
{
    public class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x => {
                x.Service<IWindowsService>(s =>
                {
                    s.ConstructUsing(name => new WindowsService(new SchedulerRegistry(new Worker())));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();

                x.SetDescription("Test");
                x.SetDisplayName("Test Service");
                x.SetServiceName("Testservice");

                x.StartAutomatically();

                x.EnableServiceRecovery(s =>
                {
                    s.RestartService(1);
                    s.RestartService(2);
                });
            });
        }
    }

    public class SchedulerRegistry : Registry
    {
        public SchedulerRegistry(Worker worker)
        {
            Schedule(() =>
            {
                try
                {
                    worker.Run();
                }
                catch (Exception ex)
                {

                    throw;
                }
            }).NonReentrant().ToRunNow().AndEvery(10).Seconds();
        }
    }

    public interface IWindowsService
    {
        void Start();
        void Stop();
    }

    public class WindowsService : IWindowsService
    {
        public WindowsService(SchedulerRegistry registry)
        {
            JobManager.Initialize(registry);
        }

        public void Start()
        {
            Console.WriteLine("Service started");
        }

        public void Stop()
        {
            JobManager.StopAndBlock();
            Console.WriteLine("Service stopped");
        }
    }

    public class Worker
    {
        public void Run()
        {
            Console.WriteLine("Running job...");
        }
    }
}
