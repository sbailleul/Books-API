using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Books.Api.Entities;

namespace Books.Api.Models.Book
{
    public class BookDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }
    }
}
