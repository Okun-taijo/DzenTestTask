using CommentsApp.Application.Common;
using CommentsApp.Application.Interfaces;
using CommentsApp.Application.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace CommentsApp.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadRoot;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif"];

    public FileService(IWebHostEnvironment env)
    {
        _uploadRoot = Path.Combine(env.ContentRootPath, "Storage/uploads");

        if (!Directory.Exists(_uploadRoot))
            Directory.CreateDirectory(_uploadRoot);
    }

    public async Task ProcessAsync(IFormFile file, Comment comment)
    {
        if (file.Length == 0)
            return;

        var ext = Path.GetExtension(file.FileName).ToLower();

        if (ext == ".txt")
        {
            if (file.Length > FileConstraints.MaxTextFileSize)
                throw new Exception("TXT too large");

            await SaveFile(file, comment);
        }
        else if (FileConstraints.AllowedImageTypes.Contains(file.ContentType))
        {
            await SaveImage(file, comment);
        }
        else
        {
            throw new Exception("Unsupported type");
        }
    }

    private async Task SaveImage(IFormFile file, Comment comment)
    {
        await using var stream = file.OpenReadStream();
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(ext))
            throw new Exception("Only JPG, PNG, GIF allowed");

        using var image = await Image.LoadAsync(stream);

        image.Mutate(x =>
        {
            x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(320, 240)
            });
        });

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(_uploadRoot, fileName);

        await image.SaveAsync(fullPath);

        comment.Attachments.Add(new CommentAttachment
        {
            FileName = file.FileName,
            Path = fileName,
            ContentType = file.ContentType,
            Size = file.Length,
            IsImage = true
        });
    }

    private async Task SaveFile(IFormFile file, Comment comment)
    {
        var fileName = $"{Guid.NewGuid()}.txt";
        var fullPath = Path.Combine(_uploadRoot, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        comment.Attachments.Add(new CommentAttachment
        {
            FileName = file.FileName,
            Path = fileName,
            ContentType = file.ContentType,
            Size = file.Length,
            IsImage = false
        });
    }
}