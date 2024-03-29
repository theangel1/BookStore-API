﻿using BookStore_UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_UI.Contracts
{
    public interface IAuthenticationRepository
    {
        Task<bool> Register(RegistrationModel user);
        Task<bool> Login(LoginModel user);
        Task Logout();
    }
}
