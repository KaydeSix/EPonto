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
using System.Net;
using System.Net.Mail;

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

                        await _loginRepository.InsereRegistroLogin(usuario.IdUsuario, token);
                        //chaamar aqui o VerificarValidacaoMesAsync
                        return new LoginDTO
                        {
                            Sucesso = true,
                            Mensagem = "Login realizado com sucesso.",
                            IdUsuario = usuario.IdUsuario,
                            Token = token
                        };
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
        
        public async Task<bool>RecuperarSenha(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return false;

                int numero;

                //Gera número aleatório 
                using (var rng = RandomNumberGenerator.Create())
                {
                    byte[] bytes = new byte[4]; // 4 bytes = 32 bits

                    do
                    {
                        rng.GetBytes(bytes);
                        numero = BitConverter.ToInt32(bytes, 0) & int.MaxValue; // Garante número positivo
                    } while (numero < 100000 || numero > 999999);

                    await _loginRepository.SalvaCodigoRecuperacao(numero, email);
                }

                await EnviarEmailAsync(email, "Recuperação de Senha EPonto", $"O código de recuperação é {numero}.");

            }
            catch (Exception ex)
            {
                return false;
            }
            
            return true;
        }
        public async Task<bool> ValidaCodigoRecuperacao(int codigo, string email)
        {
            if (codigo <= 0 || string.IsNullOrEmpty(email))
                return false;

            int resultCodigo = await _loginRepository.BuscaCodigoEmail(email);

            if (resultCodigo == codigo)
                return true;
            
            return false;
        }
        public async Task<bool> AlteraSenhaLogin(string senha, string email)
        {
            if (string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(email))
                return false;

            string senhaHash = HashPassword(senha);

            await _loginRepository.SalvaAlteracaoSenha(senhaHash, email);
            
            return true;
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
        public async Task EnviarEmailAsync(string destinatario, string assunto, string mensagem)
        {
            var remetente = "mkss.contato@gmail.com";
            var senha = "ujcz beja fkia hycw";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, // Pode ser 465 ou 587 (google)
                Credentials = new NetworkCredential(remetente, senha),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(remetente),
                Subject = assunto,
                Body = mensagem,
                IsBodyHtml = true, 
            };

            mailMessage.To.Add(destinatario);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }

}
