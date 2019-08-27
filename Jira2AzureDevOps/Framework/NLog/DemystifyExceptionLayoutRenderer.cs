using NLog.LayoutRenderers;
using System;
using System.Diagnostics;
using System.Text;

namespace Jira2AzureDevOps.Framework.NLog
{
    public class DemystifyExceptionLayoutRenderer : ExceptionLayoutRenderer
    {
        public static void Register()
        {
            LayoutRenderer.Register<DemystifyExceptionLayoutRenderer>("demystify-exception");
        }

        protected override void AppendToString(StringBuilder sb, Exception ex)
        {
            sb.Append(ex.Demystify());
        }
    }
}