using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Application.Common.Jobs
{
    public class EmailJob
    {
        public void SendWelcomeEmail(string email)
        {
            // placeholder — will connect to real email service later
            Console.WriteLine($"Sending welcome email to {email} at {DateTime.UtcNow}");
        }
    }
}
