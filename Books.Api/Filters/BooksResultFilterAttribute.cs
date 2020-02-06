using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.Models.Book;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Books.Api.Filters
{
    public class BooksResultFilterAttribute: ResultFilterAttribute
    {

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var mapper = context.HttpContext.RequestServices.GetService<IMapper>();
            var result = context.Result as ObjectResult;
            if (result?.Value == null || result.StatusCode < 200 || result.StatusCode >= 300)
            {
                await next();
            }
            result.Value = mapper.Map<IEnumerable<BookDto>>(result.Value);

            await next();

        }
    }
}
