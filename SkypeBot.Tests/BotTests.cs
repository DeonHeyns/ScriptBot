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
using NUnit.Framework;

namespace SkypeBot.Tests
{
    [TestFixture(Category = "CATEGORY", Description = "DESCRIPTION")]
    public class BotTests 
    {
        [TestAttribute]
        public void ProcessCommand_EmptyString_ReturnsCouldntFindAnyRelatedCommands()
        {
            // Arrange
            Bot bot = new Bot();

            // Act
            string results = bot.HandleCommand(string.Empty);

            // Assert
            Assert.IsTrue(results.Equals("couldn't find any related commands!\r\n", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void ProcessCommand_Scriptcs_OutputIsNotShownOnScreen()
        {
            // Arrange
            Bot bot = new Bot();

            // Act
            string results =bot.HandleCommand(@"scriptcs helloworld.csx");


            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(results));
        }

        [TestAttribute]
        public void ProcessCommand_ScriptcsFileNotFound_ReturnsCouldntFindScriptcsFile()
        {
            // Arrange
            Bot bot = new Bot();

            // Act
            string results = bot.HandleCommand(@"scriptcs random.csx");

            // Assert
            Assert.IsTrue(results.Equals("Sorry couldn't find the scriptcs file random.csx.\r\n", StringComparison.OrdinalIgnoreCase));
        }
    }
}