using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Books.Api.Contexts;
using Books.Api.Entities;
using Books.Api.ExternalModels;
using Books.Legacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Books.Api.Services
{
    public class BooksRepository: IBooksRepository, IDisposable
    {
        private BooksContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BooksRepository> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public BooksRepository(BooksContext dbContext, IHttpClientFactory httpClientFactory, ILogger<BooksRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BookCover> GetBookCoverAsync(string coverId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://localhost:52644/api/bookcovers/{coverId}");

            if (!response.IsSuccessStatusCode) return null;
            
            return JsonConvert.DeserializeObject<BookCover>(await response.Content.ReadAsStringAsync());

        }

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCover>();
            _cancellationTokenSource = new CancellationTokenSource();

            var urls = new[]
            {
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover5",
            };

            var downloadBookCoverTaskQuery = from url in urls select DownloadBookCoverAsync(httpClient, url, _cancellationTokenSource.Token);
            var downloadBookCoverTasks = downloadBookCoverTaskQuery.ToList();
            try
            {
                return await Task.WhenAll(downloadBookCoverTasks);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation($"{ex.Message}");
                downloadBookCoverTasks.ForEach(
                    t => _logger.LogInformation($"Task {t.Id} has status {t.Status}"));

                return new List<BookCover>();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                throw;
            }
        }

        private async Task<BookCover> DownloadBookCoverAsync(HttpClient httpClient, string url, CancellationToken cancellationToken)
        {
            // throw new Exception("Cannot download book cover");
            var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _cancellationTokenSource.Cancel();
                return null;
            }
            return JsonConvert.DeserializeObject<BookCover>(await response.Content.ReadAsStringAsync());
        }


        public IEnumerable<Book> GetBooks()
        {
            _dbContext.Database.ExecuteSqlRaw("WAITFOR DELAY '00:00:02';");
            return _dbContext.Books.Include(b => b.Author).ToList();
        }

        public Book GetBook(Guid id)
        {
            return  _dbContext.Books.Include(b => b.Author).
                FirstOrDefault(b => b.Id == id);
        }

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            await _dbContext.Database.ExecuteSqlRawAsync("WAITFOR DELAY '00:00:02';");
            return await _dbContext.Books.Include(b => b.Author).ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
        { 
            return await _dbContext.Books.
                Where(b => bookIds.Contains(b.Id)).
                Include(b => b.Author).
                ToListAsync();
        }

        public async Task<Book> GetBookAsync(Guid id)
        {
            _logger.LogInformation($"Thread id when entering GetBookAsync : {Thread.CurrentThread.ManagedThreadId}");
            var bookPages = await GetBookPages();
            return await _dbContext.Books.Include(b => b.Author).
                FirstOrDefaultAsync(b => b.Id == id);
        }

        private Task<int> GetBookPages()
        {
            return Task.Run(() =>
            {
                _logger.LogInformation($"Thread id when calculating amount of pages : {Thread.CurrentThread.ManagedThreadId}");
                return new ComplicatedPageCalculator().CalculateBookPages();
            });
        }

        public void AddBook(Book bookToAdd)
        {
            if (bookToAdd == null) throw new ArgumentNullException(nameof(bookToAdd));

            _dbContext.Books.Add(bookToAdd);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }

        #region Disposing
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_dbContext != null)
            {
                _dbContext.Dispose();
                _dbContext = null;
            }
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }



        #endregion

    }
}
