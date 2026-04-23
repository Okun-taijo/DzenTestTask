using CommentsApp.Application.Models;
using Microsoft.AspNetCore.Http;

namespace CommentsApp.Application.Interfaces;
public interface IFileService
{
    Task ProcessAsync(IFormFile file, Comment comment);
}