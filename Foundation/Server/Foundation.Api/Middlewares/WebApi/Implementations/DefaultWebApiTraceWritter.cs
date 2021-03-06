﻿using System;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Tracing;
using Foundation.Core.Contracts;
using Microsoft.Owin;

namespace Foundation.Api.Middlewares.WebApi.Implementations
{
    public class DefaultWebApiTraceWritter : ITraceWriter
    {
        public virtual void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (request != null && (level == TraceLevel.Fatal || level == TraceLevel.Warn || level == TraceLevel.Error))
            {
                TraceRecord traceRecord = new TraceRecord(request, category, level);
                traceAction(traceRecord);

                IDependencyResolver scopeDependencyResolver = request.GetOwinContext().GetDependencyResolver();

                ILogger logger = scopeDependencyResolver.Resolve<ILogger>();
                IScopeStatusManager scopeStatusManager = scopeDependencyResolver.Resolve<IScopeStatusManager>();

                if (scopeStatusManager.WasSucceeded())
                    scopeStatusManager.MarkAsFailed();

                if (traceRecord.Exception != null)
                {
                    Exception exception = traceRecord.Exception;

                    if (exception is TargetInvocationException && exception.InnerException != null)
                        exception = exception.InnerException;

                    logger.AddLogData("WebExceptionType", exception.GetType().FullName);
                    logger.AddLogData("WebException", exception);
                    logger.AddLogData("WebApiErrorMessage", traceRecord.Message);
                }
            }
        }
    }
}