using AutoMapper;
using BusinessLogic.Ports;
using Domain.Interface;
using Infrastructure.Interfaces;
using Infrastructure.Dto;
using System;
using System.Collections.Generic;

namespace BusinessLogic.Adapter.User;

public class UserService(IUserRepository repository, IUnitOfWork unitOfWork, IMapper mapper, IFileStorage fileStorage) : IUserService
{
    private readonly IUserRepository _repository = repository;
    private readonly IUnitOfWork _uow = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IFileStorage _fileStorage = fileStorage;

    public async Task<UserDto> Create(UserDto user)
    {
        var entity = _mapper.Map<Domain.Entity.Users>(user);
        await _repository.Create(entity);
        await _uow.Complete();
        return _mapper.Map<UserDto>(entity);
    }

    public async Task<UserDto?> Detail(Guid id)
    {
        var entity = await _repository.Detail(id);
        if (entity == null) return null;

        var dto = _mapper.Map<UserDto>(entity);
        if (!string.IsNullOrEmpty(entity.ProfileImageUrl))
        {
            dto.Base64Image = await _fileStorage.GetBase64ImageAsync(entity.ProfileImageUrl, "profile-images");
        }
        return dto;
    }

    public async Task<IEnumerable<UserDto>> All()
    {
        var list = await _repository.All();
        var dtos = _mapper.Map<List<UserDto>>(list);
        foreach (var dto in dtos)
        {
            if (!string.IsNullOrEmpty(dto.ProfileImageUrl))
            {
                dto.Base64Image = await _fileStorage.GetBase64ImageAsync(dto.ProfileImageUrl, "profile-images");
            }
        }
        return dtos;
    }

    public async Task<UserDto?> Update(Guid id, UpdateUserDto user)
    {
        var entity = await _repository.Detail(id);
        if (entity == null) return null;

        entity.Name = user.Name;
        entity.Email = user.Email;
        entity.Role = user.Role;
        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            entity.PasswordHash = user.PasswordHash;
        }

        if (!string.IsNullOrEmpty(user.Base64Image))
        {
            var fileName = await _fileStorage.SaveBase64ImageAsync(user.Base64Image, "profile-images");
            entity.ProfileImageUrl = fileName;
        }

        await _repository.Update(entity);
        await _uow.Complete();

        var dto = _mapper.Map<UserDto>(entity);
        if (!string.IsNullOrEmpty(entity.ProfileImageUrl))
        {
            dto.Base64Image = await _fileStorage.GetBase64ImageAsync(entity.ProfileImageUrl, "profile-images");
        }
        return dto;
    }

    public async Task Delete(Guid id)
    {
        var entity = await _repository.Detail(id);
        if (entity != null)
        {
            await _repository.Delete(entity);
            await _uow.Complete();
        }
    }
}
