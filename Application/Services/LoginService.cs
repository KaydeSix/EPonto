﻿using Application.DTOs;
using Application.Interfaces;
using Data.Interfaces;
using Domain.Entities.Login;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;

        public LoginService(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public async Task<LoginDTO> RealizarLogin(LoginModel dadosInformados)
        {
            string Error = string.Empty;

            if(string.IsNullOrEmpty(dadosInformados.Senha) || string.IsNullOrEmpty(dadosInformados.Email))
                return new LoginDTO { Sucesso = false, Mensagem = "Para efetuar o login, deve ser informado um email e senha." };

            try
            {
                LoginAuxiliarModel usuario = await _loginRepository.BuscaUsuarioNoSistema(dadosInformados.Email);

                if (usuario.IdUsuario > 0)
                {
                    if (VerifyPassword(dadosInformados.Senha, usuario.Senha))
                    {
                        string token = GenerateToken(usuario.IdUsuario.ToString(), dadosInformados.Email, usuario.Senha);

                        if (await _loginRepository.InsereRegistroLogin(usuario.IdUsuario, token))
                        {
                            return new LoginDTO
                            {
                                Sucesso = true,
                                Mensagem = "Login realizado com sucesso.",
                                IdUsuario = usuario.IdUsuario,
                                Token = token
                            };
                        }
                        else
                            Error = "Erro ao realizar login. Entre em contato com o administrador do sistema.";
                    }
                    else
                        Error = "Senha incorreta.";
                }
                else
                    Error = "Email não localizado no sistema.";

                if (!string.IsNullOrEmpty(Error))
                    return new LoginDTO { Sucesso = false, Mensagem = Error };
                else
                    return new LoginDTO();
            }
            catch (Exception ex)
            {
                return new LoginDTO
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }
      
        public string HashPassword(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha);
        }

        public bool VerifyPassword(string senha, string senhaHash)
        {
            return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
        }
        public string GenerateToken(string idUsuario, string email, string senhaHash, int expireMinutes = 60)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(senhaHash));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, idUsuario),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: "suaApi",
                audience: "seusClientes",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
