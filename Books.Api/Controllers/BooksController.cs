using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.Entities;
using Books.Api.ExternalModels;
using Books.Api.Filters;
using Books.Api.Models.Book;
using Books.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController:ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;

        public BooksController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }




        [HttpGet(Name = nameof(GetBooksAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [BooksResultFilter]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksAsync()
        {
            var books = await _booksRepository.GetBooksAsync();
            return Ok(books);
        }




        [HttpGet ("{id}",Name = nameof(GetBookAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [BookWithCoversFilter]
        public async Task<ActionResult<(Book, IEnumerable<BookCover>)>> GetBookAsync(Guid id)
        {
            var book = await _booksRepository.GetBookAsync(id);

            if (book == null) return NotFound();

            var bookCovers = await _booksRepository.GetBookCoversAsync(id);
            // var propertyBag = new Tuple<Book, IEnumerable<BookCoverDto>>(book, bookCovers);
            // (Book, IEnumerable<BookCoverDto>) propertyBag = (book, bookCovers);
            return Ok((book, bookCovers));
        } 

        [HttpPost(Name = nameof(CreateBook))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [BookResultFilter]
        public async Task<IActionResult> CreateBook(BookForCreationDto bookForCreation)
        {
            var bookEntity = _mapper.Map<Book>(bookForCreation);
            _booksRepository.AddBook(bookEntity);
            await _booksRepository.SaveChangesAsync();
            await _booksRepository.GetBookAsync(bookEntity.Id);
            
            return CreatedAtRoute(nameof(GetBookAsync), new {bookEntity.Id}, bookEntity);
        }
    }
}
