﻿//    Copyright 2013 Deon Heyns
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using Common.Logging;
using ScriptBot.Core.Contracts;
using ScriptBot.Core.Messages;
using SkypeCom = SKYPE4COMLib;

namespace ScriptBot.Skype
{
    public class SkypeTransport : ITransport
    {
        public string Trigger { get { return _trigger; } }
        private readonly string _trigger;
        private readonly ILog _logger;
        private bool _disposed;
        private SkypeCom.Skype _skype;

        public SkypeTransport(string trigger, ILog logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (trigger == null) throw new ArgumentNullException("trigger");
            _logger = logger;
            _logger.Info(string.Format("Set Trigger for SkypeTransport to: {0}", _trigger));
            _trigger = trigger;
           
        }

        public void Listen(Func<IncomingMessage, ReplyMessage> callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");

            _skype = new SkypeCom.Skype();
            _skype.Attach(7, false);
            _skype.MessageStatus += (message, status) =>
            {
                if (message.Body.IndexOf(_trigger, StringComparison.Ordinal) == 0 && status == SkypeCom.TChatMessageStatus.cmsReceived)
                {
                    _logger.Info(string.Format("Incoming message received from: {0}", message.FromDisplayName));
                    SkypeCom.IChat ichat = _skype.Chat[message.Chat.Name];
                    _logger.Info(string.Format("Incoming message is: {0}", message.Body));
                    var msg = new IncomingMessage { Message = message.Body.ToLowerInvariant() };
                    var results = callback(msg);
                    _logger.Info(string.Format("Message received back from callback is: {0}", results.Message));
                    ichat.SendMessage(string.Format("Bot says:{0} {1}", Environment.NewLine, results.Message));
                }
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _skype = null;
            }
            _disposed = true;
        }
    }

}
