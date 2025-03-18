using FluentValidation;
using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Exceptions;
using GroomerManager.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GroomerManager.Application.Salon;

public abstract class CreateSalonCommand
{
    public class CreateSalonRequest : IRequest<CreateSalonResponse>
    {
        public required string Name { get; set; }
        // [FromForm]
        public required IFormFile Logo { get; set; }
    }

    public class CreateSalonResponse
    {
        public required Guid SalonId { get; set; }
        public required string LogoPath { get; set; }
        public required string Name { get; set; }
    }
    
    public class Handler : BaseCommandHandler, IRequestHandler<CreateSalonRequest, CreateSalonResponse>
    {
        private readonly IGroomerManagerDbContext _dbContext;
        private readonly IAuthenticationDataProvider _authenticationData;
        private readonly IBlobService _blobService;
        
        public Handler(IGroomerManagerDbContext dbContext, ICurrentSalonProvider currentSalon, IAuthenticationDataProvider authenticationData, IBlobService blobService) : base(dbContext, currentSalon)
        {
            _dbContext = dbContext;
            _authenticationData = authenticationData;
            _blobService = blobService;
        }

        public async Task<CreateSalonResponse> Handle(CreateSalonRequest request, CancellationToken cancellationToken)
        {
            var userId = _authenticationData.GetUserId();

            if (userId == null)
            {
                throw new UnauthorizedException();
            }
            
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
            
            if (user == null)
            {
                throw new ErrorException("UserDoesNotExist");
            }
            
            var isSalonExist = await _dbContext.Salons.AnyAsync(s => s.Name == request.Name, cancellationToken);
            
            if (isSalonExist)
            {
                throw new ErrorException("SalonAlreadyExist");
            }
            
            using Stream stream = request.Logo.OpenReadStream();
            
            var (logoGlobPath, logoId) = await _blobService.UploadAsync(stream, request.Logo.ContentType, cancellationToken);
            
            if (logoGlobPath == null)
            {
                throw new ErrorException("ErrorOccuredDuringBlobLog");
            }
            
            var newSalon = new Domain.Entities.Salon
            {
                Name = request.Name,
                LogoPath = logoGlobPath.ToString(),
                LogoId = logoId,
                OwnerId = user.Id,
                Owner = user,
                IsDefault = false
            };

            _dbContext.Salons.Add(newSalon);
            
            var userSalon = new Domain.Entities.UserSalon
            {
                UserId = user.Id,
                SalonId = newSalon.Id,
                Role = user.Role.Name,
                User = user,
                Salon = newSalon
            };
            
            _dbContext.UserSalons.Add(userSalon);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new CreateSalonResponse
            {
                SalonId = newSalon.Id,
                LogoPath = newSalon.LogoPath,
                Name = newSalon.Name
            };
        }
    }
    
    public class Validator : AbstractValidator<CreateSalonRequest>
    {
        private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const long maxFileSize = 2 * 1024 * 1024; // 2MB
        
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("SalonNameIsRequired")
                .MaximumLength(50).WithMessage("SalonNameMaxLength");

            RuleFor(x => x.Logo)
                .NotNull().WithMessage("SalonLogoIsRequired")
                .Must(IsValidFileExtension).WithMessage("SalonLogoInvalidFormat")
                .Must(IsValidFileSize).WithMessage("SalonLogoFileSizeExceeded");
        }

        private bool IsValidFileExtension(IFormFile? file)
        {
            if (file == null) return false;
            var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        private bool IsValidFileSize(IFormFile? file)
        {
            return file == null || file.Length <= maxFileSize;
        }
    }
}