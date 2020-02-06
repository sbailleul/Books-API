using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.Entities;
using Books.Api.Filters;
using Books.Api.Models.Book;
using Books.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksCollectionsController: ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;

        public BooksCollectionsController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("({bookIds})", Name = nameof(GetBooksCollection))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [BooksResultFilter]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksCollection([ModelBinder(typeof(ArrayModelBinder))]
            IEnumerable<Guid> bookIds)
        {
            var bookEntities =await _booksRepository.GetBooksAsync(bookIds);

            if (bookIds.Count() != bookEntities.Count()) return NotFound();


            return Ok(bookEntities); 
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [BooksResultFilter]
        public async Task<IActionResult> CreateBooksCollection(IEnumerable<BookForCreationDto> booksForCreationDtos)
        {
            var bookEntities = _mapper.Map<IEnumerable<Book>>(booksForCreationDtos);
            foreach (var bookEntity in bookEntities)
            {
                _booksRepository.AddBook(bookEntity);
            }
            await _booksRepository.SaveChangesAsync();
            var bookIds = bookEntities.Select(b => b.Id).ToList();
            var booksToReturn = await _booksRepository.GetBooksAsync(bookIds);
            
            return CreatedAtRoute(nameof(GetBooksCollection), new { bookIds = string.Join(",", bookIds) }, booksToReturn);
        }

    }
}
