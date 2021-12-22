using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PatchHanlde.JsonPatch
{
    public class PatchJsonInputFormatterConfiguration : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;

        public PatchJsonInputFormatterConfiguration(ILoggerFactory loggerFactory)
        {
            this._loggerFactory = loggerFactory;
        }

        public void Configure(MvcOptions options)
            => options.InputFormatters.Insert(0, new PatchJsonInputFormatter(_loggerFactory));

    }
}
