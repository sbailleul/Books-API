using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Books.Api.ExternalModels;

namespace Books.Api.Models.Book
{
    public class BookWithCoversDto: BookDto
    {
        public IEnumerable<BookCoverDto> BookCovers { get; set; } = new List<BookCoverDto>(); 
    }
}
