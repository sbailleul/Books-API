using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.Entities;
using Books.Api.ExternalModels;
using Books.Api.Models.Book;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Books.Api.Filters
{
    public class BookWithCoversFilterAttribute: ResultFilterAttribute
    {
        public override async  Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var mapper = context.HttpContext.RequestServices.GetService<IMapper>();
            var result = context.Result as ObjectResult;

            if (result?.Value == null || result.StatusCode < 200 || result.StatusCode >= 300)
            {
                await next();
                return;
            }
            var (book, bookCovers) = ((Book , IEnumerable<BookCover> )) result.Value;

            var bookWithCovers = mapper.Map<BookWithCoversDto>(book);
            // var temp = ((Book , IEnumerable<BookCoverDto> )) result.Value;
            result.Value = mapper.Map(bookCovers, bookWithCovers);
            await next();

        }
    }
}
