using Microsoft.AspNetCore.Mvc.Formatters;

using System.Text;

namespace PatchHanlde
{
    public class BaseJsonInputFormatter : TextInputFormatter
    {
        private readonly ILogger<BaseJsonInputFormatter> _logger;

        public BaseJsonInputFormatter(ILogger<BaseJsonInputFormatter> logger)
        {
            this._logger = logger;

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(StandardMediaTypes.ApplicationJson);
            SupportedMediaTypes.Add(StandardMediaTypes.TextJson);
            SupportedMediaTypes.Add(StandardMediaTypes.ApplicationAnyJsonSyntax);
        }

        public override bool CanRead(InputFormatterContext context)
            => context.HttpContext.Request.Method == HttpMethod.Patch.Method;

        public async override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var httpContext = context.HttpContext;
            //var encoding = SelectCharacterEncoding(context);
            var (inputStream, usesTranscodingStream) = GetInputStream(httpContext, encoding);

            var requestContentType = context.HttpContext.Request.ContentType;
            var requestMediaType = string.IsNullOrEmpty(requestContentType) ? default : new MediaType(requestContentType);

            try
            {
                // ...
                throw new Exception($"Blah blah");
            }
            catch (Exception)
            {
                // ...
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
