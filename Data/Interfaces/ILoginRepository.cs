﻿using Domain.Entities.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface ILoginRepository
    {
        Task<LoginAuxiliarModel> BuscaUsuarioNoSistema(string email);
        Task<bool> InsereRegistroLogin(long IdUsuario, string token);
    }
}
