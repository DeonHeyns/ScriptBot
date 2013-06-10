//    Copyright 2013 Deon Heyns
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
using System.ComponentModel.Composition;

namespace SkypeBot.Common.Commands
{
    [SkypeBotExport]
    [ExportMetadata("Command", CommandParameters.Time)]
    public class SkypeBotTimeCommand : ISkypeBotCommand
    {
        public string Process(params string[] args)
        {
            return string.Format("It is {0} UTC", DateTime.UtcNow.ToShortTimeString());
        }
    }

    [SkypeBotExport]
    [ExportMetadata("Command", CommandParameters.Date)]
    public class SkypeBotDateCommand : ISkypeBotCommand
    {
        public string Process(params string[] args)
        {
            return string.Format("It is {0} UTC", DateTime.UtcNow.ToShortDateString());
        }
    }

    [SkypeBotExport]
    [ExportMetadata("Command", CommandParameters.DateTime)]
    public class SkypeBotDateTimeCommand : ISkypeBotCommand
    {
        public string Process(params string[] args)
        {
            return string.Format("It is {0} UTC", DateTime.UtcNow);
        }
    }
}
