using Microsoft.AspNetCore.Mvc.Formatters;

using PatchHanlde.JsonPatch;

using System.Text;
using System.Text.Json;

namespace PatchHanlde.JsonPatch
{
    public class PatchJsonInputFormatter : TextInputFormatter
    {
        private readonly ILogger<PatchJsonInputFormatter> _logger;
        private JsonSerializerOptions _options;

        public PatchJsonInputFormatter(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<PatchJsonInputFormatter>();

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(StandardMediaTypes.ApplicationJson);
            SupportedMediaTypes.Add(StandardMediaTypes.TextJson);
            SupportedMediaTypes.Add(StandardMediaTypes.ApplicationAnyJsonSyntax);

            _options = new JsonSerializerOptions()
            { 
                PropertyNameCaseInsensitive = true,
            };

        }

        public override bool CanRead(InputFormatterContext context)
            => context.HttpContext.Request.Method == HttpMethod.Patch.Method;

        public async override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var httpContext = context.HttpContext;
            var (inputStream, usesTranscodingStream) = GetInputStream(httpContext, encoding);

            var requestContentType = context.HttpContext.Request.ContentType;
            var requestMediaType = string.IsNullOrEmpty(requestContentType) ? default : new MediaType(requestContentType);

            try
            {
                var op = await JsonSerializer.DeserializeAsync<Operation[]>(inputStream, _options);
                return await InputFormatterResult.SuccessAsync(op);
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Error retrieving Patch information");
                return InputFormatterResult.Failure();
            }
            finally
            {
                if (usesTranscodingStream)
                {
                    await inputStream.DisposeAsync();
                }
            }
        }

        private (Stream inputStream, bool usesTranscodingStream) GetInputStream(HttpContext httpContext, Encoding encoding)
        {
            if (encoding.CodePage == Encoding.UTF8.CodePage)
            {
                return (httpContext.Request.Body, false);
            }

            var inputStream = Encoding.CreateTranscodingStream(httpContext.Request.Body, encoding, Encoding.UTF8, leaveOpen: true);
            return (inputStream, true);
        }

    }
}
