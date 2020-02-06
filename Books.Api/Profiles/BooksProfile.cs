using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.Entities;
using Books.Api.ExternalModels;
using Books.Api.Models.Book;

namespace Books.Api.Profiles
{
    public class BooksProfile: Profile
    {
        public BooksProfile()
        {
            CreateMap<Book, BookDto>().ForMember(
                dst => dst.Author, 
                opt => opt.MapFrom(
                    src => $"{src.Author.FirstName} {src.Author.LastName}")
            );

            CreateMap<BookForCreationDto, Book>();
            CreateMap<BookCover, BookCoverDto>();

            CreateMap<Book, BookWithCoversDto>().ForMember(
                dst => dst.Author,
                opt =>
                    opt.MapFrom(src => $"{src.Author.FirstName} {src.Author.LastName}")
            );

            CreateMap<IEnumerable<ExternalModels.BookCover>, BookWithCoversDto>().ForMember(
                dst => dst.BookCovers,
                opt => opt.MapFrom(
                    src => src));


        }
    }
}
