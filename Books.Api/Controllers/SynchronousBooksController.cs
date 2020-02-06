using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Books.Api.Entities;
using Books.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SynchronousBooksController:ControllerBase
    {
        private readonly IBooksRepository _booksRepository;

        public SynchronousBooksController(IBooksRepository booksRepository)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
        }

        [HttpGet(Name = nameof(GetBooks))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public  ActionResult<IEnumerable<Book>> GetBooks()
        {
            var books = _booksRepository.GetBooks();
            return Ok(books);
        }

        [HttpGet("{id}",Name = nameof(GetBook))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public  ActionResult<Book> GetBook(Guid id)
        {
            // var book = _booksRepository.GetBook(id);
            var book = _booksRepository.GetBookAsync(id).Result;

            if (book == null) return NotFound();

            return Ok(book);
        }
    }
}
