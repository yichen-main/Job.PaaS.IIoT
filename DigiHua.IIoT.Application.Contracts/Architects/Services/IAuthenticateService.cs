﻿namespace IIoT.Application.Contracts.Architects.Services;
public interface IAuthenticateService
{
    Task<string> LoginAsync(string account, string password);
    ConcurrentDictionary<string, (string token, IUser.Entity entity)> Validators { get; }
}