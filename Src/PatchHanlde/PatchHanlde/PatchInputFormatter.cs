using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace PatchHanlde
{
    public class PatchInputFormatter : SystemTextJsonInputFormatter
    {
        public PatchInputFormatter(JsonOptions options, ILogger<SystemTextJsonInputFormatter> logger) : base(options, logger)
        {
        }

        //override 
    }
}
