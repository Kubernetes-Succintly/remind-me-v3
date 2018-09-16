﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace remind_me_cron_job
{
    public class Bootstrapper
    {
        private readonly string jwtToken;
        private readonly ISmtpClient smtpClient;
        private readonly IRestClient client;

        public Bootstrapper(IRestClient client, ISmtpClient smtpClient, string jwtToken)
        {
            this.client = client;
            this.smtpClient = smtpClient;
            this.jwtToken = jwtToken;
        }

        private void SendMail(IEnumerable<Reminder> remindersToSend)

        {
            foreach (var reminder in remindersToSend)
            {
                var messageBody =
                    "<h1>Remind Me Mail Service</h1>" +
                    $"<p>I would like to remind you about: {reminder.Title}" +
                    $" which is due on {reminder.DueDate:D}.</p>" +
                    "<p>-Remind Me Mail Bot</p>";
                var message = new MailMessage
                {
                    IsBodyHtml = true,
                    From = new MailAddress("kubernetessuccinctly@outlook.com", "Remind Me Mail Bot")
                };
                message.To.Add(new MailAddress("kubernetessuccinctly@outlook.com"));
                message.Subject = "You have a reminder scheduled for today!";
                message.Body = messageBody;
                this.smtpClient.Send(message);
            }
        }

        public void Start(string[] args)
        {
            var request = new RestRequest("/api/Reminder/all", Method.GET);
            request.AddHeader("Authorization", string.Format("bearer {0}", this.jwtToken));
            var response = this.client.Execute<List<Reminder>>(request);
            if (null == response.Data)
            {
                return;
            }

            var remindersToSend = response.Data.Where(r => r.DueDate.Date == DateTime.Today.Date);
            this.SendMail(remindersToSend);
        }
    }
}