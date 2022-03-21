/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
#if !MRET_2021_OR_LATER
using Microsoft.Extensions.Logging;
using System;
using UnityEngine;

namespace Assets.VDE
{
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class UnityLogger : ILoggerProvider
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new UnityLog();
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            
        }

        public class UnityLog : Microsoft.Extensions.Logging.ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                var id = Guid.NewGuid();
                //Debug.Log($"BeginScope ({id}): {state}");
                return new Scope<TState>(state, id);
            }
            struct Scope<TState> : IDisposable
            {
                public Scope(TState state, Guid id)
                {
                    State = state;
                    Id = id;
                }

                public TState State { get; }
                public Guid Id { get; }

                public void Dispose() { }

                //public void Dispose() => Debug.Log($"EndScope ({Id}): {State}");
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        Debug.Log($"{logLevel}, {eventId}, {state}, {exception}");
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning($"{logLevel}, {eventId}, {state}, {exception}");
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Debug.LogError($"{logLevel}, {eventId}, {state}, {exception}");
                        break;
                    case LogLevel.None: break;
                }
            }
        }
    }
}
#endif