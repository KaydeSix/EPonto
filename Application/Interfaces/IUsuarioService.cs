﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioDTO> CriarUsuarioAsync(UsuarioModel usuario);
        Task<UsuarioModel> ObterUsuarioPorIdAsync(int id);
        Task<IEnumerable<UsuarioModel>> ListarTodosUsuariosAsync();
        Task<bool> AtualizarUsuarioAsync(UsuarioModel usuario);
        Task<bool> ExcluirUsuarioAsync(int id);
    }
}
