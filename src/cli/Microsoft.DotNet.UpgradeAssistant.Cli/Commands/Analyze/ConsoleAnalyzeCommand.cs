﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.UpgradeAssistant.Analysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.DotNet.UpgradeAssistant.Cli
{
    public class ConsoleAnalyzeCommand : UpgradeAssistantCommand
    {
        public ConsoleAnalyzeCommand()
            : base("analyze")
        {
            Handler = CommandHandler.Create<ParseResult, CommandOptions, CancellationToken>((result, options, token) =>
                Host.CreateDefaultBuilder()
                    .UseConsoleUpgradeAssistant<ConsoleAnalyze>(options, result)
                    .RunUpgradeAssistantAsync(token));

            if (FeatureFlags.IsAnalyzeFormatEnabled)
            {
                AddOption(new Option<string>(new[] { "--format", "-f" }, LocalizedStrings.UpgradeAssistantCommandFormat));

                AddCommand(new ListFormatsCommand());
            }
        }

        private class ListFormatsCommand : Command
        {
            public ListFormatsCommand()
                : base("list-formats")
            {
                Handler = CommandHandler.Create<ParseResult, CommandOptions, CancellationToken>((result, options, token) =>
                    Host.CreateDefaultBuilder()
                        .UseConsoleUpgradeAssistant<ListFormats>(options, result)
                        .RunUpgradeAssistantAsync(token));
            }

            private class ListFormats : IAppCommand
            {
                private readonly IEnumerable<IAnalyzeResultWriter> _writers;
                private readonly ILogger<ListFormats> _logger;

                public ListFormats(IEnumerable<IAnalyzeResultWriter> writers, ILogger<ListFormats> logger)
                {
                    _writers = writers;
                    _logger = logger;
                }

                public Task RunAsync(CancellationToken token)
                {
                    foreach (var writer in _writers)
                    {
                        _logger.LogInformation("Analysis format available: '{Format}'", writer.Format);
                    }

                    return Task.CompletedTask;
                }
            }
        }
    }
}
